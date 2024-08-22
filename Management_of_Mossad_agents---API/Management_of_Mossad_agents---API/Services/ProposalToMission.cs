using Management_of_Mossad_agents___API.DAL;
using Management_of_Mossad_agents___API.Enums;
using Management_of_Mossad_agents___API.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Management_of_Mossad_agents___API.Services
{
    public class ProposalToMission
    {
        

        //פונקציה שמקבלת סוכן ורשימה של מטרות
        public static Mission CheckByAgent(Agent agent, List<Target> listTargets) 
        { 
            foreach (Target t in listTargets)
            {
                Double distance = CheckDistance(t, agent);
                if (distance < 200)
                {
                    return CreateMission(t, agent, distance);
                }
             
            }
            return null;

        }

        //פונקציה שמקבלת מטרה ורשימה של סוכנים
        public static Mission CheckByTarget(Target target, List<Agent> listAgents) 
        {
            foreach (Agent a in listAgents)
            {
                Double distance = CheckDistance(target, a);
                if (distance < 200)
                {
                    return CreateMission(target, a, distance);
                }
            }
            return null;
        }

        //פונקציה שבודקת מרחק
        public static Double CheckDistance(Target target, Agent agent) 
        {
            Double distance = Math.Sqrt(Math.Pow(target.location.X - agent.location.X, 2) + Math.Pow(target.location.Y - agent.location.Y, 2));
            return distance;
        }


        //פונקציה שיוצרת זמן של משימה
        public static Double CalculateLeftTime(Double distance)
        {
            Double leftTime = distance / 5;
            return leftTime;
        }

        //פונקציה שיוצרת משימה תקבל 3 פרמטרים
        public static Mission CreateMission(Target target, Agent agent, Double distance)
        {
            Mission mission = new Mission();
            mission.AgentID = agent;
            mission.TargetID = target;
            mission.TimeLeft = CalculateLeftTime(distance);
            mission.status = MissionStatus.Proposal;
            return mission;

        }

    }
}
