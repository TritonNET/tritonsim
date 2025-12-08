namespace TritonSim.GUI.Infrastructure
{
    public enum SimulationFlags
    {
        None = 0,
        
        Initializing = 1 << 0,
        
        Initialized = 1 << 1,

        Error = 1 << 2,
    }
}
