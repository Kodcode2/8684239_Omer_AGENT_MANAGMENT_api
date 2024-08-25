using Management_of_Mossad_agents___API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Management_of_Mossad_agents___API.Services
{
    public static class PositionUpdater
    {
        private static readonly Dictionary<string, (int dx, int dy)> DirectionUpdates = new Dictionary<string, (int dx, int dy)>
        {
            { "nw", (-1, 1) },
            { "n", (0, 1) },
            { "ne", (1, -1) },
            { "w", (-1, 0) },
            { "e", (1, 0) },
            { "sw", (-1, -1) },
            { "s", (0, -1) },
            { "se", (1, 1) }
        };


        //פונקציה עבור מטרה
        public static async Task<bool> UpdatePositionTargetAsync(Target target, string direction)
        {
            if (DirectionUpdates.TryGetValue(direction, out (int dx, int dy) update))
            {
                target.location.X += update.dx;
                target.location.Y += update.dy;
                return true;
            }

            return false;
        }


        //פונקציה עבור סוכן
        public static async Task<bool> UpdatePositionAgentAsync(Agent agent, string direction)
        {
            if (DirectionUpdates.TryGetValue(direction, out (int dx, int dy) update))
            {
                agent.location.X += update.dx;
                agent.location.Y += update.dy;
                return true;
            }

            return false;
        }
    }
}