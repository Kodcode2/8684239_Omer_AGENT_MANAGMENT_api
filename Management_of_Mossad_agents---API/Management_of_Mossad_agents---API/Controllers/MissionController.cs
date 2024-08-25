using Humanizer;
using Management_of_Mossad_agents___API.DAL;
using Management_of_Mossad_agents___API.Enums;
using Management_of_Mossad_agents___API.Models;
using Management_of_Mossad_agents___API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Management_of_Mossad_agents___API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase
    {
        private readonly ManagementOfMossadAgentsDbContext _context;

        public MissionsController(ManagementOfMossadAgentsDbContext context)
        {
            _context = context;
        }

        //הנעת סוכנים מצוותים לכיוון המטרות
        [HttpPost("update")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PromotingAgentToTargetAsync()
        {
            // שליפת המשימות העומדות בתנאי עם טעינת ה-Agent וה-Target
            List<Mission> missions = await _context.Missions
                .Include(m => m.agentid)  // טוען את ה-Agent הקשור
                .Include(m => m.targetid) // טוען את ה-Target הקשור
                .Where(m => m.status == MissionStatus.AssignForTheMission)
                .ToListAsync();

            foreach (var mission in missions)
            {
                mission.status = MissionStatus.ChaseUnderway;
                // הרצת לולאה לכל משימה בנפרד על גבי Task
                await HandleMissionAsync(mission);
            }

            // החזרת תשובה עם הצלחה
            return Ok(new { success = true });
        }

        private async Task HandleMissionAsync(Mission mission)
        {
            //מציאת הסוכן לפי האיי די שלו וטעינת המיקום שלו
            var agent = await _context.Agents
                .Include(a => a.location)
                .FirstOrDefaultAsync(a => a.id == mission.agentid.id);

            if (agent == null || agent.status != AgentStatus.InActivity)
            {
                return; // הסוכן לא נמצא או לא נמצא בפעילות
            }

            await TrackAndMoveAgentTowardsTargetAsync(mission, agent);
        }

        private async Task TrackAndMoveAgentTowardsTargetAsync(Mission mission, Agent agent)
        {
            double ax = agent.location.X;
            double ay = agent.location.Y;

            while (true)
            {
                // מציאת המטרה לפי האיי די שלה
                var target = await GetTargetAsync(mission.targetid.id);

                if (target == null)
                {
                    break; // המטרה לא נמצאה
                }

                (ax, ay) = await MoveAgentTowardsTargetAsync(agent, target.location, ax, ay);

                // בדיקה אם הסוכן הגיע למטרה
                if (ax == target.location.X && ay == target.location.Y)
                {
                    mission.status = MissionStatus.Ended; // עדכון סטטוס המשימה כהושלמה
                    agent.status = AgentStatus.Dormant; // הסוכן סיים את המשימה
                    target.status = TargetStatus.Eliminated; // עדכון סטטוס המטרה כחוסלה
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Agent has reached the target.");
                    break;
                }
            }
        }

        private async Task<Target> GetTargetAsync(int targetId)
        {
            var target = await _context.Targets
                .Include(t => t.location)
                .FirstOrDefaultAsync(t => t.id == targetId);

            return target;
        }

        private async Task<(double ax, double ay)> MoveAgentTowardsTargetAsync(Agent agent, Location targetLocation, double ax, double ay)
        {
            while (true)
            {
                double deltaX = targetLocation.X - ax;
                double deltaY = targetLocation.Y - ay;

                // הזזת הסוכן צעד אחד בכיוון המטרה
                if (deltaX > 0)
                {
                    ax += 1; // הזזה ימינה
                }
                else if (deltaX < 0)
                {
                    ax -= 1; // הזזה שמאלה
                }

                if (deltaY > 0)
                {
                    ay += 1; // הזזה למעלה
                }
                else if (deltaY < 0)
                {
                    ay -= 1; // הזזה למטה
                }

                // עדכון המיקום הנוכחי של הסוכן
                agent.location.X = ax;
                agent.location.Y = ay;
                await _context.SaveChangesAsync();

                // השהייה קצרה 
                await Task.Delay(1000);

                // בדיקה אם הסוכן הגיע למטרה
                if (ax == targetLocation.X && ay == targetLocation.Y)
                {
                    break;
                }
            }

            return (ax, ay); // החזרת הערכים המעודכנים
        }










        [HttpPut("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignMissionAsync(int id)
        {
            // שליפת המשימה עם טעינת ה-Agent וה-Target
            var mission = await _context.Missions
                .Include(m => m.agentid)
                .Include(m => m.targetid)
                .FirstOrDefaultAsync(m => m.id == id);

            if (mission == null)
            {
                return BadRequest(new { error = "המשימה לא נמצאה." });
            }

            // שליפת הסוכן והמטרה
            var agent = await _context.Agents
                .Include(a => a.location)
                .FirstOrDefaultAsync(a => a.id == mission.agentid.id);

            var target = await _context.Targets
                .Include(t => t.location)
                .FirstOrDefaultAsync(t => t.id == mission.targetid.id);

            if (agent == null || target == null)
            {
                return BadRequest(new { error = "הסוכן או המטרה לא נמצאו." });
            }

            // חישוב המרחק בין הסוכן למטרה
            double distance = ProposalToMission.CheckDistance(target, agent);

            // בדיקה אם המרחק קטן מ-200 ק"מ
            if (distance > 200)
            {
                _context.Missions.Remove(mission); // מחיקת המשימה
                await _context.SaveChangesAsync();
                return BadRequest(new
                {
                    error = "The distance is already greater than 200"
                });
            }

            // עדכון סטטוסים
            mission.status = MissionStatus.AssignForTheMission;
            mission.timeLeft = distance / 5;
            agent.status = AgentStatus.InActivity;
            target.status = TargetStatus.InPursuit;
            await _context.SaveChangesAsync();

            // מחיקת משימות אחרות עם אותו סוכן או מטרה בסטטוס 'הצעה לציוות'
            var missionsToRemove = await _context.Missions
                .Where(m => (m.agentid.id == agent.id || m.targetid.id == target.id) && m.status == MissionStatus.Proposal)
                .ToListAsync();

            _context.Missions.RemoveRange(missionsToRemove);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

    }
}
