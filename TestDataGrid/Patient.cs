
namespace TestDataGrid
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime Birthdate { get; set; }
        public string Birthplace { get; set; }
        //public EvolutionIconEnum.IconPathDataEnumType EImage { get; set; }
        public Image Icon { get; set; }

        /// <summary>
        /// Function that return the properties of the Patient Class where the user can search
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSearchableFields()
        {
            return new List<string>
            {
                $"Id",
                $"Name",
                $"Surname",
                $"Birthdate",
                $"Birthplace"
            };
        }
    }
}
