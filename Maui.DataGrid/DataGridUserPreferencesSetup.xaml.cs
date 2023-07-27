namespace Maui.DataGrid;

using System.Collections.ObjectModel;

public partial class DataGridUserPreferencesSetup : ContentPage
{

    private ObservableCollection<DataGridColumn> _ColumnsListSource = new ObservableCollection<DataGridColumn>();
    public ObservableCollection<DataGridColumn> ColumnsListSource
    {
        get => _ColumnsListSource;
        set
        {
            _ColumnsListSource = value;
        }
    }

    public DataGrid CurrentDataGrid;

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

        CurrentDataGrid = datagrid;
    }




    private void Stepper_ValueChanged(object sender, ValueChangedEventArgs e)
    {

        if (e.NewValue != null && (DataGridColumn)(sender as CustomStepper).Tag != null)
        {
            DataGridColumn dataGridColumn = (DataGridColumn)(sender as CustomStepper).Tag;
            dataGridColumn.Width = new GridLength(e.NewValue, GridUnitType.Absolute);

            CurrentDataGrid.Reload();
        }
    }

}
