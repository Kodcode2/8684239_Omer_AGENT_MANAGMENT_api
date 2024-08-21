using Management_of_Mossad_agents___API.DAL;
using Management_of_Mossad_agents___API.Enums;
using Management_of_Mossad_agents___API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

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


        //יצירת סוכן
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult CreateAgent(Agent agent)
        {
            agent.status = AgentStatus.Dormant;
            _context.Agents.Add(agent);
            _context.SaveChanges();
            return StatusCode(
            StatusCodes.Status201Created,
            new { success = true, agentID = agent.id }
            );
        }


        //הוספת מיקום לסוכן לפי איי די
        [HttpPut("{id}/pin")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult AddLocationForAgentById(int id, Location location)
        {
            Agent agent = _context.Agents.FirstOrDefault(a => a.id == id);
            agent.location = location;
            _context.Update(agent);
            _context.SaveChanges();
            return Ok(
                new
                {
                    agent
                });
        }




    }
}
