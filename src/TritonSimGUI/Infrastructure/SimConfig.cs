using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TritonSimGUI.Infrastructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SimConfig
    {
        public IntPtr Handle;

        public int Width;
        
        public int Height;

        public RendererType Type;
    }
}
