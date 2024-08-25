using Management_of_Mossad_agents___API.Enums;
using System.ComponentModel.DataAnnotations;

namespace Management_of_Mossad_agents___API.Models
{
    public class Agent
    {
        [Key]
        public int id { get; set; }
        public string photoUrl { get; set; }
        public string nickname { get; set; }
        public Location? location { get; set; }
        public AgentStatus? status { get; set; }
    }
}
