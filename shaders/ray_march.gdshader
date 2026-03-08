shader_type spatial;
render_mode unshaded, cull_disabled;

// Camera uniforms
uniform vec3 cam_pos;
uniform mat3 cam_inv;

// Scene uniforms  
uniform float time;
uniform vec2 resolution;

// Material properties
uniform vec3 water_color = vec3(0.2, 0.5, 0.8);
uniform vec3 deep_color = vec3(0.0, 0.1, 0.3);
uniform float water_density = 0.5;
uniform float absorption = 2.0;

// Ray marching parameters
uniform int max_steps = 64;
uniform float max_distance = 100.0;
uniform float epsilon = 0.001;

// Distance functions
float sdSphere(vec3 p, float r) {
    return length(p) - r;
}

float sdBox(vec3 p, vec3 b) {
    vec3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}

float sdPlane(vec3 p, vec3 n, float h) {
    return dot(p, n) - h;
}

// Fluid distance function - combines multiple shapes
float mapTheWorld(vec3 p) {
    float ground = sdPlane(p, vec3(0.0, 1.0, 0.0), -2.0);
    
    // Animated fluid surface
    float wave1 = sin(p.x * 2.0 + time) * cos(p.z * 2.0 + time) * 0.5;
    float wave2 = sin(p.x * 4.0 + time * 2.0) * 0.25;
    float wave3 = cos(p.z * 4.0 + time * 1.5) * 0.25;
    
    // Fluid volume (animated sphere with waves)
    vec3 fluid_center = vec3(0.0, sin(time) * 0.5, 0.0);
    float fluid = sdSphere(p - fluid_center, 2.0 + wave1 + wave2 + wave3);
    
    return min(ground, fluid);
}

// Calculate normal at point
vec3 getNormal(vec3 p) {
    vec2 e = vec2(epsilon, 0.0);
    return normalize(vec3(
        mapTheWorld(p + e.xyy) - mapTheWorld(p - e.xyy),
        mapTheWorld(p + e.yxy) - mapTheWorld(p - e.yxy),
        mapTheWorld(p + e.yyx) - mapTheWorld(p - e.yyx)
    ));
}

// Simple lighting
vec3 calculateLighting(vec3 p, vec3 normal, vec3 view_dir) {
    vec3 light_pos = vec3(5.0, 10.0, 5.0);
    vec3 light_dir = normalize(light_pos - p);
    
    // Ambient
    vec3 ambient = vec3(0.1);
    
    // Diffuse
    float diff = max(dot(normal, light_dir), 0.0);
    vec3 diffuse = diff * vec3(1.0);
    
    // Specular (Blinn-Phong)
    vec3 half_vec = normalize(light_dir + view_dir);
    float spec = pow(max(dot(normal, half_vec), 0.0), 32.0);
    vec3 specular = spec * vec3(1.0);
    
    // Shadow test
    float shadow = 1.0;
    float t = 0.1;
    for(int i = 0; i < 16; i++) {
        vec3 shadow_pos = p + light_dir * t;
        float dist = mapTheWorld(shadow_pos);
        if(dist < epsilon) {
            shadow = 0.3;
            break;
        }
        t += dist;
        if(t > length(light_pos - p)) break;
    }
    
    // Water absorption (deeper = darker)
    float depth_factor = exp(-absorption * max(0.0, p.y + 2.0));
    vec3 water_tint = mix(deep_color, water_color, depth_factor);
    
    return (ambient + (diffuse + specular) * shadow) * water_tint;
}

// Ray marching function
vec3 rayMarch(vec3 ro, vec3 rd) {
    float t = 0.0;
    vec3 color = vec3(0.0);
    
    for(int i = 0; i < max_steps; i++) {
        vec3 p = ro + rd * t;
        float dist = mapTheWorld(p);
        
        if(dist < epsilon) {
            // Hit something - calculate lighting
            vec3 normal = getNormal(p);
            vec3 view_dir = -rd;
            color = calculateLighting(p, normal, view_dir);
            
            // Add fog based on distance
            float fog = exp(-t * 0.05);
            color = mix(vec3(0.1, 0.2, 0.3), color, fog);
            break;
        }
        
        if(t > max_distance) {
            // Sky color based on ray direction
            float sky_gradient = rd.y * 0.5 + 0.5;
            color = mix(vec3(0.1, 0.2, 0.4), vec3(0.4, 0.6, 0.8), sky_gradient);
            break;
        }
        
        t += dist;
    }
    
    return color;
}

void fragment() {
    // Calculate ray direction
    vec2 uv = FRAGCOORD.xy / resolution.xy * 2.0 - 1.0;
    uv.x *= resolution.x / resolution.y;
    
    vec3 ro = cam_pos;
    vec3 rd = normalize(cam_inv * vec3(uv, -1.0));
    
    // Ray march
    vec3 color = rayMarch(ro, rd);
    
    // Output
    ALBEDO = color;
}