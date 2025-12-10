$input v_texcoord0

#include <bgfx_shader.sh>

uniform vec4 u_gridSize;   // .xy = Width, Height (200, 200)
uniform vec4 u_colorAlive; 
uniform vec4 u_colorDead;  
SAMPLER2D(s_texState, 0);

void main()
{
    vec2 uv = v_texcoord0;
    
    // Sample simulation state
    float state = texture2D(s_texState, uv).r;
    vec4 finalColor = mix(u_colorDead, u_colorAlive, state);

    // --- Grid Lines ---
    vec2 gridPos = fract(uv * u_gridSize.xy);
    
    // Make lines thinner because we have 200 cells now
    float lineThick = 0.02; 
    
    float gridLine = step(1.0 - lineThick, gridPos.x) + step(1.0 - lineThick, gridPos.y);
    finalColor.rgb *= (1.0 - clamp(gridLine, 0.0, 1.0) * 0.3); // 0.3 = Subtle lines

    gl_FragColor = finalColor;
}