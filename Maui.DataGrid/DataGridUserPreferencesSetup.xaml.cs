namespace Maui.DataGrid;

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;

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
        ColumnsList.ItemsSource = ColumnsListSource;

        BindingContext = this;

        _CurrentDataGrid = datagrid;
    }

    private void Stepper_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if ((DataGridColumn)((CustomStepper)sender).Tag != null)
        {
            Debug.WriteLine("Stepper_ValueChanged");
            var dataGridColumn = (DataGridColumn)((CustomStepper)sender).Tag;

            dataGridColumn.WidthCol = e.NewValue;
            var DeltaForOtherColumns = (sender as CustomStepper).Increment / (ColumnsListSource.Count - 1);

            //true=ADD, false=REMOVE
            var AddOrRemove = true;
            if (e.NewValue > e.OldValue)
            {
                AddOrRemove = false;
            }

            var numberOfColumnToResize = (ColumnsListSource.Count - 1);

            for (var i = 0; i < ColumnsListSource.Count; i++)
            {
                var col = ColumnsListSource[i];
                if (col != dataGridColumn)
                {
                    if (AddOrRemove)
                    {
                        if (col.WidthCol + DeltaForOtherColumns >= (sender as CustomStepper).Maximum)
                        {
                            numberOfColumnToResize--;
                            DeltaForOtherColumns = (sender as CustomStepper).Increment / numberOfColumnToResize;
                        }
                    }
                    else
                    {
                        if (col.WidthCol - DeltaForOtherColumns <= (sender as CustomStepper).Minimum)
                        {
                            numberOfColumnToResize--;
                            DeltaForOtherColumns = (sender as CustomStepper).Increment / numberOfColumnToResize;
                        }
                    }
                }
            }

            for (var i = 0; i < ColumnsListSource.Count; i++)
            {
                var col = ColumnsListSource[i];
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
                    /*ColumnsListSource[i] = col;*/
                }
            }

            /*var temp = ColumnsList.ItemsSource;
            ColumnsList.ItemsSource = null;
            ColumnsList.ItemsSource = temp;*/

            //_CurrentDataGrid.RefreshCollectionHeader();

            _CurrentDataGrid.Reload();
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        if (!(sender as CheckBox).IsChecked)
        {
            var visibleColumns = 0;
            for (var i = 0; i < ColumnsListSource.Count; i++)
            {
                if (ColumnsListSource[i].IsVisible)
                {
                    visibleColumns++;
                }
            }

            if (visibleColumns > 1)
            {
                (sender as CheckBox).IsChecked = false;
                ((sender as CheckBox).BindingContext as DataGridColumn).IsVisible = false;
            }
            else
            {
                (sender as CheckBox).IsChecked = true;
            }
        }
        else
        {
            ((sender as CheckBox).BindingContext as DataGridColumn).IsVisible = true;
        }
    }

    private void ColumnsList_ReorderCompleted(object sender, EventArgs e)
    {
        var temporaryColumns = new ObservableCollection<DataGridColumn>();

        for (var i = 0; i < ColumnsListSource.Count; i++)
        {
            for (var k = 0; k < _CurrentDataGrid.ColumnsHeader.Count; k++)
            {
                if (ColumnsListSource[i].Title == _CurrentDataGrid.ColumnsHeader[k].Title)
                {
                    temporaryColumns.Add(_CurrentDataGrid.ColumnsHeader[k]);
                }
            }
        }

        _CurrentDataGrid.ColumnsHeader = temporaryColumns;

        //if all columns visible, just reorder
        if (_CurrentDataGrid.ColumnsHeader.Count == _CurrentDataGrid.Columns.Count)
        {
            _CurrentDataGrid.Columns.Clear();
            for (var i = 0; i < _CurrentDataGrid.ColumnsHeader.Count; i++)
            {
                _CurrentDataGrid.Columns.Add(_CurrentDataGrid.ColumnsHeader[i]);
            }
        }
        else
        {
            var tempArray = new DataGridColumn[_CurrentDataGrid.Columns.Count];
            _CurrentDataGrid.Columns.CopyTo(tempArray, 0);
            var temp = tempArray.ToObservableCollection();

            _CurrentDataGrid.Columns.Clear();
            for (var i = 0; i < _CurrentDataGrid.ColumnsHeader.Count; i++)
            {
                _CurrentDataGrid.Columns.Add(_CurrentDataGrid.ColumnsHeader[i]);
                temp.Remove(_CurrentDataGrid.ColumnsHeader[i]);
            }

            foreach (var col in temp)
            {
                _CurrentDataGrid.Columns.Add(col);
            }
        }

        _CurrentDataGrid.Reload();
    }
}
