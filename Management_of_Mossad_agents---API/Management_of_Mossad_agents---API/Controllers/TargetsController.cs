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
    [Route("api/[controller]")]
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
                new { success = true, targetID = target.id }
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

            target.location = location;
            _context.Update(target);
            await _context.SaveChangesAsync();
            return Ok(new { target });
        }


        //קבלת כל המטרות
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllTargets()
        {
            return StatusCode(
                StatusCodes.Status200OK,
                new
                {
                    targets = _context.Targets.Include(t => t.location)?.ToList()
                }
            );
        }


        //הזזת המטרה לכיוון מסוים
        [HttpPut("{id}/move")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MoveTargetByIdAsync(int id, [FromBody] Dictionary<string, string> moveData)
        {
            Target target = await _context.Targets
                                          .Include(t => t.location) 
                                          .FirstOrDefaultAsync(t => t.id == id); 
            if (target == null)
            {
                return NotFound(new { success = false, message = "Target not found" });
            }
            if (moveData.TryGetValue("direction", out string direction))
            {
                bool success = await PositionUpdater.UpdatePositionAsync(target, direction);
                if (!success)
                {
                    return BadRequest(new { success = false, message = "Invalid direction" });
                }
                _context.Update(target);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, target });
            }
            return BadRequest(new { success = false, message = "Direction not provided" });
        }
    }
}
