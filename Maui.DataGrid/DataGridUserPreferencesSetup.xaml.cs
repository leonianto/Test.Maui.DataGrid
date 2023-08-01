namespace Maui.DataGrid;

using System.Collections.ObjectModel;

public partial class DataGridUserPreferencesSetup
{
    public ObservableCollection<DataGridColumn> ColumnsListSource { get; set; } = new ObservableCollection<DataGridColumn>();

    private DataGrid _CurrentDataGrid;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="columns">Collection of the DataGrid Columns</param>
    /// <param name="dataGrid">DataGrid reference</param>
    public DataGridUserPreferencesSetup(ObservableCollection<DataGridColumn> columns, DataGrid datagrid)
    {
        InitializeComponent();

        //foreach (var col in columns)
        //{
        //    if (col.Width.Value < datagrid.MinColumnWidth)
        //    {
        //        col.Width = datagrid.MinColumnWidth;
        //    }
        //}

        ColumnsListSource = columns;

        BindingContext = this;

        _CurrentDataGrid = datagrid;
    }




    private void Stepper_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if ((DataGridColumn)((CustomStepper)sender).Tag != null)
        {
            var dataGridColumn = (DataGridColumn)((CustomStepper)sender).Tag;
            dataGridColumn.Width = new GridLength(e.NewValue, GridUnitType.Star);

            _CurrentDataGrid.Reload();
        }
    }

}
