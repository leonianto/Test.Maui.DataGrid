namespace Maui.DataGrid;

using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;

/// <summary>
/// Specifies each column of the DataGrid.
/// </summary>
public sealed class DataGridColumn : BindableObject, IDefinition, INotifyPropertyChanged
{
    #region Fields

    private bool? _isSortable;
    private ColumnDefinition? _columnDefinition;
    private readonly ColumnDefinition _invisibleColumnDefinition = new(0);
    private readonly WeakEventManager _sizeChangedEventManager = new();

    #endregion Fields

    public DataGridColumn()
    {
        HeaderLabel = new();
        SortingIcon = new();
        SortingIconContainer = new ContentView
        {
            IsVisible = false,
            //Content = SortingIcon,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
        };
        IsLocked = false;
    }

    #region Events

    public event EventHandler SizeChanged
    {
        add => _sizeChangedEventManager.AddEventHandler(value);
        remove => _sizeChangedEventManager.RemoveEventHandler(value);
    }

    /// <summary>
    /// Parameter class for size changed event
    /// </summary>
    public class SizeChangedEventArgs : EventArgs
    {
        public double OldSize { get; set; }
        public double NewSize { get; set; }
    }

    public event EventHandler ColumnVisibilityChanged;

    #endregion Events

    #region Bindable Properties

    public static readonly BindableProperty WidthColProperty =
        BindablePropertyExtensions.Create<double>(
        propertyChanged: (b, o, n) =>
        {
            if (!o.Equals(n) && b is DataGridColumn self)
            {

                self.ColumnDefinition = new(n);
                self.OnSizeChanged(new SizeChangedEventArgs() { OldSize = o, NewSize = n });

            }
        });

    public double WidthCol
    {
        get => (double)GetValue(WidthColProperty);
        set { SetValue(WidthColProperty, value); }
    }

    public static readonly BindableProperty TitleProperty =
        BindablePropertyExtensions.Create(string.Empty,
            propertyChanged: (b, _, n) => ((DataGridColumn)b).HeaderLabel.Text = n);

    public static readonly BindableProperty PropertyNameProperty =
        BindablePropertyExtensions.Create<string>();

    public static readonly BindableProperty IsVisibleProperty =
        BindablePropertyExtensions.Create(true,
            propertyChanged: (b, o, n) =>
            {
                if (o != n && b is DataGridColumn column)
                {
                    Debug.WriteLine("IsVisiblePropertyChange");
                    try
                    {
                        if (n)
                        {
                            for (var i = 0; i < column.DataGrid.Columns.Count; i++)
                            {
                                if (column.DataGrid.Columns[i].PropertyName == column.PropertyName)
                                {
                                    column.DataGrid.Columns.RemoveAt(i);
                                    column.DataGrid.Columns.Add(column);
                                    column.DataGrid.ColumnsHeader.Add(column);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            column.DataGrid.ColumnsHeader.Remove(column);
                        }
                        column.DataGrid.Resize(0);
                        column.ColumnVisibilityChanged?.Invoke(column, new EventArgs());
                    }
                    catch { }
                    /*finally
                    {
                        column.OnSizeChanged();
                    }*/
                }
            });

    public static readonly BindableProperty StringFormatProperty =
        BindablePropertyExtensions.Create<string>();

    public static readonly BindableProperty CellTemplateProperty =
        BindablePropertyExtensions.Create<DataTemplate>();

    public static readonly BindableProperty LineBreakModeProperty =
        BindablePropertyExtensions.Create(LineBreakMode.WordWrap);

    public static readonly BindableProperty HorizontalContentAlignmentProperty =
        BindablePropertyExtensions.Create(LayoutOptions.Center);

    public static readonly BindableProperty VerticalContentAlignmentProperty =
        BindablePropertyExtensions.Create(LayoutOptions.Center);

    public static readonly BindableProperty SortingEnabledProperty =
        BindablePropertyExtensions.Create(true);

    #endregion Bindable Properties

    #region Properties

    public DataGrid? DataGrid { get; set; }

    internal ColumnDefinition? ColumnDefinition
    {
        get
        {
            if (!IsVisible)
            {
                return _invisibleColumnDefinition;
            }

            return _columnDefinition;
        }
        set => _columnDefinition = value;
    }

    /// <summary>
    /// Column title
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Property name to bind in the object
    /// </summary>
    public string PropertyName
    {
        get => (string)GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    /// <summary>
    /// Is this column visible?
    /// </summary>
    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>
    /// String format for the cell
    /// </summary>
    public string StringFormat
    {
        get => (string)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    /// <summary>
    /// Cell template. Default value is <c>Label</c> with binding <c>PropertyName</c>
    /// </summary>
    public DataTemplate CellTemplate
    {
        get => (DataTemplate)GetValue(CellTemplateProperty);
        set => SetValue(CellTemplateProperty, value);
    }

    /// <summary>
    /// LineBreakModeProperty for the text. WordWrap by default.
    /// </summary>
    public LineBreakMode LineBreakMode
    {
        get => (LineBreakMode)GetValue(LineBreakModeProperty);
        set => SetValue(LineBreakModeProperty, value);
    }

    /// <summary>
    /// Horizontal alignment of the cell content
    /// </summary>
    public LayoutOptions HorizontalContentAlignment
    {
        get => (LayoutOptions)GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Vertical alignment of the cell content
    /// </summary>
    public LayoutOptions VerticalContentAlignment
    {
        get => (LayoutOptions)GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Defines if the column is sortable. Default is true
    /// Sortable columns must implement <see cref="IComparable"/>
    /// </summary>
    public bool SortingEnabled
    {
        get => (bool)GetValue(SortingEnabledProperty);
        set => SetValue(SortingEnabledProperty, value);
    }

    internal Polygon SortingIcon { get; }
    internal Label HeaderLabel { get; }
    //internal View SortingIconContainer { get; }

    public ContentView SortingIconContainer
    {
        get => (ContentView)GetValue(SortingIconContainerProperty);
        set => SetValue(SortingIconContainerProperty, value);
    }

    public static readonly BindableProperty SortingIconContainerProperty = BindablePropertyExtensions.Create(new ContentView(), BindingMode.TwoWay);

    internal SortingOrder SortingOrder { get; set; }

    public bool IsLocked { get; set; }
    public Stepper ColumnStepper { get; set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Determines via reflection if the column's data type is sortable.
    /// If you want to disable sorting for specific column please use <c>SortingEnabled</c> property
    /// </summary>
    /// <param name="dataGrid"></param>
    public bool IsSortable(DataGrid dataGrid)
    {
        if (_isSortable is not null)
        {
            return _isSortable.Value;
        }

        try
        {
            var listItemType = dataGrid.ItemsSource.GetType().GetGenericArguments().Single();
            var columnDataType = listItemType.GetProperty(PropertyName)?.PropertyType;

            if (columnDataType is not null)
            {
                _isSortable = typeof(IComparable).IsAssignableFrom(columnDataType);
            }
        }
        catch
        {
            _isSortable = false;
        }

        return _isSortable ?? false;
    }

    private void OnSizeChanged(SizeChangedEventArgs sizeChangedEventArgs) => _sizeChangedEventManager.HandleEvent(this, sizeChangedEventArgs, nameof(SizeChanged));

    #endregion Methods

}
