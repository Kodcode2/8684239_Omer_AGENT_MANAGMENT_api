using Management_of_Mossad_agents___API.Models;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace Management_of_Mossad_agents___API.DAL
{
    public class ManagementOfMossadAgentsDbContext : DbContext
    {
        public ManagementOfMossadAgentsDbContext(DbContextOptions<ManagementOfMossadAgentsDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Agent> Agents { get; set; }
        public DbSet<Mission> Missions { get; set; }
        public DbSet<Target> Targets { get; set; }

    }
}
