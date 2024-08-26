using Management_of_Mossad_agents___API.DAL;
using Management_of_Mossad_agents___API.Enums;
using Management_of_Mossad_agents___API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using Management_of_Mossad_agents___API.Services;

namespace Management_of_Mossad_agents___API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TargetsController : ControllerBase
    {
        private readonly ManagementOfMossadAgentsDbContext _context;

        public TargetsController(ManagementOfMossadAgentsDbContext context)
        {
            _context = context;
        }

        // יצירת מטרה 
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateTargetAsync(Target target)
        {
            target.status = TargetStatus.Live;
            await _context.Targets.AddAsync(target);
            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new { Id = target.id }
            );
        }

        // הוספת מיקום למטרה לפי איי די 
        [HttpPut("{id}/pin")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddLocationForTargetByIdAsync(int id, Location location)
        {
            Target target = await _context.Targets.FirstOrDefaultAsync(t => t.id == id);
            if (target == null)
            {
                return NotFound(new { success = false, message = "Target not found" });
            }
            else if (location.X < 0 || location.Y < 0 || location.X > 1000 || location.Y > 1000)
            {
                return BadRequest("Out of range coordinates");
            }

            target.location = location;
            _context.Update(target);
            await _context.SaveChangesAsync();



            // הפנייה לבדיקת יצירת משימה

            //if (target.status != TargetStatus.Eliminated)
                if (target.status == TargetStatus.Live)
            {
                List<Agent> agents = _context.Agents.Include(a => a.location).Where(a => a.status == AgentStatus.Dormant).ToList();
                List<Mission> missions = await ProposalToMission.CheckByTargetAsync(target, agents, _context);
                if (missions != null && missions.Count > 0)
                {
                    // הוספת המשימות החדשות 
                    _context.Missions.AddRange(missions);
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { target });
        }



        //קבלת כל המטרות
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTargets()
        {
            var targets = await _context.Targets.Include(t => t.location).ToListAsync();

            return StatusCode(
                StatusCodes.Status200OK,
                new
                {
                    targets = targets
                }
            );
        }







        //הזזת המטרה לכיוון מסוים
        [HttpPut("{id}/move")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MoveTargetByIdAsync(int id, [FromBody] Dictionary<string, string> moveData)
        {
            Target target = await _context.Targets.Include(t => t.location).FirstOrDefaultAsync(t => t.id == id);
            if (target == null)
            {
                return NotFound(new { success = false, message = "Target not found" });
            }
            if (moveData.TryGetValue("direction", out string direction))
            {
                bool success = await PositionUpdater.UpdatePositionTargetAsync(target, direction);
                if (!success)
                {
                    return BadRequest(new { success = false, message = "Invalid direction" });
                }
                _context.Update(target);
                await _context.SaveChangesAsync();


                // הפנייה לבדיקת יצירת משימה

                //if (target.status != TargetStatus.Eliminated)
                if (target.status == TargetStatus.Live)
                {
                    List<Agent> agents = _context.Agents.Include(a => a.location).Where(a => a.status == AgentStatus.Dormant).ToList();
                    List<Mission> missions = await ProposalToMission.CheckByTargetAsync(target, agents, _context);
                    if (missions != null && missions.Count > 0)
                    {
                        // הוספת המשימות החדשות 
                        _context.Missions.AddRange(missions);
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok(new { success = true, target });
            }
            return BadRequest(new { success = false, message = "Direction not provided" });
        }
    }
}
