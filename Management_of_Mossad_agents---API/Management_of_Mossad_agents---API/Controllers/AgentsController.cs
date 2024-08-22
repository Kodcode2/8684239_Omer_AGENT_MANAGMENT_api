using Management_of_Mossad_agents___API.DAL;
using Management_of_Mossad_agents___API.Enums;
using Management_of_Mossad_agents___API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Management_of_Mossad_agents___API.Controllers
{
    [Route("api/[controller]")]
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
                new { success = true, agentID = agent.id }
            );
        }

        // הוספת מיקום לסוכן לפי איי די 
        [HttpPut("{id}/pin")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddLocationForAgentByIdAsync(int id, Location location)
        {
            Agent agent = await _context.Agents.FirstOrDefaultAsync(a => a.id == id);
            if (agent == null)
            {
                return NotFound(new { success = false, message = "Agent not found" });
            }

            agent.location = location;
            _context.Update(agent);
            await _context.SaveChangesAsync();
            return Ok(new { agent });
        }


        //קבלת כל הסוכנים
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllAgents()
        {
            return StatusCode(
                StatusCodes.Status200OK,
                new
                {
                    agents = _context.Agents.Include(t => t.location)?.ToList()
                }
            );
        }
    }
}
