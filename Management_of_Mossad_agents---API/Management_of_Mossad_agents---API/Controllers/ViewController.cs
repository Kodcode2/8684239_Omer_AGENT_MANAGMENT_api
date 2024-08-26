using Management_of_Mossad_agents___API.DAL;
using Management_of_Mossad_agents___API.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Management_of_Mossad_agents___API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ViewController : ControllerBase
    {
        private readonly ManagementOfMossadAgentsDbContext _context;

        public ViewController(ManagementOfMossadAgentsDbContext context)
        {
            _context = context;
        }

        [HttpGet("overview")]
        public IActionResult GetOverview()
        {
            var totalAgents = _context.Agents.Count();
            var activeAgents = _context.Agents.Count(a => a.status == AgentStatus.InActivity);

            var totalTargets = _context.Targets.Count();
            var eliminatedTargets = _context.Targets.Count(t => t.status == TargetStatus.Eliminated);

            var totalMissions = _context.Missions.Count();
            var activeMissions = _context.Missions.Count(m => m.status == MissionStatus.AssignForTheMission);

            var agentToTargetRatio = totalAgents / totalTargets;
               

            // חישוב סוכנים שיכולים להיות מצוותים למטרות אבל עדיין לא צוותו
            var agentsAvailable = _context.Missions
                .Include(m => m.agentid).Where(m => m.status == MissionStatus.Proposal)
                .ToList();

            var agentsAvailableCount = agentsAvailable.Count();

            var availableRatio = agentsAvailableCount / totalTargets;


            var overview = new
            {
                TotalAgents = totalAgents,
                ActiveAgents = activeAgents,
                TotalTargets = totalTargets,
                EliminatedTargets = eliminatedTargets,
                TotalMissions = totalMissions,
                ActiveMissions = activeMissions,
                AgentToTargetRatio = agentToTargetRatio,
                AvailableAgentToTargetRatio = availableRatio
            };

            return Ok(overview);
        }
    }
}
