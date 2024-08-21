using Management_of_Mossad_agents___API.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Management_of_Mossad_agents___API.Models
{
    public class Mission
    {
        [Key]
        public int Id { get; set; }
        public Agent AgentID { get; set; }
        public Target TargetID { get; set; }
        public Double TimeLeft { get; set; }
        public Double ActualExecutionTime { get; set; }
        public MissionStatus? status { get; set; }



    }
}
