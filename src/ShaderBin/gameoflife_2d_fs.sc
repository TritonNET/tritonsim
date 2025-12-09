$input v_texcoord0

#include <bgfx_shader.sh>

// Uniforms
uniform vec4 u_gridSize;   // .xy = Width, Height (e.g., 64.0, 64.0)
uniform vec4 u_colorAlive; // Color for living cells
uniform vec4 u_colorDead;  // Color for empty space

// The texture holding simulation state (updated by CPU)
SAMPLER2D(s_texState, 0);

void main()
{
    vec2 uv = v_texcoord0;
    
    // 1. Sample the Simulation Texture
    // Since we use Point Sampling in C++, this will return exactly 0.0 or 1.0
    float state = texture2D(s_texState, uv).r;

    // 2. Base Color Mixing
    vec4 finalColor = mix(u_colorDead, u_colorAlive, state);

    // 3. Draw Grid Lines (Optional but looks nice)
    // Calculates position within the specific cell (0.0 to 1.0)
    vec2 gridPos = fract(uv * u_gridSize.xy);
    
    // Thickness of the line relative to the cell size
    float lineThick = 0.05; 
    
    // Determine if we are on a border pixel
    float gridLine = step(1.0 - lineThick, gridPos.x) + step(1.0 - lineThick, gridPos.y);
    
    // Darken the color where there is a grid line
    finalColor.rgb *= (1.0 - clamp(gridLine, 0.0, 1.0) * 0.5);

    gl_FragColor = finalColor;
}