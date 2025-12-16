using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Headless;
using Moq;
using System.Reflection;
using TritonSim.GUI.Controls;
using TritonSim.GUI.Infrastructure;
using TritonSim.GUI.Providers;
using Xunit;

namespace TritonSim.GUI.Test
{
    public class TritonSimRenderControlTests : IDisposable
    {
        private readonly Mock<ITritonSimNativeProvider> m_mockSimProvider;
        private readonly Mock<INativeCanvasProvider> m_mockWindowProvider;
        private readonly TritonSimRenderControl m_control;

        public TritonSimRenderControlTests()
        {
            // FIX: Only initialize the Application if it hasn't been initialized yet.
            // SetupWithLifetime returns AppBuilder, which is NOT IDisposable.
            if (Application.Current == null)
            {
                AppBuilder.Configure<Application>()
                    .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                    .SetupWithLifetime(new ClassicDesktopStyleApplicationLifetime());
            }

            m_mockSimProvider = new Mock<ITritonSimNativeProvider>();
            m_mockWindowProvider = new Mock<INativeCanvasProvider>();

            m_mockSimProvider.Setup(x => x.Init()).Returns(true);
            m_mockSimProvider.Setup(x => x.Start()).Returns(true);
            m_mockSimProvider.Setup(x => x.Stop()).Returns(true);
            m_mockSimProvider.Setup(x => x.SetSize(It.IsAny<Size>())).Returns(true);
            m_mockSimProvider.Setup(x => x.SetType(It.IsAny<RendererType>())).Returns(true);
            m_mockSimProvider.Setup(x => x.GetMode()).Returns(SimulationMode.Ready);

            m_control = new TritonSimRenderControl();

            InjectPrivateField(m_control, "NativeContainer", new Border());
            InjectPrivateField(m_control, "OverlayBorder", new StackPanel());
            InjectPrivateField(m_control, "OverlayTextBox", new TextBlock());

            m_control.SimProvider = m_mockSimProvider.Object;
            m_control.WindowProvider = m_mockWindowProvider.Object;
        }

        public void Dispose()
        {
            // AppBuilder does not need disposal in this context
        }

        private void InjectPrivateField(object target, string fieldName, object value)
        {
            var type = target.GetType();
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                        ?? type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private void SimulateAttachToVisualTree()
        {
            var method = typeof(TritonSimRenderControl).GetMethod(
                "OnAttachedToVisualTree",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(object), typeof(VisualTreeAttachmentEventArgs) },
                null
            );

            var dummyRoot = new Window();

            var args = new VisualTreeAttachmentEventArgs(dummyRoot, dummyRoot);

            method?.Invoke(m_control, new object[] { m_control, args });
        }

        private void SimulateDetachFromVisualTree()
        {
            var method = typeof(TritonSimRenderControl).GetMethod(
                "OnDetachedFromVisualTree",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(object), typeof(VisualTreeAttachmentEventArgs) },
                null
            );

            // FIX: Same here, provide valid arguments
            var dummyRoot = new Window();
            var args = new VisualTreeAttachmentEventArgs(dummyRoot, dummyRoot);

            method?.Invoke(m_control, new object[] { m_control, args });
        }

        [Fact]
        public void Initialization_ShouldNotHappen_WhenDependenciesAreMissing()
        {
            SimulateAttachToVisualTree();

            m_mockSimProvider.Verify(x => x.Init(), Times.Never);
        }

        [Fact]
        public void Initialization_ShouldTrigger_WhenAllConditionsMet()
        {
            SimulateAttachToVisualTree();

            m_control.Renderer = RendererType.RT_GAMEOFLIFE2D;
            m_mockSimProvider.Verify(x => x.SetType(RendererType.RT_GAMEOFLIFE2D), Times.Once);

            var dummyHandle = new IntPtr(12345);
            m_control.OnNativeHandleCreated(dummyHandle);
            m_mockSimProvider.Verify(x => x.SetWindowHandle(dummyHandle), Times.Once);

            m_control.Width = 800;
            m_control.Height = 600;

            var propInfo = typeof(Visual).GetProperty("Bounds");
            propInfo?.SetValue(m_control, new Rect(0, 0, 800, 600));

            var method = typeof(TritonSimRenderControl).GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);

            var args = new AvaloniaPropertyChangedEventArgs<Rect>(
                m_control,
                Visual.BoundsProperty,
                new Rect(),
                new BindingValue<Rect>(new Rect(0, 0, 100, 100)),
                BindingPriority.LocalValue
            );

            method?.Invoke(m_control, new object[] { args });

