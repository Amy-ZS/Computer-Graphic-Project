# Beta Release Notes - GPU Fluid Simulation

**Assignment 5: Beta Build** | **Computer Graphics** | **April 2026**

---

## Quick Review: First Pillar

**GPU-Accelerated SPH Fluid Simulation**

My primary technical pillar is a real-time Smoothed Particle Hydrodynamics (SPH) fluid simulation running entirely on GPU compute shaders. Key achievements:

- **5,000+ particles** simulated simultaneously at 55-60 FPS
- **Compute shader architecture** with 64-thread local work groups
- **SPH physics forces**: Pressure, viscosity, gravity, and damping
- **SDF collision detection** using 3D textures (64³ resolution)
- **Complete game loop**: Start Menu → Active Simulation → End Menu

The simulation runs in `FluidSim.cs`, dispatching compute shaders that update particle positions, velocities, and densities every frame.

---

## Secondary Pillar Integration

**Custom Water Surface Shader**

My second pillar is a **shader** that provides visual cohesion for the fluid simulation.

### How It Integrates with the Core Loop

The shader is not a separate tech demo — it is **fully integrated** into the core experience:

| Integration Point | Description |
|-------------------|-------------|
| **Real-time Rendering** | The shader renders the `RenderedWaterMesh` continuously during gameplay |
| **Toggle View (F key)** | Pressing F freezes the simulation and renders a marching cubes mesh from particle data |


## New Features Since Alpha Build

**Core Simulation**

- **GPU compute shader implementation (moved physics from CPU to GPU)
- **SDF collision detection (fixed "really bad collisions" from Alpha)
- **Particle count increased from ~2,000 to 5,000+ stable
- **Fixed memory leaks in particle system
- **Removed gray/blue screen random rendering bug
- **Visual & Shader (Secondary Pillar)
- **Toggle between particle view and mesh render (F key)

**Game Loop & UI**

- **Complete game loop (Start Menu → Playing → End Menu)
- **Main menu with Start and Quit buttons

**Environment & Assets**

- **HDR backgrounds
- **CSGBox3D environment objects for collision
- **Skybox improvements

## Known Issues
| Issue | Details |
|-------------------|-------------|
| **Particle count limited to 5,000** | Performance constraint; stable at this count but could be higher with optimization |
| **SDF uses bounding spheres** | Not precise mesh collision, but acceptable for fluid simulation purposes |
| **Marching cubes mesh artifacts** | Mesh generation from particle data has minor visual glitches; particle view works correctly |
