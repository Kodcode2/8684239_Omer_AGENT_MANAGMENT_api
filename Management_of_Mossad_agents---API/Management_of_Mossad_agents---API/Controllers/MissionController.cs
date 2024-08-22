using Management_of_Mossad_agents___API.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Management_of_Mossad_agents___API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionController : ControllerBase
    {
        private readonly ManagementOfMossadAgentsDbContext _context;

        public MissionController(ManagementOfMossadAgentsDbContext context)
        {
            _context = context;
        }

         public void SaveMissionToDb()
        {

        }

    }
}
