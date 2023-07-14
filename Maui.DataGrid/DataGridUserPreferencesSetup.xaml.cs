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

    //public Command VBackCommand
    //{
    //    get
    //    {
    //        return new Command(() =>
    //        {
    //            Debug.WriteLine("123");
    //            // if parameter are set, you could send a message to navigate
    //            //base.OnBackButtonPressed();
    //            if (CanGoBack)
    //            {
    //                WebView.GoBack();
    //                return true;
    //            }
    //            else
    //            {
    //                base.OnBackButtonPressed();
    //                return false;
    //            }
    //        });
    //    }
    //}
}
