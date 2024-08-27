using Management_of_Mossad_agents___API.DAL;
using Management_of_Mossad_agents___API.Enums;
using Management_of_Mossad_agents___API.Models;
using Management_of_Mossad_agents___API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Management_of_Mossad_agents___API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        private readonly ManagementOfMossadAgentsDbContext _context;

        public AgentsController(ManagementOfMossadAgentsDbContext context)
        {
            _context = context;
        }

        // יצירת סוכן
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateAgentAsync(Agent agent)
        {
            agent.status = AgentStatus.Dormant;
            await _context.Agents.AddAsync(agent);
            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new { Id = agent.id }
            );
        }

        // הוספת מיקום לסוכן לפי איי די 
        [HttpPut("{id}/pin")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddLocationForAgentByIdAsync(int id, Location location)
        {
            Agent agent = await _context.Agents.FirstOrDefaultAsync(a => a.id == id);
            if (agent == null && agent.status == AgentStatus.InActivity)
            {
                return NotFound(new { success = false, message = "Agent not found or in activity" });
            }
            else if (location.X < 0 || location.Y < 0 || location.X > 1000 || location.Y > 1000)
            {
                return BadRequest("Out of range coordinates");
            }
            agent.location = location;
            _context.Update(agent);
            await _context.SaveChangesAsync();

            // הפנייה לבדיקת יצירת משימה
            List<Target> targets = _context.Targets.Include(t => t.location).Where(t => t.status == TargetStatus.Live).ToList();
            List<Mission> missions = await ProposalToMission.CheckByAgentAsync(agent, targets, _context);
            if (missions != null && missions.Count > 0)
            {
                // הוספת המשימות החדשות 
                _context.Missions.AddRange(missions);
                await _context.SaveChangesAsync();
            }



            return Ok(new { agent });
        }


        //קבלת כל הסוכנים
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAgents()
        {
            var agents = await _context.Agents.Include(a => a.location).ToListAsync();

            var detailedAgents = new List<object>();

            foreach (var agent in agents)
            {
                var activeMission = await _context.Missions
                    .Where(m => m.agentid.id == agent.id && m.status == MissionStatus.AssignForTheMission)
                    .FirstOrDefaultAsync();

                double? timeToEliminate = null;

                if (activeMission != null)
                {
                    timeToEliminate = activeMission.timeLeft;
                }

                // חישוב כמות חיסולים
                var eliminationsCount = await _context.Missions
                    .Where(m => m.agentid.id == agent.id && m.status == MissionStatus.Ended && m.targetid.status == TargetStatus.Eliminated)
                    .CountAsync();

                detailedAgents.Add(new
                {
                    AgentId = agent.id,
                    AgentName = agent.nickname,
                    Location = new { X = agent.location?.X, Y = agent.location?.Y },
                    Status = agent.status,
                    PhotoUrl = agent.photoUrl,
                    MissionId = activeMission?.id,
                    TimeToEliminate = timeToEliminate,
                    EliminationsCount = eliminationsCount
                });
            }

            return StatusCode(
                StatusCodes.Status200OK,
                new
                {
                    agents = detailedAgents
                }
            );
        }





        //הזזת הסוכן לכיוון מסוים
        [HttpPut("{id}/move")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MoveAgentByIdAsync(int id, [FromBody] Dictionary<string, string> moveData)
        {
            Agent agent = await _context.Agents.Include(a => a.location).FirstOrDefaultAsync(a => a.id == id);
            if (agent == null)
            {
                return NotFound(new { success = false, message = "Agent not found" });
            }
            if (moveData.TryGetValue("direction", out string direction) && agent.status == AgentStatus.Dormant)
            {
                bool success = await PositionUpdater.UpdatePositionAgentAsync(agent, direction);
                if (!success)
                {
                    return BadRequest(new { success = false, message = "Invalid direction or agent in activity" });
                }
                _context.Update(agent);
                await _context.SaveChangesAsync();

                // הפנייה לבדיקת יצירת משימה
                List<Target> targets = _context.Targets.Include(t => t.location).Where(t => t.status == TargetStatus.Live).ToList();
                List<Mission> missions = await ProposalToMission.CheckByAgentAsync(agent, targets, _context);
                if (missions != null && missions.Count > 0)
                {
                    // הוספת המשימות החדשות 
                    _context.Missions.AddRange(missions);
                    await _context.SaveChangesAsync();
                }



                return Ok(new { success = true, agent });
            }
            return BadRequest(new { success = false, message = "Direction not provided" });
        }


    }
}
