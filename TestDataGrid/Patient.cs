
namespace TestDataGrid
{
    public enum PatientColumnSearch
    {
        All,
        Id,
        Name,
        Surname,
        Birthdate,
        Birthplace
    }

    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime Birthdate { get; set; }
        public string Birthplace { get; set; }
        //public EvolutionIconEnum.IconPathDataEnumType EImage { get; set; }
        public Image Icon { get; set; }
    }
}
