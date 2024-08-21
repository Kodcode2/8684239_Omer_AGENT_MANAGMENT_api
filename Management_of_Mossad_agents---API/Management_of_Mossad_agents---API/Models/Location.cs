using System.ComponentModel.DataAnnotations;

namespace Management_of_Mossad_agents___API.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        public Double X { get; set; }
        public Double Y { get; set; }
    }
}
