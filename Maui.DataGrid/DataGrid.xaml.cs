namespace Maui.DataGrid;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Maui.Core.Extensions;
using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Mopups.Services;
using Font = Microsoft.Maui.Font;

/// <summary>
/// DataGrid component for Maui
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DataGrid
{

    #region Fields

    private static readonly ColumnDefinitionCollection HeaderColumnDefinitions = new()
                {
                    new() { Width = new(1, GridUnitType.Star) },
                    new() { Width = new(1, GridUnitType.Auto) }
                };

    private readonly WeakEventManager _itemSelectedEventManager = new();
    private readonly WeakEventManager _refreshingEventManager = new();

    private readonly Style _defaultHeaderStyle;
    private readonly Style _defaultSortIconStyle;

    public Type? CurrentType { get; protected set; }
    #endregion Fields

    #region ctor

    public DataGrid()
    {
        InitializeComponent();
        Loaded += DataGrid_Loaded;
        _defaultHeaderStyle = (Style)Resources["DefaultHeaderStyle"];
        _defaultSortIconStyle = (Style)Resources["DefaultSortIconStyle"];

        //! move header when selection changed and platform is windows
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            self.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SelectionMode")
                {
                    if (SelectionMode == SelectionMode.Multiple)
                    {
                        Collectionheader.Margin = new Thickness(28, 0, 0, 0);
                        _HeaderMargin = 28;
                        Resize(30);
                        HideBox.IsVisible = true;
                    }
                    else if (SelectionMode == SelectionMode.Single)
                    {
                        Collectionheader.Margin = new Thickness(0, 0, 0, 0);
                        _HeaderMargin = 0;
                        Resize(-30);
                        HideBox.IsVisible = false;
                    }
                }
            };

            //add the rightClick gesture on the DataGridHeader if it's on Windowss
            var tapGesture = new TapGestureRecognizer() { Buttons = ButtonsMask.Secondary };
            tapGesture.Tapped += TapGestureRecognizer_Tapped;
            Collectionheader.GestureRecognizers.Add(tapGesture);

            DGUserPreferences.IsVisible = false;
            MainGrid.ColumnDefinitions.RemoveAt(0);
        }
    }

    private int _HeaderMargin = 50;

    private void DataGrid_Loaded(object? sender, EventArgs e)
    {
        /*foreach (var column in Columns)
        {
            column.WidthCol = (double)(Width - _HeaderMargin) / (double)Columns.Count;
        }*/
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            Resize(0);
        }
    }

    #endregion ctor

    #region Events

    public event EventHandler<SelectionChangedEventArgs> ItemSelected
    {
        add => _itemSelectedEventManager.AddEventHandler(value);
        remove => _itemSelectedEventManager.RemoveEventHandler(value);
    }

    public event EventHandler Refreshing
    {
        add => _refreshingEventManager.AddEventHandler(value);
        remove => _refreshingEventManager.RemoveEventHandler(value);
    }

    #endregion Events

    #region Sorting methods

    private bool CanSort(SortData? sortData)
    {
        if (sortData is null)
        {
            Console.WriteLine("No sort data");
            return false;
        }

        if (InternalItems is null)
        {
            Console.WriteLine("There are no items to sort");
            return false;
        }

        if (!IsSortable)
        {
            Console.WriteLine("DataGrid is not sortable");
            return false;
        }

        if (Columns.Count < 1)
        {
            Console.WriteLine("There are no columns on this DataGrid.");
            return false;
        }

        if (sortData.Index >= Columns.Count)
        {
            Console.WriteLine("Sort index is out of range");
            return false;
        }

        var columnToSort = Columns[sortData.Index];

        if (columnToSort.PropertyName == null)
        {
            Console.WriteLine($"Please set the {nameof(columnToSort.PropertyName)} of the column");
            return false;
        }

        if (!columnToSort.SortingEnabled)
        {
            Console.WriteLine($"{columnToSort.PropertyName} column does not have sorting enabled");
            return false;
        }

        if (!columnToSort.IsSortable(this))
        {
            Console.WriteLine($"{columnToSort.PropertyName} column is not sortable");
            return false;
        }

        return true;
    }

    private IList<object> GetSortedItems(IList<object> unsortedItems, SortData sortData)
    {
        var columnToSort = Columns[sortData.Index];

        foreach (var column in Columns)
        {
            if (column == columnToSort)
            {
                column.SortingOrder = sortData.Order;
                column.SortingIconContainer.IsVisible = true;
            }
            else
            {
                column.SortingOrder = SortingOrder.None;
                column.SortingIconContainer.IsVisible = false;
            }
        }

        IEnumerable<object> items;

        switch (sortData.Order)
        {
            case SortingOrder.Ascendant:
                items = unsortedItems.OrderBy(x => x.GetValueByPath(columnToSort.PropertyName));
                _ = columnToSort.SortingIcon.RotateTo(0);
                break;
            case SortingOrder.Descendant:
                items = unsortedItems.OrderByDescending(x => x.GetValueByPath(columnToSort.PropertyName));
                _ = columnToSort.SortingIcon.RotateTo(180);
                break;
            case SortingOrder.None:
                return unsortedItems;
            default:
                throw new NotImplementedException();
        }

        return items.ToList();
    }

    #endregion Sorting methods

    #region Pagination methods

    private IEnumerable<object> GetPaginatedItems(IEnumerable<object> unpaginatedItems)
    {
        var skip = (PageNumber - 1) * PageSize;

        return unpaginatedItems.Skip(skip).Take(PageSize);
    }

    private void SortAndPaginate(SortData? sortData = null)
    {
        if (ItemsSource is null)
        {
            return;
        }

        sortData ??= SortedColumnIndex;

        var originalItems = ItemsSource.Cast<object>().ToList();

        IList<object> sortedItems;

        if (sortData != null && CanSort(sortData))
        {
            sortedItems = GetSortedItems(originalItems, sortData);
        }
        else
        {
            sortedItems = originalItems;
        }

        if (PaginationEnabled)
        {
            InternalItems = GetPaginatedItems(sortedItems).ToList();
        }
        else
        {
            InternalItems = sortedItems;
        }
    }

    #endregion Pagination methods

    #region Methods

    /// <summary>
    /// Function for resize columns when changing width
    /// </summary>
    /// <param name="totDelta">if < 0 datagridMargin going from 78 to 50 = -28, if > 0 datagridMargin going from 50 to 78 = +28</param>
    public void Resize(int totDelta)
    {
        Debug.WriteLine("Resize");
        //return to columns with all the same width
        if (totDelta == 0)
        {
            foreach (var column in Columns)
            {
                column.WidthCol = (double)(Width - _HeaderMargin) / (double)ColumnsHeader.Count;
            }
        }
        else
        {
            foreach (var column in Columns)
            {
                column.WidthCol -= totDelta / ColumnsHeader.Count;
            }
        }

        Reload();
    }

    /// <summary>
    /// Scrolls to the row
    /// </summary>
    /// <param name="item">Item to scroll</param>
    /// <param name="position">Position of the row in screen</param>
    /// <param name="animated">animated</param>
    public void ScrollTo(object item, ScrollToPosition position, bool animated = true) => _collectionView.ScrollTo(item, position: position, animate: animated);

    private void SetAutoColumns()
    {
        if (UseAutoColumns)
        {
            Debug.WriteLine("SetAutoColumns");
            if (Columns is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged -= OnColumnsChanged;
            }

            var columnsCopy = new List<DataGridColumn>(Columns);

            Columns.Clear();

            if (columnsCopy.Count > 0)
            {
                //Datagrid already built one time
                foreach (var columncopy in columnsCopy)
                {

                    var column = new DataGridColumn
                    {
                        Title = columncopy.Title,
                        PropertyName = columncopy.PropertyName,
                        WidthCol = columncopy.WidthCol,
                        IsVisible = columncopy.IsVisible,
                        //column.DataGrid = this;
                        CellTemplate = columncopy.CellTemplate
                    };

                    Columns.Add(column);

                }
            }
            else
            {
                //Datagrid to build
                var types = CurrentType?.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                if (types != null)
                {
                    foreach (var propertyinfo in types)
                    {
                        if (propertyinfo != null)
                        {
                            var column = new DataGridColumn
                            {
                                Title = propertyinfo.Name,
                                PropertyName = propertyinfo.Name
                            };

                            if (propertyinfo.PropertyType != typeof(string) &&
                                propertyinfo.PropertyType != typeof(int) &&
                                propertyinfo.PropertyType != typeof(DateTime))
                            {
                                column.CellTemplate = new DataTemplate(propertyinfo.PropertyType);
                            }

                            Columns.Add(column);
                        }
                    }
                }
            }



            InitHeaderView();

            if (Columns is INotifyCollectionChanged obs)
            {
                obs.CollectionChanged += OnColumnsChanged;
            }

        }
    }

    #endregion Methods

    #region Bindable properties

    public static readonly BindableProperty ActiveRowColorProperty =
        BindablePropertyExtensions.Create(Color.FromRgb(128, 144, 160),
            coerceValue: (b, v) =>
            {
                if (!((DataGrid)b).SelectionEnabled)
                {
                    throw new InvalidOperationException("DataGrid must have SelectionEnabled to set ActiveRowColor");
                }

                return v;
            });

    public static readonly BindableProperty HeaderBackgroundProperty =
        BindablePropertyExtensions.Create(Colors.White,
            propertyChanged: (b, o, n) =>
            {
                var self = (DataGrid)b;
            });

    public static readonly BindableProperty DataGridRowColorProperty =
        BindablePropertyExtensions.Create(Colors.Black,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (self.Columns != null && self.ItemsSource != null)
                {
                    self.Reload();
                }
            });

    public static readonly BindableProperty ItemSizingStrategyProperty =
        BindablePropertyExtensions.Create(DeviceInfo.Platform == DevicePlatform.Android ? ItemSizingStrategy.MeasureAllItems : ItemSizingStrategy.MeasureFirstItem);

    public static readonly BindableProperty CanReorderItemsProperty =
   BindableProperty.Create(nameof(CanReorderItems), typeof(bool), typeof(DataGrid), false);

    public static readonly BindableProperty SelectionModeProperty =
    BindableProperty.Create(nameof(SelectionMode), typeof(SelectionMode), typeof(DataGrid), SelectionMode.None,
        propertyChanged: (b, o, n) =>
        {
            var datagrid = b as DataGrid;
            datagrid.SelectedItems?.Clear();
            datagrid.SelectedItem = null;
            datagrid.Reload();
        });

    public bool UseAutoColumns { get => (bool)GetValue(UseAutoColumnsProperty); set => SetValue(UseAutoColumnsProperty, value); }

    public static readonly BindableProperty UseAutoColumnsProperty =
        BindableProperty.Create(nameof(UseAutoColumns), typeof(bool), typeof(DataGrid), defaultValue: false,
            propertyChanged: (bo, ov, nv) => (bo as DataGrid).SetAutoColumns());

    public static readonly BindableProperty ColumnsProperty =
        BindablePropertyExtensions.Create(new ObservableCollection<DataGridColumn>(),
            propertyChanged: (b, o, n) =>
            {
                if (n == o || b is not DataGrid self)
                {
                    return;
                }

                if (o != null)
                {
                    o.CollectionChanged -= self.OnColumnsChanged;

                    foreach (var oldColumn in o)
                    {
                        oldColumn.SizeChanged -= self.OnColumnSizeChanged;
                    }
                }

                if (n != null)
                {
                    n.CollectionChanged += self.OnColumnsChanged;

                    foreach (var newColumn in n)
                    {
                        newColumn.SizeChanged += self.OnColumnSizeChanged;
                    }
                }

                self.ColumnsHeader.Clear();
                for (var i = 0; i < n.Count; i++)
                {
                    if (n[i].IsVisible)
                    {
                        self.ColumnsHeader.Add(n[i]);
                    }
                }

                self.Reload();
            },
            defaultValueCreator: _ => new ObservableCollection<DataGridColumn>());

    public static readonly BindableProperty ColumnsHeaderProperty =
        BindablePropertyExtensions.Create(new ObservableCollection<DataGridColumn>(),
            propertyChanged: (b, o, n) =>
            {
                Debug.WriteLine("ColumnsHeader change");
            },
            defaultValueCreator: _ => new ObservableCollection<DataGridColumn>());

    public static readonly BindableProperty ItemsSourceProperty =
        BindablePropertyExtensions.Create<IEnumerable>(
            propertyChanged: (b, o, n) =>
            {
                if (n == o || b is not DataGrid self)
                {
                    return;
                }

                if (n != null)
                {
                    (b as DataGrid)?._InitColumns(n);
                }

                //ObservableCollection Tracking
                if (o is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= self.OnItemsSourceCollectionChanged;
                }

                if (n == null)
                {
                    self.InternalItems = null;
                }
                else
                {
                    if (n is INotifyCollectionChanged newCollection)
                    {
                        newCollection.CollectionChanged += self.OnItemsSourceCollectionChanged;
                    }

                    var itemsSource = n.Cast<object>().ToList();

                    self.PageCount = (int)Math.Ceiling(itemsSource.Count / (double)self.PageSize);

                    if (self.PageCount == 0)
                    {
                        self.PageCount = 1;
                    }

                    self.SortAndPaginate();
                }

                if (self.SelectedItem != null && self.InternalItems?.Contains(self.SelectedItem) != true)
                {
                    self.SelectedItem = null;
                }
            });

    private void _InitColumns(object n)
    {
        Debug.WriteLine("_InitColumns");
        var sourceType = n.GetType();
        if (sourceType.GenericTypeArguments.Length != 1)
        {
            throw new InvalidOperationException("DataGrid collection must be a generic typed collection like List<T>.");
        }

        CurrentType = sourceType.GenericTypeArguments.First();

        var columnsAreReady = Columns?.Any() ?? false;

        ColumnsHeader.Clear();
        for (var i = 0; i < Columns.Count; i++)
        {
            if (Columns[i].IsVisible)
            {
                ColumnsHeader.Add(Columns[i]);
            }
        }

        SetAutoColumns();
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SortAndPaginate();

        if (SelectedItem != null && InternalItems?.Contains(SelectedItem) != true)
        {
            SelectedItem = null;
        }
    }


    //public static readonly BindableProperty PageCountProperty =
    //    BindablePropertyExtensions.Create(1,
    //        propertyChanged: (b, o, n) =>
    //        {
    //            if (o != n && b is DataGrid self && n > 0)
    //            {
    //               {
    //                   if (n > 1)
    //                   {
    //                       self._paginationStepper.IsEnabled = true;
    //                       self._paginationStepper.Maximum = n;
    //                   }
    //                   else
    //                   {
    //                       self._paginationStepper.IsEnabled = false;
    //                   }
    //               }
    //              
    //            }
    //        });

    public static readonly BindableProperty PageCountProperty =
    BindablePropertyExtensions.Create(1,
        propertyChanged: (b, o, n) =>
        {
            if (o != n && b is DataGrid self && n is int count && count > 0)
            {
                // Assigns the value of the PageCount property to the StepperMaximum property
                self.StepperMaximum = count;
            }
        });

    public static readonly BindableProperty StepperMaximumProperty =
        BindableProperty.Create(nameof(StepperMaximum), typeof(double), typeof(DataGrid), null, BindingMode.TwoWay);


    public static readonly BindableProperty PageSizeProperty =
        BindablePropertyExtensions.Create(100,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    self.PageNumber = 1;
                    if (self.ItemsSource != null)
                    {
                        self.PageCount = (int)Math.Ceiling(self.ItemsSource.Cast<object>().Count() / (double)self.PageSize);
                    }
                    self.SortAndPaginate();
                }
            });

    public static readonly BindableProperty PageSizeVisibleProperty =
        BindablePropertyExtensions.Create(true);

    public static readonly BindableProperty RowHeightProperty =
        BindablePropertyExtensions.Create(40);

    public static readonly BindableProperty FooterHeightProperty =
        BindablePropertyExtensions.Create(DeviceInfo.Platform == DevicePlatform.Android ? 50 : 40);

    public static readonly BindableProperty HeaderHeightProperty =
        BindablePropertyExtensions.Create(40);

    public static readonly BindableProperty IsSortableProperty =
        BindablePropertyExtensions.Create(true);

    public static readonly BindableProperty FontSizeProperty =
        BindablePropertyExtensions.Create(13.0);

    public static readonly BindableProperty FontFamilyProperty =
        BindablePropertyExtensions.Create(Font.Default.Family);

    public static readonly BindableProperty SelectedItemProperty =
        BindablePropertyExtensions.Create<object>(null, BindingMode.TwoWay,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (self._collectionView.SelectedItem != n)
                {
                    self._collectionView.SelectedItem = n;
                }
            },
            coerceValue: (b, v) =>
            {
                if (v is null || b is not DataGrid self)
                {
                    return null;
                }

                if (!self.SelectionEnabled)
                {
                    throw new InvalidOperationException("DataGrid must have SelectionEnabled=true to set SelectedItem");
                }

                if (self.InternalItems?.Contains(v) == true)
                {
                    return v;
                }

                return null;
            }
        );

    public static readonly BindableProperty SelectedItemsProperty =
    BindableProperty.Create(nameof(SelectedItems), typeof(IList<object>), typeof(DataGrid), null, BindingMode.TwoWay,
        propertyChanged: (b, _, n) =>
        {
            var self = (DataGrid)b;
            if (self._collectionView != null)
            {
                if (self._collectionView.SelectedItems != (IList<object>)n)
                {
                    self._collectionView.SelectedItems = (IList<object>)n;
                }
            }
        }

    );

    public static readonly BindableProperty PaginationEnabledProperty =
        BindablePropertyExtensions.Create(false,
            propertyChanged: (b, o, n) =>
            {
                if (o != n)
                {
                    var self = (DataGrid)b;
                    self.SortAndPaginate();
                }
            });

    public static readonly BindableProperty SelectionEnabledProperty =
        BindablePropertyExtensions.Create(true,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && !n)
                {
                    var self = (DataGrid)b;
                    self.SelectedItem = null;
                }
            });

    public static readonly BindableProperty RefreshingEnabledProperty =
        BindablePropertyExtensions.Create(true,
            propertyChanged: (b, o, n) =>
            {
                if (o != n)
                {
                    var self = (DataGrid)b;
                    _ = self.PullToRefreshCommand?.CanExecute(() => n);
                }
            });

    public static readonly BindableProperty PullToRefreshCommandProperty =
        BindablePropertyExtensions.Create<ICommand>(
            propertyChanged: (b, o, n) =>
            {
                if (o == n || b is not DataGrid self)
                {
                    return;
                }

                if (n == null)
                {
                    self._refreshView.Command = null;
                }
                else
                {
                    self._refreshView.Command = n;
                    _ = self._refreshView.Command?.CanExecute(self.RefreshingEnabled);
                }
            });

    public static readonly BindableProperty IsRefreshingProperty =
        BindablePropertyExtensions.Create(false, BindingMode.TwoWay);

    public static readonly BindableProperty BorderThicknessProperty =
        BindablePropertyExtensions.Create(new Thickness(1),
            propertyChanged: (b, _, _) =>
            {
                if (b is DataGrid self)
                {
                    self.Reload();
                }
            });

    public static readonly BindableProperty SortedColumnIndexProperty =
        BindablePropertyExtensions.Create<SortData>(null, BindingMode.TwoWay,
            (b, v) =>
            {
                var self = (DataGrid)b;

                if (!self.IsLoaded)
                {
                    return true;
                }

                return self.CanSort(v);
            },
            (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    self.SortAndPaginate(n);
                }
            });

    public static readonly BindableProperty PageNumberProperty =
        BindablePropertyExtensions.Create(1, BindingMode.TwoWay,
            (b, v) =>
            {
                if (b is DataGrid self)
                {
                    return v == 1 || v <= self.PageCount;
                }

                return false;
            },
            (b, o, n) =>
            {
                if (o != n && b is DataGrid self && self.ItemsSource?.Cast<object>().Any() == true)
                {
                    self.SortAndPaginate();
                }
            });

    public static readonly BindableProperty HeaderLabelStyleProperty =
        BindablePropertyExtensions.Create<Style>();

    public static readonly BindableProperty SortIconProperty =
        BindablePropertyExtensions.Create<Polygon>();

    public static readonly BindableProperty SortIconStyleProperty =
        BindablePropertyExtensions.Create<Style>(
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    foreach (var column in self.Columns)
                    {
                        column.SortingIcon.Style = n;
                    }
                }
            });

    public static readonly BindableProperty NoDataViewProperty =
        BindablePropertyExtensions.Create<View>(
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self)
                {
                    self._collectionView.EmptyView = n;
                }
            });

    #endregion Bindable properties

    #region Properties

    /// <summary>
    /// Selected Row color
    /// </summary>
    public Color ActiveRowColor
    {
        get => (Color)GetValue(ActiveRowColorProperty);
        set => SetValue(ActiveRowColorProperty, value);
    }

    /// <summary>
    /// BackgroundColor of the column header
    /// Default value is White
    /// </summary>
    public Color HeaderBackground
    {
        get => (Color)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>
    /// Border color
    /// Default Value is Black
    /// </summary>
    public Color DataGridRowColor
    {
        get => (Color)GetValue(DataGridRowColorProperty);
        set => SetValue(DataGridRowColorProperty, value);
    }

    /// <summary>
    /// ItemSizingStrategy
    /// Default Value is MeasureFirstItem, except on Android
    /// </summary>
    public ItemSizingStrategy ItemSizingStrategy
    {
        get => (ItemSizingStrategy)GetValue(ItemSizingStrategyProperty);
        set => SetValue(ItemSizingStrategyProperty, value);
    }

    /// <summary>
    /// ItemSizingStrategy
    /// Default Value is MeasureFirstItem, except on Android
    /// </summary>
    public bool CanReorderItems
    {
        get => (bool)GetValue(CanReorderItemsProperty);
        set => SetValue(CanReorderItemsProperty, value);
    }

    /// <summary>
    /// SelectionMode
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// ItemsSource of the DataGrid
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private IList<object>? _internalItems;

    internal IList<object>? InternalItems
    {
        get => _internalItems;
        set
        {
            if (_internalItems != value)
            {
                _internalItems = value;
                _collectionView.ItemsSource = _internalItems; // TODO: Are we using the most efficient CollectionChanged handling with observables?
            }
        }
    }

    /// <summary>
    /// Columns
    /// </summary>
    public ObservableCollection<DataGridColumn> Columns
    {
        get => (ObservableCollection<DataGridColumn>)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public ObservableCollection<DataGridColumn> ColumnsHeader
    {
        get => (ObservableCollection<DataGridColumn>)GetValue(ColumnsHeaderProperty);
        set => SetValue(ColumnsHeaderProperty, value);
    }


    /// <summary>
    /// Font size of the cells.
    /// It does not sets header font size. Use <c>HeaderLabelStyle</c> to set header font size.
    /// </summary>
    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Sets the font family.
    /// It does not sets header font family. Use <c>HeaderLabelStyle</c> to set header font size.
    /// </summary>
    public string FontFamily
    {
        get => (string)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>
    /// List of page sizes
    /// </summary>
    public List<int> PageSizeList { get; } = new() { 5, 10, 25, 50, 100, 200, 1000 };

    /// <summary>
    /// Gets or sets whether the page size picker is visible
    /// </summary>
    public bool PageSizeVisible
    {
        get => (bool)GetValue(PageSizeVisibleProperty);
        set => SetValue(PageSizeVisibleProperty, value);
    }

    /// <summary>
    /// Sets the row height
    /// </summary>
    public int RowHeight
    {
        get => (int)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets footer height
    /// </summary>
    public int FooterHeight
    {
        get => (int)GetValue(FooterHeightProperty);
        set => SetValue(FooterHeightProperty, value);
    }

    /// <summary>
    /// Sets header height
    /// </summary>
    public int HeaderHeight
    {
        get => (int)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets if the grid is sortable. Default value is true.
    /// Sortable columns must implement <see cref="IComparable"/>
    /// If you want to enable or disable sorting for specific column please use <c>SortingEnabled</c> property
    /// </summary>
    public bool IsSortable
    {
        get => (bool)GetValue(IsSortableProperty);
        set => SetValue(IsSortableProperty, value);
    }

    /// <summary>
    /// Gets or sets the page number. Default value is 1
    /// </summary>
    public int PageNumber
    {
        get => (int)GetValue(PageNumberProperty);
        set => SetValue(PageNumberProperty, value);
    }

    /// <summary>
    /// Enables pagination in dataGrid. Default value is False
    /// </summary>
    public bool PaginationEnabled
    {
        get => (bool)GetValue(PaginationEnabledProperty);
        set => SetValue(PaginationEnabledProperty, value);
    }

    /// <summary>
    /// Enables selection in dataGrid. Default value is True
    /// </summary>
    public bool SelectionEnabled
    {
        get => (bool)GetValue(SelectionEnabledProperty);
        set => SetValue(SelectionEnabledProperty, value);
    }

    /// <summary>
    /// Selected item
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Selected item
    /// </summary>
    public IList<object>? SelectedItems
    {
        get => (IList<object>?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// Executes the command when refreshing via pull
    /// </summary>
    public ICommand PullToRefreshCommand
    {
        get => (ICommand)GetValue(PullToRefreshCommandProperty);
        set => SetValue(PullToRefreshCommandProperty, value);
    }

    /// <summary>
    /// Displays an ActivityIndicator when is refreshing
    /// </summary>
    public bool IsRefreshing
    {
        get => (bool)GetValue(IsRefreshingProperty);
        set => SetValue(IsRefreshingProperty, value);
    }

    /// <summary>
    /// Enables refreshing the DataGrid by a pull down command
    /// </summary>
    public bool RefreshingEnabled
    {
        get => (bool)GetValue(RefreshingEnabledProperty);
        set => SetValue(RefreshingEnabledProperty, value);
    }

    /// <summary>
    /// Border thickness for header &amp; each cell
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Column index and sorting order for the DataGrid
    /// </summary>
    public SortData? SortedColumnIndex
    {
        get => (SortData?)GetValue(SortedColumnIndexProperty);
        set => SetValue(SortedColumnIndexProperty, value);
    }

    /// <summary>
    /// Style of the header label.
    /// Style's <c>TargetType</c> must be Label.
    /// </summary>
    public Style HeaderLabelStyle
    {
        get => (Style)GetValue(HeaderLabelStyleProperty);
        set => SetValue(HeaderLabelStyleProperty, value);
    }

    /// <summary>
    /// Sort icon
    /// </summary>
    public Polygon SortIcon
    {
        get => (Polygon)GetValue(SortIconProperty);
        set => SetValue(SortIconProperty, value);
    }

    /// <summary>
    /// Style of the sort icon
    /// Style's <c>TargetType</c> must be Polygon.
    /// </summary>
    public Style SortIconStyle
    {
        get => (Style)GetValue(SortIconStyleProperty);
        set => SetValue(SortIconStyleProperty, value);
    }

    /// <summary>
    /// View to show when there is no data to display
    /// </summary>
    public View NoDataView
    {
        get => (View)GetValue(NoDataViewProperty);
        set => SetValue(NoDataViewProperty, value);
    }

    /// <summary>
    /// Gets the page count
    /// </summary>
    public int PageCount
    {
        get => (int)GetValue(PageCountProperty);
        private set => SetValue(PageCountProperty, value);
    }

    /// <summary>
    /// Gets maximum value for pagination stepper
    /// </summary>
    public double StepperMaximum
    {
        get => (double)GetValue(StepperMaximumProperty);
        set => SetValue(StepperMaximumProperty, value);
    }

    #endregion Properties

    #region UI Methods

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent is null)
        {
            _collectionView.SelectionChanged -= OnSelectionChanged;
        }
        else if (SelectionEnabled)
        {
            _collectionView.SelectionChanged += OnSelectionChanged;
        }

        if (Parent is null)
        {
            _refreshView.Refreshing -= OnRefreshing;
        }
        else if (RefreshingEnabled)
        {
            _refreshView.Refreshing += OnRefreshing;
        }

        if (Parent is null)
        {
            Columns.CollectionChanged -= OnColumnsChanged;

            foreach (var column in Columns)
            {
                column.SizeChanged -= OnColumnSizeChanged;
            }
        }
        else
        {
            Columns.CollectionChanged += OnColumnsChanged;

            foreach (var column in Columns)
            {
                column.SizeChanged += OnColumnSizeChanged;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        InitHeaderView();
    }

    private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Debug.WriteLine("OnColumnsChanged");
        /*Reload();*/
    }

    private void OnColumnSizeChanged(object? sender, EventArgs e)
    {
        Debug.WriteLine("OnColumnSizeChanged");
        /*Reload();*/
    }

    private void OnRefreshing(object? sender, EventArgs e)
    {
        _refreshingEventManager.HandleEvent(this, e, nameof(Refreshing));
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedItem = _collectionView.SelectedItem;
        SelectedItems = _collectionView.SelectedItems;

        _itemSelectedEventManager.HandleEvent(this, e, nameof(ItemSelected));
    }

    public void Reload()
    {
        Debug.WriteLine("Reload");

        InitHeaderView();

        if (_internalItems is not null)
        {
            InternalItems = new List<object>(_internalItems);
        }
    }

    #endregion UI Methods

    #region Header Creation Methods

    private void InitHeaderView()
    {
        Debug.WriteLine("InitHeaderView");
        SetColumnsBindingContext();

        ResetSortingOrders();

        if (Columns == null)
        {
            return;
        }

        for (var i = 0; i < Columns.Count; i++)
        {
            var col = Columns[i];

            col.ColumnDefinition ??= new(col.WidthCol);

            col.DataGrid ??= this;

            if (!col.IsVisible)
            {
                continue;
            }
        }
    }

    private void ResetSortingOrders()
    {
        foreach (var column in Columns)
        {
            column.SortingOrder = SortingOrder.None;
        }
    }

    private void SetColumnsBindingContext()
    {
        if (Columns != null)
        {
            foreach (var c in Columns)
            {
                c.BindingContext = BindingContext;
            }
        }
    }

    #endregion Header Creation Methods

    private void DataGridUserPreferencesClick(object sender, EventArgs e)
    {
        // Navigation.PushAsync(new DataGridUserPreferencesSetup(Columns, this));
        MopupService.Instance.PushAsync(new DataGridUserPreferencesSetup(Columns, this));
    }

    /// <summary>
    /// Function for Search in the given string in the specified columns of the dataGrid
    /// </summary>
    /// <typeparam name="T">Generic Type of the List objects</typeparam>
    /// <param name="list">List of objects where to search</param>
    /// <param name="searchTerm">String to research</param>
    /// <param name="propertyNames">Name of the property where to search</param>
    /// <returns>List of objects that match the research</returns>
    public static List<T> Search<T>(List<T> list, string searchTerm, params string[] propertyNames)
    {
        /*if (propertyNames == null || propertyNames.Length == 0)
        {
            propertyNames = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .Select(property => property.Name)
                                     .ToArray();

            // If no property names are specified, use all searchable properties
            propertyNames = list[0].GetSearchableList().ToArray();
        }*/

        var searchableProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                            .Where(property => propertyNames.Contains(property.Name))
                                            .ToArray();

        var resultList = list.Where(item => searchableProperties
                                            .Any(property => property.GetValue(item)?.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                             .ToList();

        return resultList;
    }

    /// <summary>
    /// Function for sort the itemsSource of the Datagrid on the selected column datas
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        var column = ((sender as Border).BindingContext as DataGridColumn);

        if (column.IsSortable(this))
        {
            var sortIconSize = HeaderHeight * 0.3;
            column.SortingIconContainer.HeightRequest = sortIconSize;
            column.SortingIconContainer.WidthRequest = sortIconSize;
            column.SortingIcon.Style = SortIconStyle ?? _defaultSortIconStyle;

            // This is to invert SortOrder when the user taps on a column.
            var order = column.SortingOrder == SortingOrder.Ascendant
                ? SortingOrder.Descendant
                : SortingOrder.Ascendant;

            var index = Columns.IndexOf(column);

            SortedColumnIndex = new(index, order);

            column.SortingOrder = order;
            (column.SortingIconContainer as ContentView).Content = column.SortingIcon;
            try
            {
                ((sender as Border).Content as Grid).Children[1] = column.SortingIconContainer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        /*if (column.IsSortable(this))
        {
            var sortIconSize = HeaderHeight * 0.3;
            (((sender as Border).Content as Grid).Children[1] as ContentView).HeightRequest = sortIconSize;
            (((sender as Border).Content as Grid).Children[1] as ContentView).WidthRequest = sortIconSize;
            column.SortingIcon.Style = SortIconStyle ?? _defaultSortIconStyle;

            // This is to invert SortOrder when the user taps on a column.
            var order = column.SortingOrder == SortingOrder.Ascendant
                ? SortingOrder.Descendant
                : SortingOrder.Ascendant;

            var index = Columns.IndexOf(column);

            SortedColumnIndex = new(index, order);

            column.SortingOrder = order;

            (((sender as Border).Content as Grid).Children[1] as ContentView).Content = column.SortingIcon;
        }*/
    }

    /// <summary>
    /// Function for re order the DataGridColumns after they were moved from the header
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Collectionheader_ReorderCompleted(object sender, EventArgs e)
    {
        //if all columns visible, just reorder
        if (ColumnsHeader.Count == Columns.Count)
        {
            Columns.Clear();
            for (var i = 0; i < ColumnsHeader.Count; i++)
            {
                Columns.Add(ColumnsHeader[i]);
            }
        }
        else
        {
            var tempArray = new DataGridColumn[Columns.Count];
            Columns.CopyTo(tempArray, 0);
            var temp = tempArray.ToObservableCollection();

            Columns.Clear();
            for (var i = 0; i < ColumnsHeader.Count; i++)
            {
                Columns.Add(ColumnsHeader[i]);
                temp.Remove(ColumnsHeader[i]);
            }

            foreach (var col in temp)
            {
                Columns.Add(col);
            }
        }

        Reload();
    }

    /// <summary>
    /// Function for enter in the setup menu when user rightClicks on the DataGridHeader
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        MopupService.Instance.PushAsync(new DataGridUserPreferencesSetup(Columns, this));
    }
}
