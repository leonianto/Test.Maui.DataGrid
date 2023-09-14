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
        ColumnsList.ItemsSource = ColumnsListSource;

        BindingContext = this;

        _CurrentDataGrid = datagrid;
    }

    /// <summary>
    /// Function for refresh the collectionview
    /// </summary>
    public void Refresh()
    {
        if (ColumnsList.IsLoaded)
        {
            ColumnsList.ItemsSource = null;
            GC.Collect();
            ColumnsList.ItemsSource = ColumnsListSource;
        }
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
            CustomStepper stepper = (CustomStepper)parent.Children[2];
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
    /// Change visibility of a column only if there is more than 1 column visible (on checkbox value change)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ColumnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        //if new value == false
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
            //if there is more than 1 column visible keep new value = false 
            if (visibleColumns > 1)
            {
                ((sender as CheckBox).BindingContext as DataGridColumn).IsVisible = false;
            }
            else
            {
                //otherwise force checkbox value to be true
                (sender as CheckBox).CheckedChanged -= ColumnCheckedChanged;
                (sender as CheckBox).IsChecked = true;
                (sender as CheckBox).CheckedChanged += ColumnCheckedChanged;
            }
        }
        else
        {
            //new value true is always allowed
            ((sender as CheckBox).BindingContext as DataGridColumn).IsVisible = true;
        }
    }
}
