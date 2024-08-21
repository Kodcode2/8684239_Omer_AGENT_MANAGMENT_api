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
    public class TargetsController : ControllerBase
    {
        private readonly ManagementOfMossadAgentsDbContext _context;

        public TargetsController(ManagementOfMossadAgentsDbContext context)
        {
            _context = context;
        }

        //יצירת מטרה
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult CreateTarget(Target target)
        {
            target.status = TargetStatus.Live;
            _context.Targets.Add(target);
            _context.SaveChanges();
            return StatusCode(
            StatusCodes.Status201Created,
            new { success = true, targetID = target.id }
            );
        }


        //הוספת מיקום למטרה לפי איי די
        [HttpPut("{id}/pin")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult AddLocationForTargetById(int id, Location location)
        {
            Target target = _context.Targets.FirstOrDefault(t => t.id == id);
            target.location = location;
            _context.Update(target);
            _context.SaveChanges();
            return Ok(
                new
                {
                    target
                });
        }
    }
}
