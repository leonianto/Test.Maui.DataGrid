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

    /// <summary>
    /// Lock/Unlock column size to allow or not resizing
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

    /// <summary>
    /// Load image of the lock/unlock button during control initialization
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImageButtonLoaded(object sender, EventArgs e)
    {

        Debug.WriteLine("Loaded");
        var imageButton = (ImageButton)sender;
        var binding = imageButton.BindingContext;

        if (((DataGridColumn)binding).IsLocked)
        {

            imageButton.Source = "lock.png";
        }
        else
        {
            imageButton.Source = "unlock.png";
        }

    }

    /// <summary>
    /// Set the new widthCol value after user press return on the entry button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EntryCompleted(object sender, EventArgs e)
    {
        int newValue = int.Parse(((Entry)sender).Text);

        if (newValue >= 92 && newValue <= 500)
        {
            DataGridColumn dataGridColumn = (DataGridColumn)((Entry)sender).BindingContext;
            dataGridColumn.WidthCol = newValue;

        }
        else
        {
            //reset entry value if exceed limits
            ((Entry)sender).Text = ((DataGridColumn)((Entry)sender).BindingContext).WidthCol.ToString();
        }
    }
}
