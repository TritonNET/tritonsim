using Avalonia;
using System;
using TritonSim.GUI.Infrastructure;

namespace TritonSim.GUI.Providers
{
    public class NativeProvider : ITritonSimNativeProvider
    {
        private readonly object m_lock = new();

        private SimConfig m_config;
        private SimContext m_context;
        private SimulationFlags m_flags;
        private ResponseCode m_lastResponse;
        private SimulationMode m_mode = SimulationMode.NotReady;

        private readonly INativeSimulator m_native;

        public NativeProvider(INativeSimulator native)
        {
            m_native = native;
        }

        public SimulationMode GetMode() => m_mode;

        public bool SetSize(Size size)
        {
            m_config.Width = (int)size.Width;
            m_config.Height = (int)size.Height;

            if (m_mode == SimulationMode.NotReady)
                return true;

            m_lastResponse = m_native.UpdateConfig(ref m_context, ref m_config);

            if (m_lastResponse.IsSuccess())
                return true;

            m_flags |= SimulationFlags.Error;
            return false;
        }

        public bool SetBackgroundColor(uint rgb)
        {
            m_config.BackgroundColor = rgb;

            if (m_mode == SimulationMode.NotReady)
                return true;

            m_lastResponse = m_native.UpdateConfig(ref m_context, ref m_config);

            if (m_lastResponse.IsSuccess())
                return true;

            m_flags |= SimulationFlags.Error;
            return false;
        }

        public bool SetType(RendererType type)
        {
            if (m_mode == SimulationMode.Running)
                throw new InvalidOperationException("Renderer type cannot be changed while the simulation is running.");

            var lastMode = m_mode;
            lock(m_lock)
            {
                if (lastMode == SimulationMode.Ready && !Shutdown())
                    return false;
            }

            m_config.Type = type;

            if (lastMode == SimulationMode.Ready && !Init())
                return false;

            return true;
        }


        public bool SetWindowHandle(IntPtr handle)
        {
            if (m_mode != SimulationMode.NotReady)
                throw new InvalidOperationException("Window handle cannot be changed after initialization.");

            m_config.Handle = handle;

            return true;
        }


        public bool Init()
        {
            lock (m_lock)
            {
                if (m_mode != SimulationMode.NotReady)
                    throw new InvalidOperationException("Simulation is already initialized.");

                if (m_config.Handle == IntPtr.Zero)
                    throw new InvalidOperationException("Config.Handle is IntPtr.Zero. Set the Config property before calling Init().");

                m_flags |= SimulationFlags.Initializing;

                m_lastResponse = m_native.Init(ref m_config, out m_context);

                if (!m_lastResponse.IsSuccess())
                {
                    m_flags = SimulationFlags.Error;
                    m_mode = SimulationMode.NotReady;
                    return false;                    
                }

                if(m_context.Renderer == IntPtr.Zero)
                    throw new ContextMarshalException("Native simulator returned an invalid renderer context.");

                m_flags = SimulationFlags.Initialized;
                m_mode = SimulationMode.Ready;
                return true;
            }
        }

        public bool Start()
        {
            lock (m_lock)
            {
                if (m_mode != SimulationMode.Ready)
                    throw new InvalidOperationException($"Cannot start simulation from state: {m_mode}. It must be Ready.");

                m_lastResponse = m_native.Start(ref m_context);

                if (m_lastResponse.IsSuccess())
                {
                    m_mode = SimulationMode.Running;
                    return true;
                }

                m_flags |= SimulationFlags.Error;
                return false;
            }
        }

        public bool Stop()
        {
            lock (m_lock)
            {
                if (m_mode != SimulationMode.Running)
                    throw new InvalidOperationException($"Cannot stop simulation when it is not Running.");

                m_lastResponse = m_native.Stop(ref m_context);

                if (m_lastResponse.IsSuccess())
                {
                    m_mode = SimulationMode.Ready;
                    return true;
                }

                m_flags |= SimulationFlags.Error;
                return false;
            }
        }

        public bool Shutdown()
        {
            lock (m_lock)
            {
                if (m_mode == SimulationMode.Running && !Stop())
                    return false;

                m_lastResponse = m_native.Shutdown(ref m_context);

                if (m_lastResponse.IsSuccess())
                {
                    // Reset local state completely on success
                    m_context = default;
                    m_mode = SimulationMode.NotReady;
                    m_flags = SimulationFlags.None;
                    return true;
                }

                // If shutdown failed native side, we are in an unstable state
                m_flags |= SimulationFlags.Error;

                return false;
            }
        }

        public bool RenderFrame()
        {
            lock (m_lock)
            {
                if (m_mode != SimulationMode.Running)
                    return false;

                m_lastResponse = m_native.RenderFrame(ref m_context);

                if (m_lastResponse.IsSuccess())
                    return true;

                m_flags |= SimulationFlags.Error;

                return false;
            }
        }

        public string GetLastError()
        {
            return m_lastResponse.IsFailure() ? m_lastResponse.ToFriendlyError() : string.Empty;
        }
    }
}
