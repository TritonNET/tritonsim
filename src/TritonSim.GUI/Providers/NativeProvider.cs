using Avalonia;
using System;
using System.Threading; // Required for Monitor
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

        public SimulationMode GetMode()
        {
            lock (m_lock) return m_mode;
        }

        public bool SetSize(Size size)
        {
            // [FIX 1] Critical Lock added here to prevent memory corruption
            lock (m_lock)
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
        }

        public bool SetBackgroundColor(uint rgb)
        {
            // [FIX 1] Critical Lock added here
            lock (m_lock)
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
        }

        public bool SetType(RendererType type)
        {
            if (m_mode == SimulationMode.Running)
                throw new InvalidOperationException("Renderer type cannot be changed while the simulation is running.");

            var lastMode = m_mode;
            lock (m_lock)
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
            lock (m_lock)
            {
                if (m_mode != SimulationMode.NotReady)
                    throw new InvalidOperationException("Window handle cannot be changed after initialization.");

                m_config.Handle = handle;
                return true;
            }
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

                if (m_context.Renderer == IntPtr.Zero)
                    throw new Exception("Native simulator returned an invalid renderer context.");

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
                    throw new InvalidOperationException($"Cannot start from state: {m_mode}");

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
                    throw new InvalidOperationException($"Cannot stop from state: {m_mode}");

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
                    m_context = default;
                    m_mode = SimulationMode.NotReady;
                    m_flags = SimulationFlags.None;
                    return true;
                }

                m_flags |= SimulationFlags.Error;
                return false;
            }
        }

        // [FIX 2] Non-Blocking Render Frame to prevent UI Freeze
        public bool RenderFrame()
        {
            // Instead of waiting ("blocking") for the lock, we only enter if it's free.
            // If the UI thread is holding the lock (e.g., resizing or stopping), 
            // we skip this frame. This prevents the "Freeze after a while".

            bool lockTaken = false;
            try
            {
                // Try to acquire lock for 0ms (instant check)
                Monitor.TryEnter(m_lock, 0, ref lockTaken);

                if (lockTaken)
                {
                    if (m_mode != SimulationMode.Running)
                        return false;

                    m_lastResponse = m_native.RenderFrame(ref m_context);

                    if (m_lastResponse.IsSuccess())
                        return true;

                    m_flags |= SimulationFlags.Error;
                    return false;
                }
                else
                {
                    // Lock was busy (UI is doing something). 
                    // Just skip this frame. The user won't notice a missing frame, 
                    // but they WILL notice a frozen UI.
                    return true;
                }
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(m_lock);
            }
        }

        public string GetLastError()
        {
            lock (m_lock)
            {
                return m_lastResponse.IsFailure() ? "Error" : string.Empty;
            }
        }
    }
}