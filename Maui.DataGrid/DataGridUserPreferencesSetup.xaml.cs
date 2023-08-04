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

        ColumnsListSource = columns;

        BindingContext = this;

        _CurrentDataGrid = datagrid;
    }

    private void Stepper_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if ((DataGridColumn)((CustomStepper)sender).Tag != null)
        {
            var dataGridColumn = (DataGridColumn)((CustomStepper)sender).Tag;
            //dataGridColumn.Width = new GridLength(e.NewValue, GridUnitType.Star);

            dataGridColumn.WidthCol = e.NewValue;
            var DeltaForOtherColumns = (sender as CustomStepper).Increment / (ColumnsListSource.Count - 1);

            //true=ADD, false=REMOVE
            var AddOrRemove = true;
            if (e.NewValue > e.OldValue)
            {
                AddOrRemove = false;
            }

            foreach (var col in ColumnsListSource)
            {
                if (col != dataGridColumn)
                {
                    if (AddOrRemove)
                    {
                        if (col.WidthCol + DeltaForOtherColumns < (sender as CustomStepper).Maximum)
                        {
                            col.WidthCol += DeltaForOtherColumns;
                        }
                    }
                    else
                    {
                        if (col.WidthCol - DeltaForOtherColumns > (sender as CustomStepper).Minimum)
                        {
                            col.WidthCol -= DeltaForOtherColumns;
                        }
                    }
                }
            }

            var temp = ColumnsList.ItemsSource;
            ColumnsList.ItemsSource = null;
            ColumnsList.ItemsSource = temp;

            _CurrentDataGrid.RefreshCollectionHeader();

            _CurrentDataGrid.Reload();
        }
    }

}
