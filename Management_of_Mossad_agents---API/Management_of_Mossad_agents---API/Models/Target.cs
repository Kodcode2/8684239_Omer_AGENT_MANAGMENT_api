using Management_of_Mossad_agents___API.Enums;
using System.ComponentModel.DataAnnotations;

namespace Management_of_Mossad_agents___API.Models
{
    public class Target
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string? photo_url { get; set; }
        public string position { get; set; }
        public Location? location { get; set; }
        public TargetStatus? status { get; set; }
    }
}
