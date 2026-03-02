shader_type spatial;
render_mode unshaded, cull_disabled, depth_draw_always;

uniform sampler2D particle_data;
uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear_mipmap;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform int count;
uniform float radius = 0.25;
uniform float smooth_k = 0.45;
uniform vec3 water_color : source_color = vec3(0.1, 0.4, 0.8);

float smin(float a, float b, float k) {
    float h = max(k - abs(a - b), 0.0) / k;
    return min(a, b) - h * h * h * k * (1.0 / 6.0);
}

float map(vec3 p) {
    float d = 1000.0;
    if (count <= 0) return 1000.0;
    
    for (int i = 0; i < count; i++) {
        ivec2 tex_coords = ivec2(i % 64, i / 64);
        vec3 p_pos = texelFetch(particle_data, tex_coords, 0).rgb;
        
        float sphere = length(p - p_pos) - radius;
        
        d = smin(d, sphere, smooth_k);
    }
    
    return d + 0.02; 
}

vec3 get_normal(vec3 p) {
    vec2 e = vec2(0.01, 0.0);
    return normalize(vec3(
        map(p + e.xyy) - map(p - e.xyy),
        map(p + e.yxy) - map(p - e.yxy),
        map(p + e.yyx) - map(p - e.yyx)
    ));
}

void fragment() {
    vec3 ro = CAMERA_POSITION_WORLD;
    vec3 world_v = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
    vec3 rd = normalize(world_v - ro);

    float depth_raw = texture(DEPTH_TEXTURE, SCREEN_UV).r;
    
    vec4 upos = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, depth_raw, 1.0);
    vec3 pixel_view_pos = upos.xyz / upos.w;
    vec3 pixel_world_pos = (INV_VIEW_MATRIX * vec4(pixel_view_pos, 1.0)).xyz;
    
    float scene_dist = distance(ro, pixel_world_pos);

    float t = FRONT_FACING ? distance(ro, world_v) : 0.0;
    
    float max_t = min(t + 50.0, scene_dist); 

    bool hit = false;
    vec3 p;

    for (int i = 0; i < 100; i++) {
        p = ro + rd * t;
        float d = map(p);
        
        if (d < 0.01) { 
            hit = true; 
            break; 
        }
        t += d;
        
        if (t >= max_t) break;
    }

    if (hit) {
        vec3 n = get_normal(p);
        
        vec4 ray_clip_pos = PROJECTION_MATRIX * VIEW_MATRIX * vec4(p, 1.0);
        DEPTH = ray_clip_pos.z / ray_clip_pos.w;

        vec2 refraction_uv = SCREEN_UV + (n.xy * 0.02); 
        vec3 screen_col = texture(SCREEN_TEXTURE, refraction_uv).rgb;
        
        float fresnel = pow(1.0 - max(dot(n, -rd), 0.0), 5.0);
        vec3 light_dir = normalize(vec3(0.5, 1.0, 0.2));
        float spec = pow(max(dot(reflect(rd, n), light_dir), 0.0), 64.0);
        
        vec3 base = mix(screen_col, water_color, 0.5); 
        ALBEDO = mix(base, vec3(1.0), fresnel * 0.4) + (spec * 0.8);
    } else {
        discard;
    }
}