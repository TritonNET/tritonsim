using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TritonSim.GUI.Infrastructure
{
    public enum SimulationState
    {
        Unknown = 0,      // Default / Not loaded
        Initializing = 1, // Native init is in progress
        Initialized = 2,  // Native init success, but simulation is stopped
        Running = 3,      // Simulation loop is active
        Paused = 4,       // (Optional) Loop active but physics halted
        Error = 5         // Native init failed
    }
}
