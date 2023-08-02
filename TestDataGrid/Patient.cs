
namespace TestDataGrid
{
    public class Patient : Maui.DataGrid.IDataGridSearchable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime Birthdate { get; set; }
        public string Birthplace { get; set; }
        //public EvolutionIconEnum.IconPathDataEnumType EImage { get; set; }
        public Image Icon { get; set; }

        /*/// <summary>
        /// Function that return all the properties of the Patient
        /// </summary>
        /// <returns></returns>
        public string GetSearchableText()
        {
            return $"{Id} {Name} {Surname} {Birthdate} {Birthplace}";
        }*/

        /// <summary>
        /// Function that return the properties of the Patient Class where the user can search
        /// </summary>
        /// <returns></returns>
        public List<string> GetSearchableList()
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
