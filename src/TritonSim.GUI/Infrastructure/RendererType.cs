using System;
using System.ComponentModel;

namespace TritonSim.GUI.Infrastructure
{
    [Flags]
    public enum RendererType : uint
    {
        // Groups
        RT_TESTS = 0x80000000, // Test Renderers
        RT_2DSIM = 0x40000000, // All 2D simulations
        RT_3DSIM = 0x20000000, // All 3D simulations
        RT_RESER = 0x10000000, // Reserved

        RT_MASK_ID = 0x0FFFFFFF,

        RT_UNKNOWN = 0x00,

        // Individual renderers
        [Description("Test Color Changing")]
        RT_TEST_COLOR_CHANGING = RT_TESTS | 0x01,

        [Description("Test Edges")]
        RT_TEST_EDGES = RT_TESTS | 0x02,

        [Description("Test Bouncing Circle")]
        RT_TEST_BOUNCING_CIRCLE = RT_TESTS | 0x03,

        [Description("2D Game of Life")]
        RT_GAMEOFLIFE2D = RT_2DSIM | 0x01,

        [Description("3D Game of Life")]
        RT_GAMEOFLIFE3D = RT_3DSIM | 0x01
    }
}