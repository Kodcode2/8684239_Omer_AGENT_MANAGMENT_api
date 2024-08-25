using Management_of_Mossad_agents___API.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Management_of_Mossad_agents___API.Models
{
    public class Mission
    {
        [Key]
        public int id { get; set; }
        public Agent agentid { get; set; }
        public Target targetid { get; set; }
        public Double? timeLeft { get; set; }
        public Double? actualExecutionTime { get; set; }
        public MissionStatus status { get; set; }



    }
}
