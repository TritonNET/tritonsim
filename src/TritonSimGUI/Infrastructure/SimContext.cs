using System.Runtime.InteropServices;

namespace TritonSimGUI.Infrastructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SimContext
    {
        public IntPtr Renderer;

        public bool IsInitialized() => Renderer != IntPtr.Zero;
    }
}
