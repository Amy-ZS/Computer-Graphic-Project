#[compute]
#version 450

layout(local_size_x = 64, local_size_y = 1, local_size_z = 1) in;

layout(set = 0, binding = 0, std430) buffer PositionBuffer { vec4 positions[]; };
layout(set = 0, binding = 1, std430) buffer VelocityBuffer { vec4 velocities[]; };
layout(set = 0, binding = 2, std430) buffer DensityBuffer  { float densities[]; };
layout(set = 0, binding = 3) uniform sampler3D sdf_tex;

layout(push_constant) uniform Params {
    float radius;
    float particle_count;
    float delta_time;
    float target_density;
    float pressure_multiplier;
    float viscosity_strength;
    float damping;
    float idk1; //this is just padding
    vec4 bounds;
    vec4 sdf_origin;
    vec4 sdf_size;
} params;

//im using sebastion lagues smoothing kernel
float SmoothingKernel(float r, float dst) {
    if (dst >= r) return 0.0;
    float v = r - dst;
    return (v * v * v) / (r * r);
}

void main() {
    uint index = gl_GlobalInvocationID.x;
    if (index >= uint(params.particle_count)) return;

    vec3 pos = positions[index].xyz;
    vec3 vel = velocities[index].xyz;
    
    // calculate density
    float density = 0.001;
    //this is o(n^2) so we still need to fix this,
    //i will be using a grid (spacial hashing) in the future but we dont one for mvp
    for (int i = 0; i < int(params.particle_count); i++) {
        float dist = distance(positions[i].xyz, pos);
        density += SmoothingKernel(params.radius, max(dist, 0.0001));
    }
    densities[index] = density;

    // calculate forces
    float pressure = (density-params.target_density) * params.pressure_multiplier;
    vec3 force = vec3(0.0, -9.8, 0.0);

    for (int i = 0; i < int(params.particle_count); i++) {
        if (i == int(index)) continue;
        
        vec3 dir = positions[i].xyz - pos;
        float dist = length(dir);
        
        if (dist > params.radius || dist < 0.0001) continue;

        float neighbor_density = max(densities[i], 0.001);
        float neighbor_pressure = (neighbor_density - params.target_density) * params.pressure_multiplier;
        float shared_p = (pressure + neighbor_pressure) * .5;
        
        // pressure force
        vec3 push_dir = dir/dist;
        force -= push_dir * shared_p * SmoothingKernel(params.radius, dist)/neighbor_density;
        
        // viscosity
        force += (velocities[i].xyz - vel) * params.viscosity_strength * SmoothingKernel(params.radius, dist);
    }

    vec3 new_vel = vel + (force * params.delta_time);
    
    // velocity clamp to 50
    if (length(new_vel) > 50) {
        new_vel = normalize(new_vel) * 50;
    }
    

    vec3 next_pos = pos + (new_vel * params.delta_time);

    if (abs(next_pos.x) > params.bounds.x) { next_pos.x = sign(next_pos.x) * params.bounds.x; new_vel.x *= -params.damping; }
    if (abs(next_pos.y) > params.bounds.y) { next_pos.y = sign(next_pos.y) * params.bounds.y; new_vel.y *= -params.damping; }
    if (abs(next_pos.z) > params.bounds.z) { next_pos.z = sign(next_pos.z) * params.bounds.z; new_vel.z *= -params.damping; }

    for (int iteration = 0; iteration < 3; iteration++) {
        vec3 sdf_uv = (next_pos - params.sdf_origin.xyz) / params.sdf_size.xyz + 0.5;
        if (all(greaterThanEqual(sdf_uv, vec3(0.0))) && all(lessThanEqual(sdf_uv, vec3(1.0)))) {
            
            float dist = texture(sdf_tex, sdf_uv).r;
            float epsilon_dist = 0.15; 

            if (dist < epsilon_dist) {
                float e = 0.001; 
                
                vec3 grad = vec3(
                    texture(sdf_tex, sdf_uv + vec3(e, 0, 0)).r - texture(sdf_tex, sdf_uv - vec3(e, 0, 0)).r,
                    texture(sdf_tex, sdf_uv + vec3(0, e, 0)).r - texture(sdf_tex, sdf_uv - vec3(0, e, 0)).r,
                    texture(sdf_tex, sdf_uv + vec3(0, 0, e)).r - texture(sdf_tex, sdf_uv - vec3(0, 0, e)).r
                );

                vec3 fallback_normal = normalize(next_pos - params.sdf_origin.xyz);
                vec3 normal = (length(grad) > 0.0001) ? normalize(grad) : fallback_normal;

                float penetration = epsilon_dist - dist;
                next_pos += normal * (penetration + 0.001);

                if (penetration > 0.0) {
                    next_pos += normal * (penetration + 0.01);
                    float vDotN = dot(new_vel, normal);
                    if (vDotN < 0.0) {
                        new_vel -= normal * vDotN; 
                        new_vel *= 0.5; 
                    }
                }
            } else {
                break; // Outside the collision shell
            }
        }
    }
    
    positions[index] = vec4(next_pos, 0.0);
    velocities[index] = vec4(new_vel, 0.0);
}