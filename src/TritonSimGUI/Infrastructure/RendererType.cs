namespace TritonSimGUI.Infrastructure
{
    [Flags]
    public enum RendererType : uint
    {
        // Groups
        RT_TESTS = 0x80000000, // Test Renderers
        RT_2DSIM = 0x40000000, // All 2D simulations
        RT_3DSIM = 0x20000000, // All 3D simulations
        RT_RESER = 0x10000000, // Reserved

        RT_UNKNOWN = 0x00,

        // Individual renderers
        RT_TEST_COLOR_CHANGING = RT_TESTS | 0x01,
        RT_TEST_EDGES = RT_TESTS | 0x02,
        RT_TEST_BOUNCING_CIRCLE = RT_TESTS | 0x03,

        RT_GAMEOFLIFE2D = RT_2DSIM | 0x01,
        RT_GAMEOFLIFE3D = RT_3DSIM | 0x01
    }
}