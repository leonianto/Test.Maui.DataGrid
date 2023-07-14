using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TestDataGrid;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
	private ObservableCollection<Patient> _List = new ObservableCollection<Patient>();

	#region INotifyPropertyChanged implementation

	public event PropertyChangedEventHandler PropertyChanged;

	private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

	#endregion INotifyPropertyChanged implementation

	public MainPage()
	{
		BindingContext = this;
		List = GetPatients();
		InitializeComponent();

	}
	public ObservableCollection<Patient> List {
		get => _List;
		set {
			_List = value;
			OnPropertyChanged(nameof(List));
		}
	}



	private ObservableCollection<Patient> GetPatients()
	{
		return   new ObservableCollection<Patient>() {
			new Patient {
				Id = 1,
				Name = "Mario",
				Surname = "Rossi",
				Birthdate = new DateTime(1990, 5, 10),
				Birthplace = "Roma"
			},
            new Patient {
				Id = 2,
				Name = "Laura",
				Surname = "Bianchi",
				Birthdate = new DateTime(1985, 9, 15),
				Birthplace = "Milano"
			},
            new Patient {
				Id = 3,
				Name = "Giuseppe",
				Surname = "Verdi",
				Birthdate = new DateTime(1978, 3, 25),
				Birthplace = "Napoli"
			},
            new Patient {
				Id = 4,
				Name = "Paolo",
				Surname = "Ferrari",
				Birthdate = new DateTime(1992, 7, 7),
				Birthplace = "Torino"
			},
            new Patient {
				Id = 5,
				Name = "Francesca",
				Surname = "Russo",
				Birthdate = new DateTime(1987, 12, 18),
				Birthplace = "Palermo"
			},
            new Patient {
				Id = 6,
				Name = "Luca",
				Surname = "Marini",
				Birthdate = new DateTime(1983, 2, 5),
				Birthplace = "Firenze"
			},
            new Patient {
				Id = 7,
				Name = "Alessia",
				Surname = "Galli",
				Birthdate = new DateTime(1995, 11, 3),
				Birthplace = "Bologna"
			},
            new Patient {
				Id = 8,
				Name = "Roberto",
				Surname = "Conti",
				Birthdate = new DateTime(1980, 8, 20),
				Birthplace = "Genova"
			},
            new Patient {
				Id = 9,
				Name = "Elisa",
				Surname = "Marchetti",
				Birthdate = new DateTime(1998, 4, 14),
				Birthplace = "Verona"
			},
            new Patient {
				Id = 10,
				Name = "Giovanni",
				Surname = "Ricci",
				Birthdate = new DateTime(1975, 1, 30),
				Birthplace = "Padova"
			},
            new Patient {
				Id = 11,
				Name = "Stefania",
				Surname = "Esposito",
				Birthdate = new DateTime(1991, 6, 8),
				Birthplace = "Perugia"
			},
            new Patient {
				Id = 12,
				Name = "Marco",
				Surname = "Ferri",
				Birthdate = new DateTime(1986, 10, 12),
				Birthplace = "Trieste"
			},
            new Patient {
				Id = 13,
				Name = "Valentina",
				Surname = "Barbieri",
				Birthdate = new DateTime(1982, 9, 22),
				Birthplace = "Cagliari"
			},
            new Patient {
				Id = 14,
				Name = "Antonio",
				Surname = "Vitali",
				Birthdate = new DateTime(1993, 3, 9),
				Birthplace = "Catania"
			},
            new Patient {
				Id = 15,
				Name = "Sara",
				Surname = "Fabbri",
				Birthdate = new DateTime(1979, 11, 1),
				Birthplace = "Messina"
			},
            new Patient {
				Id = 16,
				Name = "Simone",
				Surname = "Costa",
				Birthdate = new DateTime(1996, 7, 19),
				Birthplace = "Pisa"
			},
            new Patient {
				Id = 17,
				Name = "Eleonora",
				Surname = "Gentile",
				Birthdate = new DateTime(1984, 4, 28),
				Birthplace = "Modena"
			},
            new Patient {
				Id = 18,
				Name = "Riccardo",
				Surname = "Lombardi",
				Birthdate = new DateTime(1999, 2, 16),
				Birthplace = "Bari"
			},
            new Patient {
				Id = 19,
				Name = "Cristina",
				Surname = "Martini",
				Birthdate = new DateTime(1977, 10, 7),
				Birthplace = "Trento"
			},
            new Patient {
				Id = 20,
				Name = "Lorenzo",
				Surname = "Galli",
				Birthdate = new DateTime(1994, 8, 23),
				Birthplace = "Venezia"
			}

		};
	}
	

}

