namespace Maui.DataGrid;

public partial class DataGridUserPreferencesSetup : ContentPage
{
    public System.Collections.ObjectModel.ObservableCollection<DataGridColumn> Columns;
    public DataGrid CurrentDataGrid;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="columns">Collection of the DataGrid Columns</param>
    /// <param name="dataGrid">DataGrid reference</param>
    public DataGridUserPreferencesSetup(System.Collections.ObjectModel.ObservableCollection<DataGridColumn> columns, DataGrid dataGrid)
    {
        InitializeComponent();

        CurrentDataGrid = dataGrid;

        foreach (var col in columns)
        {
            if (col.Width.Value < col.DataGrid.MinColumnWidth)
            {
                col.Width = col.DataGrid.MinColumnWidth;
                CurrentDataGrid.GetHeaderViewForColumn(col);
            }
        }
        CurrentDataGrid.Reload();

        Columns = columns;

        ColumnsList.ItemsSource = columns;
        BindingContext = this;
    }

    private void Stepper_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        foreach (var col in Columns)
        {
            if (col == (sender as CustomStepper).Tag)
            {
                col.Width = (sender as CustomStepper).Value;
                CurrentDataGrid.GetHeaderViewForColumn(col);
                break;
            }

            //col.Width = (sender as CustomStepper).Value;
        }
        CurrentDataGrid.Reload();
    }
}
