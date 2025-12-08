using CommunityToolkit.Mvvm.ComponentModel;
using TritonSim.GUI.Infrastructure;

namespace TritonSim.GUI.ViewModels
{
    public class VmRendererType : ObservableObject
    {
        public RendererType Type { get; set; }

        public required string Name { get; set; }
    }
}
