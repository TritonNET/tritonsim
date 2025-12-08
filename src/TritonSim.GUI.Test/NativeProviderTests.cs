using Avalonia;
using Moq;
using System.Drawing;
using TritonSim.GUI.Infrastructure;
using TritonSim.GUI.Providers;
using Xunit;

namespace TritonSim.GUI.Test
{
    public class NativeProviderTests
    {
        private readonly Mock<INativeSimulator> m_mockNative;
        private readonly NativeProvider m_provider;

        private delegate ResponseCode InitDelegate(ref SimConfig config, out SimContext ctx);
        private delegate ResponseCode UpdateConfigDelegate(ref SimContext ctx, ref SimConfig config);
        private delegate ResponseCode LifecycleDelegate(ref SimContext ctx);

        public NativeProviderTests()
        {
            m_mockNative = new Mock<INativeSimulator>();
            m_provider = new NativeProvider(m_mockNative.Object);
        }

        [Fact]
        public void Init_ShouldThrow_IfHandleIsZero()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => m_provider.Init());

            Assert.Contains("Handle is IntPtr.Zero", ex.Message);
        }

        [Fact]
        public void Init_ShouldThrow_IfAlreadyInitialized()
        {
            SetupSuccessfulInit();
            m_provider.Init();

            var ex = Assert.Throws<InvalidOperationException>(() => m_provider.Init());
            Assert.Contains("already initialized", ex.Message);
        }

        [Fact]
        public void Init_ShouldSucceed_WhenNativeReturnsSuccess()
        {
            m_provider.SetWindowHandle(new IntPtr(123));

            m_mockNative.Setup(x => x.Init(ref It.Ref<SimConfig>.IsAny, out It.Ref<SimContext>.IsAny))
                       .Returns(new InitDelegate((ref SimConfig config, out SimContext ctx) =>
                       {
                           ctx = new SimContext { };
                           return ResponseCode.Success;
                       }));

            bool result = m_provider.Init();

            Assert.True(result);
            Assert.Equal(SimulationMode.Ready, m_provider.GetMode());

            m_mockNative.Verify(x => x.Init(ref It.Ref<SimConfig>.IsAny, out It.Ref<SimContext>.IsAny), Times.Once);
        }

        [Fact]
        public void Init_ShouldFail_WhenNativeReturnsError()
        {
            m_provider.SetWindowHandle(new IntPtr(123));
            m_mockNative.Setup(x => x.Init(ref It.Ref<SimConfig>.IsAny, out It.Ref<SimContext>.IsAny))
                       .Returns(ResponseCode.Failed);

            bool result = m_provider.Init();

            Assert.False(result);
            Assert.Equal(SimulationMode.NotReady, m_provider.GetMode());
            Assert.Contains("Error: General Failure", m_provider.GetLastError());
        }

        [Fact]
        public void SetWindowHandle_ShouldThrow_IfAlreadyInitialized()
        {
            SetupSuccessfulInit();
            m_provider.Init();

            Assert.Throws<InvalidOperationException>(() => m_provider.SetWindowHandle(new IntPtr(999)));
        }

        [Fact]
        public void SetSize_ShouldUpdateConfigOnly_WhenNotReady()
        {
            var newSize = new Avalonia.Size(800, 600);

            bool result = m_provider.SetSize(newSize);

            Assert.True(result);

            m_mockNative.Verify(x => x.UpdateConfig(ref It.Ref<SimContext>.IsAny, ref It.Ref<SimConfig>.IsAny), Times.Never);
        }

        [Fact]
        public void SetSize_ShouldCallNativeUpdate_WhenReady()
        {
            SetupSuccessfulInit();
            m_provider.Init();

            m_mockNative.Setup(x => x.UpdateConfig(ref It.Ref<SimContext>.IsAny, ref It.Ref<SimConfig>.IsAny))
                       .Returns(ResponseCode.Success);

            bool result = m_provider.SetSize(new Avalonia.Size(1024, 768));

            Assert.True(result);
            m_mockNative.Verify(x => x.UpdateConfig(ref It.Ref<SimContext>.IsAny, ref It.Ref<SimConfig>.IsAny), Times.Once);
        }

        [Fact]
        public void SetSize_ShouldReportError_WhenNativeUpdateFails()
        {
            SetupSuccessfulInit();
            m_provider.Init();

            m_mockNative.Setup(x => x.UpdateConfig(ref It.Ref<SimContext>.IsAny, ref It.Ref<SimConfig>.IsAny))
                       .Returns(ResponseCode.Failed);

            bool result = m_provider.SetSize(new Avalonia.Size(100, 100));

            Assert.False(result);
        }

        [Fact]
        public void SetType_ShouldThrow_IfRunning()
        {
            SetupSuccessfulInit();
            m_provider.Init();
            SetupSuccessfulStart();
            m_provider.Start();

            Assert.Throws<InvalidOperationException>(() => m_provider.SetType(RendererType.RT_GAMEOFLIFE2D));
        }

        [Fact]
        public void SetType_ShouldJustUpdateConfig_WhenNotReady()
        {
            bool result = m_provider.SetType(RendererType.RT_GAMEOFLIFE2D);

            Assert.True(result);

            m_mockNative.Verify(x => x.Shutdown(ref It.Ref<SimContext>.IsAny), Times.Never);
            m_mockNative.Verify(x => x.Init(ref It.Ref<SimConfig>.IsAny, out It.Ref<SimContext>.IsAny), Times.Never);
        }

        [Fact]
        public void SetType_ShouldReinitialize_WhenReady()
        {
            SetupSuccessfulInit();
            m_provider.Init();

            m_mockNative.Setup(x => x.Shutdown(ref It.Ref<SimContext>.IsAny)).Returns(ResponseCode.Success);

            var capturedType = RendererType.RT_UNKNOWN;
            m_mockNative.Setup(x => x.Init(ref It.Ref<SimConfig>.IsAny, out It.Ref<SimContext>.IsAny))
                       .Returns(new InitDelegate((ref SimConfig c, out SimContext ctx) =>
                       {
                           capturedType = c.Type;
                           ctx = new SimContext();
                           return ResponseCode.Success;
                       }));

            bool result = m_provider.SetType(RendererType.RT_GAMEOFLIFE3D);

            Assert.True(result);
            Assert.Equal(SimulationMode.Ready, m_provider.GetMode());
            Assert.Equal(RendererType.RT_GAMEOFLIFE3D, capturedType);

            m_mockNative.Verify(x => x.Shutdown(ref It.Ref<SimContext>.IsAny), Times.Once);
            m_mockNative.Verify(x => x.Init(ref It.Ref<SimConfig>.IsAny, out It.Ref<SimContext>.IsAny), Times.Exactly(2));
        }

        [Fact]
        public void Start_ShouldThrow_IfNotReady()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => m_provider.Start());
            Assert.Contains("must be Ready", ex.Message);
        }

        [Fact]
        public void Start_ShouldSucceed_WhenReady()
        {
            SetupSuccessfulInit();
            
            m_provider.Init();
            
            SetupSuccessfulStart();

            bool result = m_provider.Start();

            Assert.True(result);
            Assert.Equal(SimulationMode.Running, m_provider.GetMode());
            
            m_mockNative.Verify(x => x.Start(ref It.Ref<SimContext>.IsAny), Times.Once);
        }

        [Fact]
        public void Stop_ShouldThrow_IfNotRunning()
        {
            SetupSuccessfulInit();
            m_provider.Init();

            Assert.Throws<InvalidOperationException>(() => m_provider.Stop());
        }

        [Fact]
        public void Stop_ShouldSucceed_WhenRunning()
        {
            SetupSuccessfulInit();
            m_provider.Init();
            SetupSuccessfulStart();
            m_provider.Start();

            m_mockNative.Setup(x => x.Stop(ref It.Ref<SimContext>.IsAny)).Returns(ResponseCode.Success);

            bool result = m_provider.Stop();

            Assert.True(result);
            Assert.Equal(SimulationMode.Ready, m_provider.GetMode());
        }

        [Fact]
        public void Shutdown_ShouldAutoStop_WhenRunning()
        {
            SetupSuccessfulInit();
            m_provider.Init();
            SetupSuccessfulStart();
            m_provider.Start();

            m_mockNative.Setup(x => x.Stop(ref It.Ref<SimContext>.IsAny)).Returns(ResponseCode.Success);
            m_mockNative.Setup(x => x.Shutdown(ref It.Ref<SimContext>.IsAny)).Returns(ResponseCode.Success);

            bool result = m_provider.Shutdown();

            Assert.True(result);
            Assert.Equal(SimulationMode.NotReady, m_provider.GetMode());

            m_mockNative.Verify(x => x.Stop(ref It.Ref<SimContext>.IsAny), Times.Once);
            m_mockNative.Verify(x => x.Shutdown(ref It.Ref<SimContext>.IsAny), Times.Once);
        }

        [Fact]
        public void Shutdown_ShouldFail_IfNativeShutdownFails()
        {
            SetupSuccessfulInit();
            m_provider.Init();

            m_mockNative.Setup(x => x.Shutdown(ref It.Ref<SimContext>.IsAny)).Returns(ResponseCode.Failed);

            bool result = m_provider.Shutdown();

            Assert.False(result);
            Assert.Equal(SimulationMode.Ready, m_provider.GetMode());
        }

        [Fact]
        public void RenderFrame_ShouldReturnFalse_IfNotRunning()
        {
            SetupSuccessfulInit();
            m_provider.Init();

            bool result = m_provider.RenderFrame();

            Assert.False(result);
            m_mockNative.Verify(x => x.RenderFrame(ref It.Ref<SimContext>.IsAny), Times.Never);
        }

        [Fact]
        public void RenderFrame_ShouldCallNative_WhenRunning()
        {
            SetupSuccessfulInit();

            m_provider.Init();
            
            SetupSuccessfulStart();
            
            m_provider.Start();

            m_mockNative.Setup(x => x.RenderFrame(ref It.Ref<SimContext>.IsAny)).Returns(ResponseCode.Success);

            bool result = m_provider.RenderFrame();

            Assert.True(result);
            m_mockNative.Verify(x => x.RenderFrame(ref It.Ref<SimContext>.IsAny), Times.Once);
        }

        [Fact]
        public void RenderFrame_ShouldReturnFalse_IfNativeFails()
        {
            SetupSuccessfulInit();
            
            m_provider.Init();
            
            SetupSuccessfulStart();
            
            m_provider.Start();

            m_mockNative.Setup(x => x.RenderFrame(ref It.Ref<SimContext>.IsAny)).Returns(ResponseCode.Failed);

            bool result = m_provider.RenderFrame();

            Assert.False(result);
        }

        private void SetupSuccessfulInit()
        {
            m_provider.SetWindowHandle(new IntPtr(1));
            m_mockNative.Setup(x => x.Init(ref It.Ref<SimConfig>.IsAny, out It.Ref<SimContext>.IsAny))
                       .Returns(ResponseCode.Success);
        }

        private void SetupSuccessfulStart()
        {
            m_mockNative.Setup(x => x.Start(ref It.Ref<SimContext>.IsAny))
                       .Returns(ResponseCode.Success);
        }
    }
}