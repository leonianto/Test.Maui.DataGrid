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
        if (((Stepper)sender).BindingContext != null)
        {

            Debug.WriteLine("Stepper_ValueChanged");
            if (e.OldValue <= (sender as Stepper).Minimum || e.NewValue <= (sender as Stepper).Minimum)
            {
                return;
            }
            var dataGridColumn = (DataGridColumn)((Stepper)sender).BindingContext;

            //Get count of columns not locked
            var numberOfColumnToResize = ColumnsListSource.Where(x => x.IsLocked == false).ToList().Count;

            //dataGridColumn.WidthCol = e.NewValue;

            //var DeltaForOtherColumns = (sender as Stepper).Increment / (numberOfColumnToResize - 1);
            //var DeltaForOtherColumns = (e.OldValue - e.NewValue) / (numberOfColumnToResize - 1);
            //true=ADD, false=REMOVE
            var incrementRemainingColumns = e.OldValue > e.NewValue;


            var DeltaForOtherColumns = 0;
            if (incrementRemainingColumns)
            {
                DeltaForOtherColumns = (int)((e.OldValue - e.NewValue) / (numberOfColumnToResize - 1));
            } else
            {
                DeltaForOtherColumns = (int)((e.NewValue - e.OldValue) / (numberOfColumnToResize - 1));
            }

            for (var i = 0; i < ColumnsListSource.Count; i++)
            {
                var col = ColumnsListSource[i];
                if (col != dataGridColumn && !col.IsLocked)
                {
                    if (incrementRemainingColumns)
                    {
                        if (col.WidthCol + DeltaForOtherColumns >= (sender as Stepper).Maximum)
                        {
                            numberOfColumnToResize--;
                            DeltaForOtherColumns = (int)((e.OldValue - e.NewValue) / numberOfColumnToResize);
                        }
                    }
                    else
                    {
                        if (col.WidthCol - DeltaForOtherColumns <= (sender as Stepper).Minimum)
                        {
                            numberOfColumnToResize--;
                            DeltaForOtherColumns = (int)((e.NewValue - e.OldValue) / numberOfColumnToResize);
                        }
                    }
                }
            }

            for (var i = 0; i < ColumnsListSource.Count; i++)
            {
                var col = ColumnsListSource[i];
                if (col != dataGridColumn && !col.IsLocked)
                {
                    if (incrementRemainingColumns)
                    {
                        if (col.WidthCol + DeltaForOtherColumns < (sender as Stepper).Maximum && col.ColumnStepper != null)
                        {

                            col.ColumnStepper.ValueChanged -= Stepper_ValueChanged;
                            
                            col.WidthCol += DeltaForOtherColumns;
                            col.ColumnStepper.ValueChanged += Stepper_ValueChanged;
                        }
                    }
                    else
                    {
                        if (col.WidthCol - DeltaForOtherColumns > (sender as Stepper).Minimum && col.ColumnStepper != null)
                        {
                            col.ColumnStepper.ValueChanged -= Stepper_ValueChanged;
                            col.WidthCol -= DeltaForOtherColumns;
                            col.ColumnStepper.ValueChanged += Stepper_ValueChanged;
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
        var t = ColumnsList.ItemsSource;
        ColumnsList.ItemsSource = null;
        ColumnsList.ItemsSource = t;
    }

    private void LockButtonClick(object sender, EventArgs e)
    {
        HorizontalStackLayout parent = ((ImageButton)sender).Parent as HorizontalStackLayout;

        if (parent != null)
        {
            Stepper stepper = (Stepper)parent.Children[2];
            if (stepper != null && stepper.BindingContext is DataGridColumn)
            {
                var dataGridColumn = (DataGridColumn)stepper.BindingContext;
                if (dataGridColumn != null)
                {
                    if (dataGridColumn.IsLocked)
                    {
                        dataGridColumn.IsLocked = false;
                        ((ImageButton)sender).Source = "unlock.png";
                    }
                    else
                    {
                        dataGridColumn.IsLocked = true;
                        ((ImageButton)sender).Source = "lock.png";
                    }
                }
            }
        }
    }

    private void ImageButtonLoaded(object sender, EventArgs e)
    {
        
        Debug.WriteLine("Loaded");
        var imageButton = (ImageButton)sender;
        var binding = imageButton.BindingContext;

        if (((DataGridColumn)binding).IsLocked){

            imageButton.Source = "lock.png";
        } else
        {
            imageButton.Source = "unlock.png";
        }

    }


    private void ColumnStepperLoaded(object sender, EventArgs e)
    {
        DataGridColumn dataGridColumn = ((Stepper) sender).BindingContext as DataGridColumn;

        dataGridColumn.ColumnStepper = sender as Stepper;
    }

    private void EntryCompleted(object sender, EventArgs e)
    {
        int newValue = int.Parse(((Entry)sender).Text);
        DataGridColumn dataGridColumn = (DataGridColumn)((Entry)sender).BindingContext;
        dataGridColumn.WidthCol = newValue;
    }
}
