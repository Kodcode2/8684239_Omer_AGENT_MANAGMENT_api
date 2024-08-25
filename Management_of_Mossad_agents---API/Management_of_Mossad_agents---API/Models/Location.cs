using System.ComponentModel.DataAnnotations;

namespace Management_of_Mossad_agents___API.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Range(0, 1000)]
        public Double X { get; set; }

        [Range(0, 1000)]
        public Double Y { get; set; }
    }
}
