namespace TestDataGrid;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using Maui.DataGrid;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private List<Patient> _List = new List<Patient>();

    /// <summary>
    /// List for the columns names that can be selected for the specific research
    /// </summary>
    public ObservableCollection<string> ColumnsWhereToSearch { get; set; } = new();

    #region INotifyPropertyChanged implementation

    public new event PropertyChangedEventHandler PropertyChanged;

    private new void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    #endregion INotifyPropertyChanged implementation

    public MainPage()
    {
        BindingContext = this;
        InitializeComponent();
        List = GetPatients();

        //! ESSENTIAL FOR PAGINATION
        //! used to update stepper maximum property
        SizePicker.PropertyChanged += MainPage_PropertyChanged;
        PropertyChanged += MainPage_PropertyChanged;

        ColumnsWhereToSearch.Add("All");
        foreach (var col in DataGrid.Columns)
        {
            col.ColumnVisibilityChanged += Col_ColumnVisibilityChanged;
            if (col.IsVisible && Patient.GetSearchableFields().Contains(col.PropertyName))
            {
                ColumnsWhereToSearch.Add(col.PropertyName);
            }
        }
        columnSearch.SelectedIndex = 0;
    }

    /// <summary>
    /// Function for update the Columns names list where the user can make the research when their visibility is changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Col_ColumnVisibilityChanged(object sender, EventArgs e)
    {
        Debug.WriteLine("Col_ColumnVisibilityChanged: ");

        if ((sender as DataGridColumn).IsVisible)
        {
            if (!ColumnsWhereToSearch.Contains((sender as DataGridColumn).PropertyName))
            {
                ColumnsWhereToSearch.Add((sender as DataGridColumn).PropertyName);
            }
        }
        else
        {
            if (ColumnsWhereToSearch.Contains((sender as DataGridColumn).PropertyName))
            {
                ColumnsWhereToSearch.Remove((sender as DataGridColumn).PropertyName);
            }
        }

    }

    private void MainPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        _SetMaximumStepperValue();
    }

    /// <summary>
    /// Function for Set the correct maximum value of the stepper when modify the pagination or the list
    /// </summary>
    private void _SetMaximumStepperValue()
    {
        if (DataGrid.StepperMaximum > 1)
        {
            PaginationStepper.IsEnabled = true;
            PaginationStepper.Maximum = DataGrid.StepperMaximum;
        }
        else
        {
            PaginationStepper.IsEnabled = false;
        }
    }

    public List<Patient> List
    {
        get => _List;
        set
        {
            _List = value;
            OnPropertyChanged(nameof(List));
        }
    }

    private static List<Patient> GetPatients()
    {
        try
        {
            return new List<Patient>() {
            new Patient {
                Id = 1,
                Name = "Mario",
                Surname = "Rossi",
                Birthdate = new DateTime(1990, 5, 10),
                Birthplace = "Roma",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog.png"}
            },
            new Patient {
                Id = 2,
                Name = "Laura",
                Surname = "Bianchi",
                Birthdate = new DateTime(1985, 9, 15),
                Birthplace = "Milano",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "anal_gland_expression.png"}
            },
            new Patient {
                Id = 3,
                Name = "Giuseppe",
                Surname = "Verdi",
                Birthdate = new DateTime(1978, 3, 25),
                Birthplace = "Napoli",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "cone_of_shame.png"}
            },
            new Patient {
                Id = 4,
                Name = "Paolo",
                Surname = "Ferrari",
                Birthdate = new DateTime(1992, 7, 7),
                Birthplace = "Torino",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog2.png"}
            },
            new Patient {
                Id = 5,
                Name = "Francesca",
                Surname = "Russo",
                Birthdate = new DateTime(1987, 12, 18),
                Birthplace = "Palermo",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet_insurance.png"}
            },
            new Patient {
                Id = 6,
                Name = "Luca",
                Surname = "Marini",
                Birthdate = new DateTime(1983, 2, 5),
                Birthplace = "Firenze",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 7,
                Name = "Alessia",
                Surname = "Galli",
                Birthdate = new DateTime(1995, 11, 3),
                Birthplace = "Bologna",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 8,
                Name = "Roberto",
                Surname = "Conti",
                Birthdate = new DateTime(1980, 8, 20),
                Birthplace = "Genova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian.png"}
            },
            new Patient {
                Id = 9,
                Name = "Elisa",
                Surname = "Marchetti",
                Birthdate = new DateTime(1998, 4, 14),
                Birthplace = "Verona",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian2.png"}
            },
            new Patient {
                Id = 10,
                Name = "Giovanni",
                Surname = "Ricci",
                Birthdate = new DateTime(1975, 1, 30),
                Birthplace = "Padova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian3.png"}
            },
            new Patient {
                Id = 11,
                Name = "Stefania",
                Surname = "Esposito",
                Birthdate = new DateTime(1991, 6, 8),
                Birthplace = "Perugia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian4.png"}
            },
            new Patient {
                Id = 12,
                Name = "Marco",
                Surname = "Ferri",
                Birthdate = new DateTime(1986, 10, 12),
                Birthplace = "Trieste",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian5.png"}
            },
            new Patient {
                Id = 13,
                Name = "Valentina",
                Surname = "Barbieri",
                Birthdate = new DateTime(1982, 9, 22),
                Birthplace = "Cagliari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "weight.png"}
            },
            new Patient {
                Id = 14,
                Name = "Antonio",
                Surname = "Vitali",
                Birthdate = new DateTime(1993, 3, 9),
                Birthplace = "Catania",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray.png"}
            },
            new Patient {
                Id = 15,
                Name = "Sara",
                Surname = "Fabbri",
                Birthdate = new DateTime(1979, 11, 1),
                Birthplace = "Messina",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray2.png"}
            },
            new Patient {
                Id = 16,
                Name = "Simone",
                Surname = "Costa",
                Birthdate = new DateTime(1996, 7, 19),
                Birthplace = "Pisa",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray3.png"}
            },
            new Patient {
                Id = 17,
                Name = "Eleonora",
                Surname = "Gentile",
                Birthdate = new DateTime(1984, 4, 28),
                Birthplace = "Modena",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray4.png"}
            },
            new Patient {
                Id = 18,
                Name = "Riccardo",
                Surname = "Lombardi",
                Birthdate = new DateTime(1999, 2, 16),
                Birthplace = "Bari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian6.png"}
            },
            new Patient {
                Id = 19,
                Name = "Cristina",
                Surname = "Martini",
                Birthdate = new DateTime(1977, 10, 7),
                Birthplace = "Trento",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 20,
                Name = "Lorenzo",
                Surname = "Galli",
                Birthdate = new DateTime(1994, 8, 23),
                Birthplace = "Venezia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 1,
                Name = "Mario",
                Surname = "Rossi",
                Birthdate = new DateTime(1990, 5, 10),
                Birthplace = "Roma",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog.png"}
            },
            new Patient {
                Id = 2,
                Name = "Laura",
                Surname = "Bianchi",
                Birthdate = new DateTime(1985, 9, 15),
                Birthplace = "Milano",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "anal_gland_expression.png"}
            },
            new Patient {
                Id = 3,
                Name = "Giuseppe",
                Surname = "Verdi",
                Birthdate = new DateTime(1978, 3, 25),
                Birthplace = "Napoli",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "cone_of_shame.png"}
            },
            new Patient {
                Id = 4,
                Name = "Paolo",
                Surname = "Ferrari",
                Birthdate = new DateTime(1992, 7, 7),
                Birthplace = "Torino",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog2.png"}
            },
            new Patient {
                Id = 5,
                Name = "Francesca",
                Surname = "Russo",
                Birthdate = new DateTime(1987, 12, 18),
                Birthplace = "Palermo",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet_insurance.png"}
            },
            new Patient {
                Id = 6,
                Name = "Luca",
                Surname = "Marini",
                Birthdate = new DateTime(1983, 2, 5),
                Birthplace = "Firenze",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 7,
                Name = "Alessia",
                Surname = "Galli",
                Birthdate = new DateTime(1995, 11, 3),
                Birthplace = "Bologna",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 8,
                Name = "Roberto",
                Surname = "Conti",
                Birthdate = new DateTime(1980, 8, 20),
                Birthplace = "Genova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian.png"}
            },
            new Patient {
                Id = 9,
                Name = "Elisa",
                Surname = "Marchetti",
                Birthdate = new DateTime(1998, 4, 14),
                Birthplace = "Verona",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian2.png"}
            },
            new Patient {
                Id = 10,
                Name = "Giovanni",
                Surname = "Ricci",
                Birthdate = new DateTime(1975, 1, 30),
                Birthplace = "Padova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian3.png"}
            },
            new Patient {
                Id = 11,
                Name = "Stefania",
                Surname = "Esposito",
                Birthdate = new DateTime(1991, 6, 8),
                Birthplace = "Perugia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian4.png"}
            },
            new Patient {
                Id = 12,
                Name = "Marco",
                Surname = "Ferri",
                Birthdate = new DateTime(1986, 10, 12),
                Birthplace = "Trieste",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian5.png"}
            },
            new Patient {
                Id = 13,
                Name = "Valentina",
                Surname = "Barbieri",
                Birthdate = new DateTime(1982, 9, 22),
                Birthplace = "Cagliari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "weight.png"}
            },
            new Patient {
                Id = 14,
                Name = "Antonio",
                Surname = "Vitali",
                Birthdate = new DateTime(1993, 3, 9),
                Birthplace = "Catania",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray.png"}
            },
            new Patient {
                Id = 15,
                Name = "Sara",
                Surname = "Fabbri",
                Birthdate = new DateTime(1979, 11, 1),
                Birthplace = "Messina",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray2.png"}
            },
            new Patient {
                Id = 16,
                Name = "Simone",
                Surname = "Costa",
                Birthdate = new DateTime(1996, 7, 19),
                Birthplace = "Pisa",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray3.png"}
            },
            new Patient {
                Id = 17,
                Name = "Eleonora",
                Surname = "Gentile",
                Birthdate = new DateTime(1984, 4, 28),
                Birthplace = "Modena",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray4.png"}
            },
            new Patient {
                Id = 18,
                Name = "Riccardo",
                Surname = "Lombardi",
                Birthdate = new DateTime(1999, 2, 16),
                Birthplace = "Bari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian6.png"}
            },
            new Patient {
                Id = 19,
                Name = "Cristina",
                Surname = "Martini",
                Birthdate = new DateTime(1977, 10, 7),
                Birthplace = "Trento",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 20,
                Name = "Lorenzo",
                Surname = "Galli",
                Birthdate = new DateTime(1994, 8, 23),
                Birthplace = "Venezia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 1,
                Name = "Mario",
                Surname = "Rossi",
                Birthdate = new DateTime(1990, 5, 10),
                Birthplace = "Roma",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog.png"}
            },
            new Patient {
                Id = 2,
                Name = "Laura",
                Surname = "Bianchi",
                Birthdate = new DateTime(1985, 9, 15),
                Birthplace = "Milano",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "anal_gland_expression.png"}
            },
            new Patient {
                Id = 3,
                Name = "Giuseppe",
                Surname = "Verdi",
                Birthdate = new DateTime(1978, 3, 25),
                Birthplace = "Napoli",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "cone_of_shame.png"}
            },
            new Patient {
                Id = 4,
                Name = "Paolo",
                Surname = "Ferrari",
                Birthdate = new DateTime(1992, 7, 7),
                Birthplace = "Torino",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog2.png"}
            },
            new Patient {
                Id = 5,
                Name = "Francesca",
                Surname = "Russo",
                Birthdate = new DateTime(1987, 12, 18),
                Birthplace = "Palermo",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet_insurance.png"}
            },
            new Patient {
                Id = 6,
                Name = "Luca",
                Surname = "Marini",
                Birthdate = new DateTime(1983, 2, 5),
                Birthplace = "Firenze",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 7,
                Name = "Alessia",
                Surname = "Galli",
                Birthdate = new DateTime(1995, 11, 3),
                Birthplace = "Bologna",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 8,
                Name = "Roberto",
                Surname = "Conti",
                Birthdate = new DateTime(1980, 8, 20),
                Birthplace = "Genova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian.png"}
            },
            new Patient {
                Id = 9,
                Name = "Elisa",
                Surname = "Marchetti",
                Birthdate = new DateTime(1998, 4, 14),
                Birthplace = "Verona",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian2.png"}
            },
            new Patient {
                Id = 10,
                Name = "Giovanni",
                Surname = "Ricci",
                Birthdate = new DateTime(1975, 1, 30),
                Birthplace = "Padova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian3.png"}
            },
            new Patient {
                Id = 11,
                Name = "Stefania",
                Surname = "Esposito",
                Birthdate = new DateTime(1991, 6, 8),
                Birthplace = "Perugia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian4.png"}
            },
            new Patient {
                Id = 12,
                Name = "Marco",
                Surname = "Ferri",
                Birthdate = new DateTime(1986, 10, 12),
                Birthplace = "Trieste",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian5.png"}
            },
            new Patient {
                Id = 13,
                Name = "Valentina",
                Surname = "Barbieri",
                Birthdate = new DateTime(1982, 9, 22),
                Birthplace = "Cagliari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "weight.png"}
            },
            new Patient {
                Id = 14,
                Name = "Antonio",
                Surname = "Vitali",
                Birthdate = new DateTime(1993, 3, 9),
                Birthplace = "Catania",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray.png"}
            },
            new Patient {
                Id = 15,
                Name = "Sara",
                Surname = "Fabbri",
                Birthdate = new DateTime(1979, 11, 1),
                Birthplace = "Messina",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray2.png"}
            },
            new Patient {
                Id = 16,
                Name = "Simone",
                Surname = "Costa",
                Birthdate = new DateTime(1996, 7, 19),
                Birthplace = "Pisa",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray3.png"}
            },
            new Patient {
                Id = 17,
                Name = "Eleonora",
                Surname = "Gentile",
                Birthdate = new DateTime(1984, 4, 28),
                Birthplace = "Modena",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray4.png"}
            },
            new Patient {
                Id = 18,
                Name = "Riccardo",
                Surname = "Lombardi",
                Birthdate = new DateTime(1999, 2, 16),
                Birthplace = "Bari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian6.png"}
            },
            new Patient {
                Id = 19,
                Name = "Cristina",
                Surname = "Martini",
                Birthdate = new DateTime(1977, 10, 7),
                Birthplace = "Trento",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 20,
                Name = "Lorenzo",
                Surname = "Galli",
                Birthdate = new DateTime(1994, 8, 23),
                Birthplace = "Venezia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 1,
                Name = "Mario",
                Surname = "Rossi",
                Birthdate = new DateTime(1990, 5, 10),
                Birthplace = "Roma",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog.png"}
            },
            new Patient {
                Id = 2,
                Name = "Laura",
                Surname = "Bianchi",
                Birthdate = new DateTime(1985, 9, 15),
                Birthplace = "Milano",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "anal_gland_expression.png"}
            },
            new Patient {
                Id = 3,
                Name = "Giuseppe",
                Surname = "Verdi",
                Birthdate = new DateTime(1978, 3, 25),
                Birthplace = "Napoli",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "cone_of_shame.png"}
            },
            new Patient {
                Id = 4,
                Name = "Paolo",
                Surname = "Ferrari",
                Birthdate = new DateTime(1992, 7, 7),
                Birthplace = "Torino",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog2.png"}
            },
            new Patient {
                Id = 5,
                Name = "Francesca",
                Surname = "Russo",
                Birthdate = new DateTime(1987, 12, 18),
                Birthplace = "Palermo",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet_insurance.png"}
            },
            new Patient {
                Id = 6,
                Name = "Luca",
                Surname = "Marini",
                Birthdate = new DateTime(1983, 2, 5),
                Birthplace = "Firenze",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 7,
                Name = "Alessia",
                Surname = "Galli",
                Birthdate = new DateTime(1995, 11, 3),
                Birthplace = "Bologna",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 8,
                Name = "Roberto",
                Surname = "Conti",
                Birthdate = new DateTime(1980, 8, 20),
                Birthplace = "Genova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian.png"}
            },
            new Patient {
                Id = 9,
                Name = "Elisa",
                Surname = "Marchetti",
                Birthdate = new DateTime(1998, 4, 14),
                Birthplace = "Verona",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian2.png"}
            },
            new Patient {
                Id = 10,
                Name = "Giovanni",
                Surname = "Ricci",
                Birthdate = new DateTime(1975, 1, 30),
                Birthplace = "Padova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian3.png"}
            },
            new Patient {
                Id = 11,
                Name = "Stefania",
                Surname = "Esposito",
                Birthdate = new DateTime(1991, 6, 8),
                Birthplace = "Perugia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian4.png"}
            },
            new Patient {
                Id = 12,
                Name = "Marco",
                Surname = "Ferri",
                Birthdate = new DateTime(1986, 10, 12),
                Birthplace = "Trieste",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian5.png"}
            },
            new Patient {
                Id = 13,
                Name = "Valentina",
                Surname = "Barbieri",
                Birthdate = new DateTime(1982, 9, 22),
                Birthplace = "Cagliari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "weight.png"}
            },
            new Patient {
                Id = 14,
                Name = "Antonio",
                Surname = "Vitali",
                Birthdate = new DateTime(1993, 3, 9),
                Birthplace = "Catania",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray.png"}
            },
            new Patient {
                Id = 15,
                Name = "Sara",
                Surname = "Fabbri",
                Birthdate = new DateTime(1979, 11, 1),
                Birthplace = "Messina",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray2.png"}
            },
            new Patient {
                Id = 16,
                Name = "Simone",
                Surname = "Costa",
                Birthdate = new DateTime(1996, 7, 19),
                Birthplace = "Pisa",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray3.png"}
            },
            new Patient {
                Id = 17,
                Name = "Eleonora",
                Surname = "Gentile",
                Birthdate = new DateTime(1984, 4, 28),
                Birthplace = "Modena",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray4.png"}
            },
            new Patient {
                Id = 18,
                Name = "Riccardo",
                Surname = "Lombardi",
                Birthdate = new DateTime(1999, 2, 16),
                Birthplace = "Bari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian6.png"}
            },
            new Patient {
                Id = 19,
                Name = "Cristina",
                Surname = "Martini",
                Birthdate = new DateTime(1977, 10, 7),
                Birthplace = "Trento",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 20,
                Name = "Lorenzo",
                Surname = "Galli",
                Birthdate = new DateTime(1994, 8, 23),
                Birthplace = "Venezia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 1,
                Name = "Mario",
                Surname = "Rossi",
                Birthdate = new DateTime(1990, 5, 10),
                Birthplace = "Roma",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog.png"}
            },
            new Patient {
                Id = 2,
                Name = "Laura",
                Surname = "Bianchi",
                Birthdate = new DateTime(1985, 9, 15),
                Birthplace = "Milano",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "anal_gland_expression.png"}
            },
            new Patient {
                Id = 3,
                Name = "Giuseppe",
                Surname = "Verdi",
                Birthdate = new DateTime(1978, 3, 25),
                Birthplace = "Napoli",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "cone_of_shame.png"}
            },
            new Patient {
                Id = 4,
                Name = "Paolo",
                Surname = "Ferrari",
                Birthdate = new DateTime(1992, 7, 7),
                Birthplace = "Torino",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog2.png"}
            },
            new Patient {
                Id = 5,
                Name = "Francesca",
                Surname = "Russo",
                Birthdate = new DateTime(1987, 12, 18),
                Birthplace = "Palermo",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet_insurance.png"}
            },
            new Patient {
                Id = 6,
                Name = "Luca",
                Surname = "Marini",
                Birthdate = new DateTime(1983, 2, 5),
                Birthplace = "Firenze",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 7,
                Name = "Alessia",
                Surname = "Galli",
                Birthdate = new DateTime(1995, 11, 3),
                Birthplace = "Bologna",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 8,
                Name = "Roberto",
                Surname = "Conti",
                Birthdate = new DateTime(1980, 8, 20),
                Birthplace = "Genova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian.png"}
            },
            new Patient {
                Id = 9,
                Name = "Elisa",
                Surname = "Marchetti",
                Birthdate = new DateTime(1998, 4, 14),
                Birthplace = "Verona",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian2.png"}
            },
            new Patient {
                Id = 10,
                Name = "Giovanni",
                Surname = "Ricci",
                Birthdate = new DateTime(1975, 1, 30),
                Birthplace = "Padova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian3.png"}
            },
            new Patient {
                Id = 11,
                Name = "Stefania",
                Surname = "Esposito",
                Birthdate = new DateTime(1991, 6, 8),
                Birthplace = "Perugia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian4.png"}
            },
            new Patient {
                Id = 12,
                Name = "Marco",
                Surname = "Ferri",
                Birthdate = new DateTime(1986, 10, 12),
                Birthplace = "Trieste",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian5.png"}
            },
            new Patient {
                Id = 13,
                Name = "Valentina",
                Surname = "Barbieri",
                Birthdate = new DateTime(1982, 9, 22),
                Birthplace = "Cagliari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "weight.png"}
            },
            new Patient {
                Id = 14,
                Name = "Antonio",
                Surname = "Vitali",
                Birthdate = new DateTime(1993, 3, 9),
                Birthplace = "Catania",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray.png"}
            },
            new Patient {
                Id = 15,
                Name = "Sara",
                Surname = "Fabbri",
                Birthdate = new DateTime(1979, 11, 1),
                Birthplace = "Messina",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray2.png"}
            },
            new Patient {
                Id = 16,
                Name = "Simone",
                Surname = "Costa",
                Birthdate = new DateTime(1996, 7, 19),
                Birthplace = "Pisa",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray3.png"}
            },
            new Patient {
                Id = 17,
                Name = "Eleonora",
                Surname = "Gentile",
                Birthdate = new DateTime(1984, 4, 28),
                Birthplace = "Modena",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray4.png"}
            },
            new Patient {
                Id = 18,
                Name = "Riccardo",
                Surname = "Lombardi",
                Birthdate = new DateTime(1999, 2, 16),
                Birthplace = "Bari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian6.png"}
            },
            new Patient {
                Id = 19,
                Name = "Cristina",
                Surname = "Martini",
                Birthdate = new DateTime(1977, 10, 7),
                Birthplace = "Trento",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 20,
                Name = "Lorenzo",
                Surname = "Galli",
                Birthdate = new DateTime(1994, 8, 23),
                Birthplace = "Venezia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 1,
                Name = "Mario",
                Surname = "Rossi",
                Birthdate = new DateTime(1990, 5, 10),
                Birthplace = "Roma",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog.png"}
            },
            new Patient {
                Id = 2,
                Name = "Laura",
                Surname = "Bianchi",
                Birthdate = new DateTime(1985, 9, 15),
                Birthplace = "Milano",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "anal_gland_expression.png"}
            },
            new Patient {
                Id = 3,
                Name = "Giuseppe",
                Surname = "Verdi",
                Birthdate = new DateTime(1978, 3, 25),
                Birthplace = "Napoli",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "cone_of_shame.png"}
            },
            new Patient {
                Id = 4,
                Name = "Paolo",
                Surname = "Ferrari",
                Birthdate = new DateTime(1992, 7, 7),
                Birthplace = "Torino",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "dog2.png"}
            },
            new Patient {
                Id = 5,
                Name = "Francesca",
                Surname = "Russo",
                Birthdate = new DateTime(1987, 12, 18),
                Birthplace = "Palermo",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet_insurance.png"}
            },
            new Patient {
                Id = 6,
                Name = "Luca",
                Surname = "Marini",
                Birthdate = new DateTime(1983, 2, 5),
                Birthplace = "Firenze",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 7,
                Name = "Alessia",
                Surname = "Galli",
                Birthdate = new DateTime(1995, 11, 3),
                Birthplace = "Bologna",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            },
            new Patient {
                Id = 8,
                Name = "Roberto",
                Surname = "Conti",
                Birthdate = new DateTime(1980, 8, 20),
                Birthplace = "Genova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian.png"}
            },
            new Patient {
                Id = 9,
                Name = "Elisa",
                Surname = "Marchetti",
                Birthdate = new DateTime(1998, 4, 14),
                Birthplace = "Verona",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian2.png"}
            },
            new Patient {
                Id = 10,
                Name = "Giovanni",
                Surname = "Ricci",
                Birthdate = new DateTime(1975, 1, 30),
                Birthplace = "Padova",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian3.png"}
            },
            new Patient {
                Id = 11,
                Name = "Stefania",
                Surname = "Esposito",
                Birthdate = new DateTime(1991, 6, 8),
                Birthplace = "Perugia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian4.png"}
            },
            new Patient {
                Id = 12,
                Name = "Marco",
                Surname = "Ferri",
                Birthdate = new DateTime(1986, 10, 12),
                Birthplace = "Trieste",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian5.png"}
            },
            new Patient {
                Id = 13,
                Name = "Valentina",
                Surname = "Barbieri",
                Birthdate = new DateTime(1982, 9, 22),
                Birthplace = "Cagliari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "weight.png"}
            },
            new Patient {
                Id = 14,
                Name = "Antonio",
                Surname = "Vitali",
                Birthdate = new DateTime(1993, 3, 9),
                Birthplace = "Catania",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray.png"}
            },
            new Patient {
                Id = 15,
                Name = "Sara",
                Surname = "Fabbri",
                Birthdate = new DateTime(1979, 11, 1),
                Birthplace = "Messina",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray2.png"}
            },
            new Patient {
                Id = 16,
                Name = "Simone",
                Surname = "Costa",
                Birthdate = new DateTime(1996, 7, 19),
                Birthplace = "Pisa",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray3.png"}
            },
            new Patient {
                Id = 17,
                Name = "Eleonora",
                Surname = "Gentile",
                Birthdate = new DateTime(1984, 4, 28),
                Birthplace = "Modena",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "x_ray4.png"}
            },
            new Patient {
                Id = 18,
                Name = "Riccardo",
                Surname = "Lombardi",
                Birthdate = new DateTime(1999, 2, 16),
                Birthplace = "Bari",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "veterinarian6.png"}
            },
            new Patient {
                Id = 19,
                Name = "Cristina",
                Surname = "Martini",
                Birthdate = new DateTime(1977, 10, 7),
                Birthplace = "Trento",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet1.png"}
            },
            new Patient {
                Id = 20,
                Name = "Lorenzo",
                Surname = "Galli",
                Birthdate = new DateTime(1994, 8, 23),
                Birthplace = "Venezia",
                //EImage =  EvolutionIconEnum.IconPathDataEnumType.PatientIcon,
                Icon = new Image{Source = "pet2.png"}
            }

    };
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.StackTrace);
        }
        return null;
    }

    /// <summary>
    /// Research through the items a specific query inserted in the searchBar
    /// </summary>
    public ICommand PerformSearch => new Command<string>((string query) =>
    {
        List = GetPatients();

        List<Patient> searchResult = null;
        if ((string)columnSearch.SelectedItem == "All")
        {
            //searchResult = _GetSearchResults(query);
            searchResult = DataGrid.Search(List, query, columnSearch.Items.ToArray());
        }
        else
        {
            //searchResult = _GetSearchResults(query, (string)columnSearch.SelectedItem);
            searchResult = DataGrid.Search(List, query, (string)columnSearch.SelectedItem);
        }

        if (searchResult != null)
        {
            try
            {
                List = searchResult;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    });

    /*/// <summary>
    /// Function for search the given text in all the visible fields
    /// </summary>
    /// <param name="query">string to research</param>
    /// <returns>List of Patients that match the research</returns>
    private List<Patient> _GetSearchResults(string query)
    {
        var result = new List<Patient>();

        foreach (var column in DataGrid.Columns)
        {
            result = _Search(query, result, column);
        }

        for (var i = 0; i < result.Count; i++)
        {
            for (var k = i + 1; k < result.Count; k++)
            {
                if (result[i] == result[k])
                {
                    result.Remove(result[k]);
                    k--;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Function for search the given text in the selected column the visible fields
    /// </summary>
    /// <param name="query">string to research</param>
    /// <param name="columnWhereToSearch">column where to search</param>
    /// <returns>List of Patients that match the research</returns>
    private List<Patient> _GetSearchResults(string query, string columnWhereToSearch)
    {
        var result = new List<Patient>();

        foreach (var column in DataGrid.Columns)
        {
            if (column.PropertyName == columnWhereToSearch)
            {
                result = _Search(query, result, column);
            }
        }

        for (var i = 0; i < result.Count; i++)
        {
            for (var k = i + 1; k < result.Count; k++)
            {
                if (result[i] == result[k])
                {
                    result.Remove(result[k]);
                    k--;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Function for the research on column
    /// </summary>
    /// <param name="query">string to research</param>
    /// <param name="result">List of previous search results</param>
    /// <param name="column">Column where to search</param>
    /// <returns>List of Patients that match the research</returns>
    private List<Patient> _Search(string query, List<Patient> result, DataGridColumn column)
    {
        if (column.PropertyName == "Id")
        {
            if (column.IsVisible)
            {
                var temp = new List<Patient>(List.Where(x => !string.IsNullOrWhiteSpace(x.Id.ToString(CultureInfo.CurrentCulture)) && x.Id.ToString(CultureInfo.CurrentCulture).Contains(query, StringComparison.OrdinalIgnoreCase)));
                result = result.Concat(temp).ToList();
            }
        }

        if (column.PropertyName == "Name")
        {
            if (column.IsVisible)
            {
                var temp = new List<Patient>(List.Where(x => !string.IsNullOrWhiteSpace(x.Name.ToString(CultureInfo.CurrentCulture)) && x.Name.ToString(CultureInfo.CurrentCulture).Contains(query, StringComparison.OrdinalIgnoreCase)));
                result = result.Concat(temp).ToList();
            }
        }

        if (column.PropertyName == "Surname")
        {
            if (column.IsVisible)
            {
                var temp = new List<Patient>(List.Where(x => !string.IsNullOrWhiteSpace(x.Surname.ToString(CultureInfo.CurrentCulture)) && x.Surname.ToString(CultureInfo.CurrentCulture).Contains(query, StringComparison.OrdinalIgnoreCase)));
                result = result.Concat(temp).ToList();
            }
        }

        if (column.PropertyName == "Birthplace")
        {
            if (column.IsVisible)
            {
                var temp = new List<Patient>(List.Where(x => !string.IsNullOrWhiteSpace(x.Birthplace.ToString(CultureInfo.CurrentCulture)) && x.Birthplace.ToString(CultureInfo.CurrentCulture).Contains(query, StringComparison.OrdinalIgnoreCase)));
                result = result.Concat(temp).ToList();
            }
        }

        if (column.PropertyName == "Birthdate")
        {
            if (column.IsVisible)
            {
                var temp = new List<Patient>(List.Where(x => !string.IsNullOrWhiteSpace(x.Birthdate.ToString(CultureInfo.CurrentCulture)) && x.Birthdate.ToString(CultureInfo.CurrentCulture).Contains(query, StringComparison.OrdinalIgnoreCase)));
                result = result.Concat(temp).ToList();
            }
        }

        return result;
    }*/

    private async void _DataGridItemSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectionMode = DataGrid.SelectionMode;
        IList<object> selectedItems = new List<object>();

        if (selectionMode == SelectionMode.Single)
        {
            selectedItems.Add(DataGrid.SelectedItem);
        }
        else if (selectionMode == SelectionMode.Multiple)
        {
            selectedItems = DataGrid.SelectedItems;
        }
        var popup = new PatientPopup(selectionMode, selectedItems);

        await this.ShowPopupAsync(popup);
    }

    private void SelectionModeButtonClick(object sender, EventArgs e)
    {
        FontImageSource singleSelectionImage = new FontImageSource()
        {
            FontFamily = "MaterialRegular",
            Glyph = UraniumUI.Icons.MaterialIcons.MaterialRegular.Person,
            Color = Colors.Black

        };

        FontImageSource multipleSelectionImage = new FontImageSource()
        {
            FontFamily = "MaterialRegular",
            Glyph = UraniumUI.Icons.MaterialIcons.MaterialRegular.Group,
            Color = Colors.Black

        };

        if (DataGrid.SelectionMode == SelectionMode.Single)
        {
            DataGrid.SelectionMode = SelectionMode.Multiple;
            SelectionModeButton.ImageSource = singleSelectionImage;
        }
        else if (DataGrid.SelectionMode == SelectionMode.Multiple)
        {
            DataGrid.SelectionMode = SelectionMode.Single;
            SelectionModeButton.ImageSource = multipleSelectionImage;
        }

    }


    /// <summary>
    /// Method to resize all columns to width 1*
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ResizeDatagrid(object sender, EventArgs e)
    {
        DataGrid.Resize(0);
    }
}

