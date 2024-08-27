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
            List<Mission> missions = await _context.Missions
                .Include(m => m.agentid)  
                .Include(m => m.targetid) 
                .Where(m => m.status == MissionStatus.AssignForTheMission)
                .ToListAsync();

            foreach (var mission in missions)
            {
               
                await HandleMissionAsync(mission);
            }

            return Ok(new { success = true });
        }

        private async Task HandleMissionAsync(Mission mission)
        {
            var agent = await _context.Agents
                .Include(a => a.location)
                .FirstOrDefaultAsync(a => a.id == mission.agentid.id);

            if (agent == null || agent.status != AgentStatus.InActivity)
            {
                return; 
            }

            await MoveAgentTowardsTargetAsync(mission, agent);
        }

        private async Task MoveAgentTowardsTargetAsync(Mission mission, Agent agent)
        {
            // מציאת המטרה לפי האיי די שלה
            var target = await GetTargetAsync(mission.targetid.id);

            if (target == null)
            {
                return; 
            }

            double ax = agent.location.X;
            double ay = agent.location.Y;

            // חישוב המרחק בין הסוכן למטרה
            double differenceX = target.location.X - ax;
            double differenceY = target.location.Y - ay;

            
            if (differenceX > 0)
            {
                ax += 1; 
            }
            else if (differenceX < 0)
            {
                ax -= 1; 
            }

            if (differenceY > 0)
            {
                ay += 1; 
            }
            else if (differenceY < 0)
            {
                ay -= 1; 
            }

            // עדכון המיקום הנוכחי של הסוכן
            agent.location.X = ax;
            agent.location.Y = ay;
            double distance = ProposalToMission.CheckDistance(target, agent);
            mission.timeLeft = distance;
            
            await _context.SaveChangesAsync();

            // בדיקה אם הסוכן הגיע למטרה
            if (ax == target.location.X && ay == target.location.Y)
            {
                target.status = TargetStatus.Eliminated; 
                mission.status = MissionStatus.Ended; 
                agent.status = AgentStatus.Dormant; 
                await _context.SaveChangesAsync();
            }
        }

        private async Task<Target> GetTargetAsync(int targetId)
        {
            var target = await _context.Targets
                .Include(t => t.location)
                .FirstOrDefaultAsync(t => t.id == targetId);

            return target;
        }





        [HttpPut("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignMissionAsync(int id)
        {
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

            if (distance > 200)
            {
                _context.Missions.Remove(mission); 
                await _context.SaveChangesAsync();
                return BadRequest(new
                {
                    error = "המרחק כבר יותר מ-200"
                });
            }

            // עדכון סטטוסים
            mission.status = MissionStatus.AssignForTheMission;
            mission.timeLeft = distance / 5;
            agent.status = AgentStatus.InActivity;
            target.status = TargetStatus.InPursuit;
            await _context.SaveChangesAsync();

            // מחיקת משימות אחרות עם אותו סוכן או מטרה
            var missionsToRemove = await _context.Missions
                .Where(m => (m.agentid.id == agent.id || m.targetid.id == target.id) && m.status == MissionStatus.Proposal)
                .ToListAsync();

            _context.Missions.RemoveRange(missionsToRemove);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }




        // קבלת כל המשימות שהסטטוס שלהם בהצעה לציוות
        [HttpGet("proposal")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllMissionsProposalWithDetails()
        {
            var missions = await _context.Missions
                .Include(m => m.agentid)
                .Include(m => m.targetid)
                .Where(m => m.status == MissionStatus.Proposal)
                .ToListAsync();

            var detailedMissions = new List<object>();

            foreach (var mission in missions)
            {
                var agent = await _context.Agents
                    .Include(a => a.location)
                    .FirstOrDefaultAsync(a => a.id == mission.agentid.id);

                var target = await _context.Targets
                    .Include(t => t.location)
                    .FirstOrDefaultAsync(t => t.id == mission.targetid.id);

                if (agent == null || target == null)
                {
                    continue; 
                }

                // חישוב המרחק בין הסוכן למטרה
                double distance = ProposalToMission.CheckDistance(target, agent);

                // הוספת פרטי המשימה לרשימה
                detailedMissions.Add(new
                {
                    MissionId = mission.id,
                    AgentId = agent.id,
                    AgentName = agent.nickname,
                    AgentLocation = new { X = agent.location.X, Y = agent.location.Y },
                    TargetId = target.id,
                    TargetName = target.name,
                    TargetLocation = new { X = target.location.X, Y = target.location.Y },
                    Distance = distance,
                    Status = mission.status
                });
            }

            return StatusCode(
                StatusCodes.Status200OK,
                new
                {
                    missions = detailedMissions
                }
            );




        }


        // קבלת משימה לפי איי די

        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMissionDetails(int id)
        {
            var mission = await _context.Missions
                .Include(m => m.agentid)
                .Include(m => m.targetid)
                .FirstOrDefaultAsync(m => m.id == id);

            if (mission == null)
            {
                return NotFound(new { error = "המשימה לא נמצאה" });
            }

            return Ok(new
            {
                MissionId = mission.id,
                AgentId = mission.agentid.id,
                AgentName = mission.agentid.nickname,
                TargetId = mission.targetid.id,
                TargetName = mission.targetid.name,
                Status = mission.status,
                TimeLeft = mission.timeLeft
            });
        }
    }
}
