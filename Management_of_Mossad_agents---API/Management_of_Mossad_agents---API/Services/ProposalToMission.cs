using Management_of_Mossad_agents___API.DAL;
using Management_of_Mossad_agents___API.Enums;
using Management_of_Mossad_agents___API.Models;
using Microsoft.EntityFrameworkCore;

namespace Management_of_Mossad_agents___API.Services
{
    public class ProposalToMission
    {
        // פונקציה שמקבלת סוכן ורשימה של מטרות
        public static async Task<List<Mission>> CheckByAgentAsync(Agent agent, List<Target> listTargets, ManagementOfMossadAgentsDbContext context)
        {
            List<Mission> missions = new List<Mission>();

            foreach (Target t in listTargets)
            {
                if (t.location != null)
                {
                    Double distance = CheckDistance(t, agent);

                    if (distance < 200)
                    {
                        // בדיקה אם יש משימה כפולה אם כן לא להוסיף חדשה
                        bool isDuplicate = await CheckForDuplicateMissionsAsync(context, agent, t);
                        if (!isDuplicate)
                        {
                            Mission newMission = CreateMission(t, agent);
                            missions.Add(newMission);
                        }
                    }
                    else
                    {
                        // אם המרחק גדול מ200 למחוק משימה קיימת
                        await RemoveExistingMissionAsync(context, agent, t);
                    }
                }
            }

            return missions.Count > 0 ? missions : null;
        }

        // פונקציה שמקבלת מטרה ורשימה של סוכנים
        public static async Task<List<Mission>> CheckByTargetAsync(Target target, List<Agent> listAgents, ManagementOfMossadAgentsDbContext context)
        {
            List<Mission> missions = new List<Mission>();

            foreach (Agent a in listAgents)
            {
                if (a.location != null)
                {
                    Double distance = CheckDistance(target, a);
                    if (distance < 200)
                    {
                        // בדיקה אם יש משימה כפולה אם כן לא להוסיף חדשה
                        bool isDuplicate = await CheckForDuplicateMissionsAsync(context, a, target);
                        if (!isDuplicate)
                        {
                            Mission newMission = CreateMission(target, a);
                            missions.Add(newMission);
                        }
                    }
                    else
                    {
                        // אם המרחק גדול מ 200 למחוק משימה קיימת
                        await RemoveExistingMissionAsync(context, a, target);
                    }
                }
            }

            return missions.Count > 0 ? missions : null;
        }

        // פונקציה שבודקת מרחק בין סוכן למטרה
        public static Double CheckDistance(Target target, Agent agent)
        {
            Double distance = Math.Sqrt(Math.Pow(target.location.X - agent.location.X, 2) + Math.Pow(target.location.Y - agent.location.Y, 2));
            return distance;
        }



        // פונקציה שיוצרת משימה 
        public static Mission CreateMission(Target target, Agent agent)
        {
            Mission mission = new Mission();
            mission.agentid = agent;
            mission.targetid = target;
            mission.status = MissionStatus.Proposal;
            return mission;
        }

        // פונקציה שבודקת אם יש כבר משימה כפולה עם אותו סוכן ואותה מטרה
        public static async Task<bool> CheckForDuplicateMissionsAsync(ManagementOfMossadAgentsDbContext context, Agent agent, Target target)
        {
            var existingMission = await context.Missions
                .FirstOrDefaultAsync(m => m.agentid.id == agent.id && m.targetid.id == target.id );

            // אם נמצאה משימה כפולה תחזיר טרו
            return existingMission != null;
        }



        // פונקציה שמוחקת משימה קיימת אם המרחק גדול מ 200
        public static async Task RemoveExistingMissionAsync(ManagementOfMossadAgentsDbContext context, Agent agent, Target target)
        {
            var existingMission = await context.Missions
                .FirstOrDefaultAsync(m => m.agentid.id == agent.id && m.targetid.id == target.id);

            if (existingMission != null)
            {
                context.Missions.Remove(existingMission);
                await context.SaveChangesAsync();
            }
        }
    }
}