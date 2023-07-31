namespace Maui.DataGrid;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls.Shapes;
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

    private int _draggedElementIndex;
    public Type CurrentType { get; protected set; }
    #endregion Fields

    #region ctor

    public DataGrid()
    {
        InitializeComponent();
        _defaultHeaderStyle = (Style)Resources["DefaultHeaderStyle"];
        _defaultSortIconStyle = (Style)Resources["DefaultSortIconStyle"];

        //! move header when selection changed
        self.PropertyChanged += (s, e) =>
        {
            if (SelectionMode == SelectionMode.Multiple)
            {
                _headerView.Margin = new Thickness(30, 0, 0, 0);
                HideBox.IsVisible = true;
            }
            else if (SelectionMode == SelectionMode.Single)
            {
                _headerView.Margin = new Thickness(0, 0, 0, 0);
                HideBox.IsVisible = false;
            }
        };
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
    /// Scrolls to the row
    /// </summary>
    /// <param name="item">Item to scroll</param>
    /// <param name="position">Position of the row in screen</param>
    /// <param name="animated">animated</param>
    public void ScrollTo(object item, ScrollToPosition position, bool animated = true) => _collectionView.ScrollTo(item, position: position, animate: animated);

    private void SetAutoColumns()
    {

        Debug.WriteLine("SetAutoColumns");
        if (UseAutoColumns)
        {
            if (Columns is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged -= OnColumnsChanged;
            }

            List<DataGridColumn> columnsCopy = new List<DataGridColumn>(Columns);

            Columns.Clear();

            if (columnsCopy.Count > 0)
            {
                //Datagrid already built one time
                foreach (DataGridColumn columncopy in columnsCopy)
                {

                    DataGridColumn column = new DataGridColumn();
                    column.Title = columncopy.Title;
                    column.PropertyName = columncopy.PropertyName;

                    column.Width = columncopy.Width;
                    column.IsVisible = columncopy.IsVisible;
                    //column.DataGrid = this;
                    column.CellTemplate = columncopy.CellTemplate;

                    Columns.Add(column);

                }
            }
            else
            {
                //Datagrid to build
                PropertyInfo[] types = CurrentType?.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo propertyinfo in types)
                {
                    DataGridColumn column = new DataGridColumn();
                    column.Title = propertyinfo.Name;
                    column.PropertyName = propertyinfo.Name;
                    column.CellTemplate = (propertyinfo.PropertyType != typeof(string) &&
                                        propertyinfo.PropertyType != typeof(int) &&
                                        propertyinfo.PropertyType != typeof(DateTime)) ? new DataTemplate(propertyinfo.PropertyType) : null;
                    Columns.Add(column);
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
                if (o != n && self._headerView != null && !self.HeaderBordersVisible)
                {
                    foreach (var child in self._headerView.Children.OfType<View>())
                    {
                        child.BackgroundColor = n;
                    }
                }
            });

    public static readonly BindableProperty FooterBackgroundProperty =
        BindablePropertyExtensions.Create(Colors.White);

    public static readonly BindableProperty BorderColorProperty =
        BindablePropertyExtensions.Create(Colors.Black,
            propertyChanged: (b, _, n) =>
            {
                var self = (DataGrid)b;
                if (self._headerView != null && self.HeaderBordersVisible)
                {
                    self._headerView.BackgroundColor = n;
                }

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
    BindableProperty.Create(nameof(SelectionMode), typeof(SelectionMode), typeof(DataGrid), SelectionMode.None);

    public bool UseAutoColumns { get => (bool)GetValue(UseAutoColumnsProperty); set => SetValue(UseAutoColumnsProperty, value); }

    public static readonly BindableProperty UseAutoColumnsProperty =
        BindableProperty.Create(nameof(UseAutoColumns), typeof(bool), typeof(DataGrid), defaultValue: true,
            propertyChanged: (bo, ov, nv) => (bo as DataGrid).SetAutoColumns());



    public static readonly BindableProperty RowsBackgroundColorPaletteProperty =
        BindablePropertyExtensions.Create<IColorProvider>(new PaletteCollection { Colors.White },
            propertyChanged: (b, _, _) =>
            {
                var self = (DataGrid)b;
                if (self.Columns != null && self.ItemsSource != null)
                {
                    self.Reload();
                }
            });

    public static readonly BindableProperty RowsTextColorPaletteProperty =
        BindablePropertyExtensions.Create<IColorProvider>(new PaletteCollection { Colors.Black },
            propertyChanged: (b, _, _) =>
            {
                var self = (DataGrid)b;
                if (self.Columns != null && self.ItemsSource != null)
                {
                    self.Reload();
                }
            });

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

                self.Reload();
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

                (b as DataGrid)._InitColumns(n);

                //ObservableCollection Tracking
                if (o is INotifyCollectionChanged oldCollection)
                {
                    oldCollection.CollectionChanged -= self.HandleItemsSourceCollectionChanged;
                }

                if (n == null)
                {
                    self.InternalItems = null;
                }
                else
                {
                    if (n is INotifyCollectionChanged newCollection)
                    {
                        newCollection.CollectionChanged += self.HandleItemsSourceCollectionChanged;
                    }

                    var itemsSource = n.Cast<object>().ToList();

                    self.PageCount = (int)Math.Ceiling(itemsSource.Count / (double)self.PageSize);

                    self.SortAndPaginate();
                }

                if (self.SelectedItem != null && self.InternalItems?.Contains(self.SelectedItem) != true)
                {
                    self.SelectedItem = null;
                }
            });

    private void _InitColumns(object n)
    {
        var sourceType = n.GetType();
        if (sourceType.GenericTypeArguments.Length != 1)
        {
            throw new InvalidOperationException("DataGrid collection must be a generic typed collection like List<T>.");
        }

        CurrentType = sourceType.GenericTypeArguments.First();

        var columnsAreReady = Columns?.Any() ?? false;

        SetAutoColumns();
    }

    private void HandleItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SortAndPaginate();

        if (SelectedItem != null && InternalItems?.Contains(SelectedItem) != true)
        {
            SelectedItem = null;
        }
    }

    public static readonly BindableProperty PageCountProperty =
        BindablePropertyExtensions.Create(1,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGrid self && n > 0)
                {
                    if (n > 1)
                    {
                        self._paginationStepper.IsEnabled = true;
                        self._paginationStepper.Maximum = n;
                    }
                    else
                    {
                        self._paginationStepper.IsEnabled = false;
                    }
                }
            });

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

    public static readonly BindableProperty HeaderBordersVisibleProperty =
        BindablePropertyExtensions.Create(true,
            propertyChanged: (b, _, n) => ((DataGrid)b)._headerView.BackgroundColor =
                n ? ((DataGrid)b).BorderColor : ((DataGrid)b).HeaderBackground);

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


    /*public static readonly BindableProperty MaxColumnWidthProperty =
       BindableProperty.Create(nameof(MaxColumnWidth), typeof(double), typeof(DataGrid), 1000);

    public static readonly BindableProperty MinColumnWidthProperty =
       BindableProperty.Create(nameof(MinColumnWidth), typeof(double), typeof(DataGrid), 99);*/

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
    /// BackgroundColor of the footer that comtains pagination elements
    /// Default value is White
    /// </summary>
    public Color FooterBackground
    {
        get => (Color)GetValue(FooterBackgroundProperty);
        set => SetValue(FooterBackgroundProperty, value);
    }

    /// <summary>
    /// Border color
    /// Default Value is Black
    /// </summary>
    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
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
    /// Background color of the rows. It repeats colors consecutively for rows.
    /// </summary>
    public IColorProvider RowsBackgroundColorPalette
    {
        get => (IColorProvider)GetValue(RowsBackgroundColorPaletteProperty);
        set => SetValue(RowsBackgroundColorPaletteProperty, value);
    }

    /// <summary>
    /// Text color of the rows. It repeats colors consecutively for rows.
    /// </summary>
    public IColorProvider RowsTextColorPalette
    {
        get => (IColorProvider)GetValue(RowsTextColorPaletteProperty);
        set => SetValue(RowsTextColorPaletteProperty, value);
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
    public List<int> PageSizeList { get; } = new() { 5, 10, 50, 100, 200, 1000 };

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
    /// Determines to show the borders of header cells.
    /// Default value is <c>true</c>
    /// </summary>
    public bool HeaderBordersVisible
    {
        get => (bool)GetValue(HeaderBordersVisibleProperty);
        set => SetValue(HeaderBordersVisibleProperty, value);
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

    /*/// <summary>
    /// Max Width of the DataGrid Columns
    /// </summary>
    public double MaxColumnWidth
    {
        get => (double)GetValue(MaxColumnWidthProperty);
        set => SetValue(MaxColumnWidthProperty, value);
    }

    /// <summary>
    /// Min Width of the DataGrid Columns
    /// </summary>
    public double MinColumnWidth
    {
        get => (double)GetValue(MinColumnWidthProperty);
        set => SetValue(MinColumnWidthProperty, value);
    }*/

    public double MinColumnWidth { get; set; } = 100;

    public double MaxColumnWidth { get; set; } = 1000;

    #endregion Properties

    #region UI Methods

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (SelectionEnabled)
        {
            if (Parent is null)
            {
                _collectionView.SelectionChanged -= OnSelectionChanged;
            }
            else
            {
                _collectionView.SelectionChanged += OnSelectionChanged;
            }
        }

        if (RefreshingEnabled)
        {
            if (Parent is null)
            {
                _refreshView.Refreshing -= OnRefreshing;
            }
            else
            {
                _refreshView.Refreshing += OnRefreshing;
            }
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

    private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Reload();

    private void OnColumnSizeChanged(object? sender, EventArgs e) => Reload();

    private void OnRefreshing(object? sender, EventArgs e) => _refreshingEventManager.HandleEvent(this, e, nameof(Refreshing));

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedItem = _collectionView.SelectedItem;
        SelectedItems = _collectionView.SelectedItems;

        _itemSelectedEventManager.HandleEvent(this, e, nameof(ItemSelected));
    }

    public void Reload()
    {
        InitHeaderView();

        if (_internalItems is not null)
        {
            InternalItems = new List<object>(_internalItems);
        }
    }

    #endregion UI Methods

    #region Header Creation Methods

    public View GetHeaderViewForColumn(DataGridColumn column)
    {
        column.HeaderLabel.Style = column.HeaderLabelStyle ?? HeaderLabelStyle ?? _defaultHeaderStyle;

        if (!IsSortable || !column.SortingEnabled || !column.IsSortable(this))
        {
            return new ContentView
            {
                Content = column.HeaderLabel,
                GestureRecognizers =
                {
                    new DragGestureRecognizer
                    {

                        DragStartingCommand = new Command((s) =>
                        {
                           _draggedElementIndex = Columns.IndexOf(column);
                        }),


                    },
                    new DropGestureRecognizer
                    {

                        DropCommand = new Command(() =>
                        {
                            int currentIndex = Columns.IndexOf(column);
                            DataGridColumn cellToDrop = Columns[_draggedElementIndex];
                            if (Columns is INotifyCollectionChanged observable)
                                {
                                    observable.CollectionChanged -= OnColumnsChanged;
                                }
                            Columns[_draggedElementIndex] = Columns[currentIndex];
                            Columns[currentIndex] = cellToDrop;
                            if (Columns is INotifyCollectionChanged observable2)
                                {
                                    observable2.CollectionChanged += OnColumnsChanged;
                                }

                            Reload();
                        })
                    }

                }
            };
        }

        var sortIconSize = HeaderHeight * 0.3;
        column.SortingIconContainer.HeightRequest = sortIconSize;
        column.SortingIconContainer.WidthRequest = sortIconSize;
        column.SortingIcon.Style = SortIconStyle ?? _defaultSortIconStyle;

        var grid = new Grid
        {
            ColumnSpacing = 0,
            Padding = new(0, 0, 4, 0),
            ColumnDefinitions = HeaderColumnDefinitions,
            Children = { column.HeaderLabel, column.SortingIconContainer },
            GestureRecognizers =
                {
                    new TapGestureRecognizer
                    {
                        Command = new Command(() =>
                        {
                            // This is to invert SortOrder when the user taps on a column.
                            var order = column.SortingOrder == SortingOrder.Ascendant
                                ? SortingOrder.Descendant
                                : SortingOrder.Ascendant;

                            var index = Columns.IndexOf(column);

                            SortedColumnIndex = new(index, order);

                            column.SortingOrder = order;
                        }, () => column.SortingEnabled)
                    },
                    new DragGestureRecognizer
                    {
                        
                        DragStartingCommand = new Command((s) =>
                        {
                           _draggedElementIndex = Columns.IndexOf(column);
                        }),
                    },
                    new DropGestureRecognizer
                    {
                        DropCommand = new Command(() =>
                        {
                            int currentIndex = Columns.IndexOf(column);
                            DataGridColumn cellToDrop = Columns[_draggedElementIndex];
                            if (Columns is INotifyCollectionChanged observable)
                                {
                                    observable.CollectionChanged -= OnColumnsChanged;
                                }
                            Columns[_draggedElementIndex] = Columns[currentIndex];
                            Columns[currentIndex] = cellToDrop;
                            if (Columns is INotifyCollectionChanged observable2)
                                {
                                    observable2.CollectionChanged += OnColumnsChanged;
                                }

                            Reload();
                        })
                    }

                }

            };

            Grid.SetColumn(column.SortingIconContainer, 1);
            return grid;

    }
    //void OnDragStarting(object sender, DragStartingEventArgs e)
    //{
    //    e.Data.Text = "My text data goes here";
    //}

    private void InitHeaderView()
    {
        Debug.WriteLine("InitHeaderView");
        SetColumnsBindingContext();

        /*_headerView.GestureRecognizers.Clear();

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += Pan_PanUpdated;
        _headerView.GestureRecognizers.Add(pan);

        var ptr = new PointerGestureRecognizer();
        ptr.PointerMoved += Ptr_PointerMoved;
        _headerView.GestureRecognizers.Add(ptr);*/

        _headerView.Children.Clear();
        _headerView.ColumnDefinitions.Clear();
        ResetSortingOrders();

        _headerView.Padding = new(BorderThickness.Left, BorderThickness.Top, BorderThickness.Right, 0);
        _headerView.ColumnSpacing = BorderThickness.HorizontalThickness;

        if (Columns == null)
        {
            return;
        }

        for (var i = 0; i < Columns.Count; i++)
        {
            var col = Columns[i];

            //if (col.Width.Value < MinColumnWidth)
            //{
            //    col.Width = MinColumnWidth;
            //}

            col.ColumnDefinition ??= new(col.Width);

            col.DataGrid ??= this;

            _headerView.ColumnDefinitions.Add(col.ColumnDefinition);

            if (!col.IsVisible)
            {
                continue;
            }

            col.HeaderView ??= GetHeaderViewForColumn(col);

            col.HeaderView.SetBinding(BackgroundColorProperty, new Binding(nameof(HeaderBackground), source: this));

            Grid.SetColumn(col.HeaderView, i);
            _headerView.Children.Add(col.HeaderView);

        }
    }

    /*private int SizingValueArea = 20;
    private double AlreadyAddedX = 0;
    private Grid SelectedColumn = null;
    private bool IsPanning = false;

    private enum SizingPosition
    {
        left,
        right,
        unkown
    }

    private SizingPosition _SizingPosition;

    /// <summary>
    /// Function for reconize in wich column the pointer is
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Ptr_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!IsPanning)
        {
            //Debug.WriteLine("Moving");
            var position = e.GetPosition(_headerView);

            double TotWidth = 0;
            foreach (var col in _headerView.Children)
            {
                TotWidth += (col as Grid).Bounds.Width;

                if (position.Value.X < TotWidth)
                {
                    if (position.Value.X > TotWidth - SizingValueArea && position.Value.X < TotWidth)
                    {
                        _SizingPosition = SizingPosition.right;
                    }
                    else if (position.Value.X > TotWidth - (col as Grid).Bounds.Width && position.Value.X < TotWidth - (col as Grid).Bounds.Width + SizingValueArea)
                    {
                        _SizingPosition = SizingPosition.left;
                    }
                    else
                    {
                        _SizingPosition = SizingPosition.unkown;
                    }

                    if (_SizingPosition != SizingPosition.unkown)
                    {
                        SelectedColumn = (col as Grid);
                    }

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Function for Resize the selected Column
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Pan_PanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        //Debug.WriteLine("Panning");
        //if sizing from the right side of the column
        if (_SizingPosition == SizingPosition.right)
        {
            IsPanning = true;
            if (e.StatusType is GestureStatus.Completed or GestureStatus.Canceled or GestureStatus.Started)
            {
                AlreadyAddedX = 0;
                IsPanning = false;
            }
            var move = e.TotalX *//*+ (e.TotalX / 5)*//* - AlreadyAddedX;

            if (SelectedColumn.Bounds.Width == 1)
            {
                SelectedColumn.WidthRequest = MinColumnWidth;
            }

            var initialWidth = SelectedColumn.Bounds.Width;
            if (initialWidth + move < MaxColumnWidth && initialWidth + move > MinColumnWidth)
            {
                _headerView.ColumnDefinitions[_headerView.Children.IndexOf(SelectedColumn)].Width = initialWidth + move;
                SelectedColumn.WidthRequest = initialWidth + move;
                AlreadyAddedX += move;
            }
        }
        //if sizing from the left side of the column
        else if (_SizingPosition == SizingPosition.left)
        {
            IsPanning = true;
            if (e.StatusType is GestureStatus.Completed or GestureStatus.Canceled or GestureStatus.Started)
            {
                AlreadyAddedX = 0;
                IsPanning = false;
            }
            var move = e.TotalX *//*+ (e.TotalX / 5)*//* - AlreadyAddedX;

            if (_headerView.ColumnDefinitions[_headerView.Children.IndexOf(SelectedColumn) - 1].Width.Value == 1)
            {
                (_headerView.Children[_headerView.Children.IndexOf(SelectedColumn) - 1] as Grid).WidthRequest = MinColumnWidth;
            }

            var initialWidth = _headerView.ColumnDefinitions[_headerView.Children.IndexOf(SelectedColumn) - 1].Width.Value;
            if (initialWidth + move < MaxColumnWidth && initialWidth + move > MinColumnWidth)
            {
                _headerView.ColumnDefinitions[_headerView.Children.IndexOf(SelectedColumn) - 1].Width = initialWidth + move;
                (_headerView.Children[_headerView.Children.IndexOf(SelectedColumn) - 1] as Grid).WidthRequest = initialWidth + move;
                AlreadyAddedX += move;
            }
        }

    }
*/
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


}
