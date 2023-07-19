namespace Maui.DataGrid;

using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

public partial class DataGridUserPreferencesSetup : ContentPage
{

    public DataGridUserPreferencesSetup(System.Collections.ObjectModel.ObservableCollection<DataGridColumn> columns)
    {
        InitializeComponent();
        ColumnsList.ItemsSource = columns;
        BindingContext = this;
    }


}
