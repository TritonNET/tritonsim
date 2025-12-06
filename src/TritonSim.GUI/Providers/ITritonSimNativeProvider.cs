using TritonSim.GUI.Infrastructure;

namespace TritonSim.GUI.Providers
{
    public interface ITritonSimNativeProvider
    {
        ResponseCode Init(ref SimConfig config, out SimContext ctx);

        ResponseCode UpdateConfig(ref SimContext ctx, ref SimConfig config);
        
        ResponseCode RenderFrame(ref SimContext ctx);
        
        ResponseCode Start(ref SimContext ctx);
        
        ResponseCode Stop(ref SimContext ctx);
        
        ResponseCode Shutdown(ref SimContext ctx);
    }
}
