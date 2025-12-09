$input v_texcoord0

#include <bgfx_shader.sh>

// Define uniforms we will send from C++
// u_time.x = elapsed time
uniform vec4 u_time;
uniform vec4 u_screenParams; // .xy = width, height

// Helper function from original code
vec3 palette(float t) {
    vec3 a = vec3(0.5, 0.5, 0.5);
    vec3 b = vec3(0.5, 0.5, 0.5);
    vec3 c = vec3(1.0, 1.0, 1.0);
    vec3 d = vec3(0.263, 0.416, 0.557);
    return a + b * cos(6.28318 * (c * t + d));
}

void main() {
    // 1. Convert BGFX UVs (0..1) to Centered coords (-1..1)
    // In ShaderToy: (fragCoord * 2.0 - iResolution.xy) / iResolution.y;
    
    vec2 uv = v_texcoord0 * 2.0 - 1.0;
    
    // 2. Fix Aspect Ratio
    uv.x *= u_screenParams.x / u_screenParams.y;

    vec2 uv0 = uv;
    vec3 finalColor = vec3(0.0, 0.0, 0.0);
    
    // 3. The Algorithm (Ported from your snippet)
    float time = u_time.x;

    for (float i = 0.0; i < 4.0; i++) {
        uv = fract(uv * 1.5) - 0.5;

        float d = length(uv) * exp(-length(uv0));

        vec3 col = palette(length(uv0) + i * 0.4 + time * 0.4);

        d = sin(d * 8.0 + time) / 8.0;
        d = abs(d);

        d = pow(0.01 / d, 1.2);

        finalColor += col * d;
    }

    gl_FragColor = vec4(finalColor, 1.0);
}