            m_mockSimProvider.Verify(x => x.Init(), Times.Once);
        }

        [Fact]
        public void Init_ShouldCall_SetWindowHandle_Before_Init()
        {
            SimulateAttachToVisualTree();
            m_control.Renderer = RendererType.RT_TEST_BOUNCING_CIRCLE;

            var method = typeof(TritonSimRenderControl).GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);

            var args = new AvaloniaPropertyChangedEventArgs<Rect>(
                 m_control,
                 Visual.BoundsProperty,
                 new Rect(),
                 new BindingValue<Rect>(new Rect(0, 0, 100, 100)),
                 BindingPriority.LocalValue
             );

            method?.Invoke(m_control, new object[] { args });

            // Act
            m_control.OnNativeHandleCreated(new IntPtr(99));

            // Assert
            m_mockSimProvider.Verify(x => x.SetWindowHandle(new IntPtr(99)), Times.Once);
            m_mockSimProvider.Verify(x => x.Init(), Times.Once);
        }

        [Fact]
        public void Start_ShouldCallProviderStart_WhenInitialized()
        {
            SimulateAttachToVisualTree();
            m_control.Renderer = RendererType.RT_TEST_EDGES;
            m_control.OnNativeHandleCreated(new IntPtr(1));

            var onPropChange = typeof(TritonSimRenderControl).GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);

            var args = new AvaloniaPropertyChangedEventArgs<Rect>(
                 m_control,
                 Visual.BoundsProperty,
                 new Rect(),
                 new BindingValue<Rect>(new Rect(0, 0, 100, 100)),
                 BindingPriority.LocalValue
             );

            onPropChange?.Invoke(m_control, new object[] { args });

            m_control.Start();

            m_mockSimProvider.Verify(x => x.Start(), Times.Once);
        }

        [Fact]
        public void Start_ShouldDoNothing_WhenNotInitialized()
        {
            SimulateAttachToVisualTree();

            m_control.Start();

            m_mockSimProvider.Verify(x => x.Start(), Times.Never);
        }

        [Fact]
        public void Stop_ShouldCallProviderStop_WhenInitialized()
        {
            SimulateAttachToVisualTree();

            m_control.Renderer = RendererType.RT_TEST_EDGES;
            m_control.OnNativeHandleCreated(new IntPtr(1));

            var onPropChange = typeof(TritonSimRenderControl).GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);

            var args = new AvaloniaPropertyChangedEventArgs<Rect>(
                 m_control,
                 Visual.BoundsProperty,
                 new Rect(),
                 new BindingValue<Rect>(new Rect(0, 0, 100, 100)),
                 BindingPriority.LocalValue
             );

            onPropChange?.Invoke(m_control, new object[] { args });

            m_control.Stop();

            m_mockSimProvider.Verify(x => x.Stop(), Times.Once);
        }

        [Fact]
        public void ChangingRendererType_ShouldReinitialize_IfAlreadyInitialized()
        {
            SimulateAttachToVisualTree();

            m_control.OnNativeHandleCreated(new IntPtr(1));

            var onPropChange = typeof(TritonSimRenderControl).GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);

            var args = new AvaloniaPropertyChangedEventArgs<Rect>(
                 m_control,
                 Visual.BoundsProperty,
                 new Rect(),
                 new BindingValue<Rect>(new Rect(0, 0, 100, 100)),
                 BindingPriority.LocalValue
             );

            onPropChange?.Invoke(m_control, new object[] { args });

            m_control.Renderer = RendererType.RT_GAMEOFLIFE2D;
            m_mockSimProvider.Verify(x => x.Init(), Times.Once);

            m_control.Renderer = RendererType.RT_GAMEOFLIFE3D;

            m_mockSimProvider.Verify(x => x.Shutdown(), Times.Once);
            m_mockSimProvider.Verify(x => x.SetType(RendererType.RT_GAMEOFLIFE3D), Times.Once);
            m_mockSimProvider.Verify(x => x.Init(), Times.Exactly(2));
        }

        [Fact]
        public void ChangingRendererType_ShouldNotInit_IfNotReadyYet()
        {
            m_control.Renderer = RendererType.RT_GAMEOFLIFE2D;

            m_mockSimProvider.Verify(x => x.SetType(RendererType.RT_GAMEOFLIFE2D), Times.Once);
            m_mockSimProvider.Verify(x => x.Init(), Times.Never);
            m_mockSimProvider.Verify(x => x.Shutdown(), Times.Never);
        }

        [Fact]
        public void InitFailure_ShouldShowOverlay()
        {
            m_mockSimProvider.Setup(x => x.Init()).Returns(false);
            m_mockSimProvider.Setup(x => x.GetLastError()).Returns("Failed to init native");

            SimulateAttachToVisualTree();
            m_control.Renderer = RendererType.RT_TEST_EDGES;
            m_control.OnNativeHandleCreated(new IntPtr(1));

            var onPropChange = typeof(TritonSimRenderControl).GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);

            var args = new AvaloniaPropertyChangedEventArgs<Rect>(
                 m_control,
                 Visual.BoundsProperty,
                 new Rect(),
                 new BindingValue<Rect>(new Rect(0, 0, 100, 100)),
                 BindingPriority.LocalValue
             );

            onPropChange?.Invoke(m_control, new object[] { args });

            m_mockSimProvider.Verify(x => x.Init(), Times.Once);
            m_mockSimProvider.Verify(x => x.GetLastError(), Times.AtLeast(2));

            var overlay = (StackPanel)typeof(TritonSimRenderControl)
                          .GetField("OverlayBorder", BindingFlags.NonPublic | BindingFlags.Instance)!
                          .GetValue(m_control)!;

            Assert.True(overlay.IsVisible);

            var textBox = (TextBlock)typeof(TritonSimRenderControl)
                          .GetField("OverlayTextBox", BindingFlags.NonPublic | BindingFlags.Instance)!
                          .GetValue(m_control)!;

            Assert.Equal("Failed to init native", textBox.Text);
        }

        [Fact]
        public void NativeHandleDestroyed_ShouldShutdownRenderer()
        {
            SimulateAttachToVisualTree();

            m_control.Renderer = RendererType.RT_TEST_EDGES;
            m_control.OnNativeHandleCreated(new IntPtr(1));

            var onPropChange = typeof(TritonSimRenderControl).GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);

            var args = new AvaloniaPropertyChangedEventArgs<Rect>(
                 m_control,
                 Visual.BoundsProperty,
                 new Rect(),
                 new BindingValue<Rect>(new Rect(0, 0, 100, 100)),
                 BindingPriority.LocalValue
             );

            onPropChange?.Invoke(m_control, new object[] { args });

            m_control.OnNativeHandleDestroyed();

            m_mockSimProvider.Verify(x => x.Shutdown(), Times.Once);
        }
    }
}