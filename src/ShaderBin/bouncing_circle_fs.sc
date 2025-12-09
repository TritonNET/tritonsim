$input v_color0, v_texcoord0

#include <bgfx_shader.sh>

void main()
{
    // 1. Remap UVs from 0.0 -> 1.0  to  -1.0 -> 1.0
    // This puts (0,0) exactly in the center of the quad.
    vec2 uv = v_texcoord0 * 2.0 - 1.0;

    // 2. Calculate distance from the center
    float dist = length(uv);

    // 3. Define the Circle Shape
    // We want the circle to fade out at radius 1.0.
    // smoothstep(edge0, edge1, x) returns 0 if x < edge0, 1 if x > edge1.
    // Here we want the opposite: 1 (opaque) inside, 0 (transparent) outside.
    
    float radius = 1.0;
    float softness = 0.05; // Adjust this for harder or softer edges
    
    // This creates a smooth gradient from 1.0 down to 0.0 at the edge
    float circleAlpha = 1.0 - smoothstep(radius - softness, radius, dist);

    // 4. Output
    // Combine the vertex color (v_color0) with our calculated alpha
    gl_FragColor = vec4(v_color0.rgb, v_color0.a * circleAlpha);
}