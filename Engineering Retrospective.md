# Engineering Retrospective

**Group 7:** Amy Shi, Larry He, Qinan Shen  
**Course:** CS 428/523 – Computer Graphics  
**Date:** May 2026

---

## What We Built

Fluid simulation using SPH (Smoothed Particle Hydrodynamics) on GPU compute shaders. Fly through a forest, watch water particles move, press F to pause and render the water surface.

**Two pillars:**
- **Physics:** SPH particles with pressure, viscosity, gravity
- **Rendering:** Particle view + marching cubes mesh on demand

---

## Playtest Feedback – What We Fixed

From 5 classmates:

| Problem | Our Fix |
|---------|---------|
| Render view <1 FPS | Lowered resolution, added progressive loading |
| Laptops lag/freeze | Added Low Quality Mode (2000 particles) |
| Particles fall through floor | Increased collision margin + safety checks |
| "fluid.exe" not obvious | Renamed to `Start_FluidSim.exe` |
| No quality settings | Added simple toggle in menu |

---

## What Went Wrong

**1. SPH is heavy**  
O(n²) neighbor checks killed us. Kept saying "we'll add spatial hashing next week." Never did.

**2. Cross-platform GPU is a nightmare**  
Runs different on Mac, Windows, Linux. We tested on one laptop. Exports crashed on other platforms.

**3. Marching cubes is slow**  
Particles: 60 FPS. Press F: <1 FPS. Should have cut this feature earlier.

**4. Debugging compute shaders sucks**  
Godot gives no useful errors. Black screens, random crashes. Wasted days on typos.

**5. Collisions break on low FPS**  
Particles move too far per frame and phase through walls. Never fully fixed.

---

## Features We Cut

| Feature | Why |
|---------|-----|
| Sound | No time |
| WebGL | Compute shaders not supported |
| Full environment collision | Too hard – kept box bounds |
| Real-time mesh | Too slow – now manual with F |
| Terrain generation | Never built |

---

## AI Tools – Helpful or Not?

**Helpful:**
- Translating C# to GLSL
- Generating 3D models from images
- Ray marching shader templates

**Not helpful:**
- AI gave wrong Godot API answers
- Wasted hours debugging AI hallucinations
- Compute shader collision code was confidently wrong

**Verdict:** Good for structure, bad for platform-specific details.

---

## What We Learned

1. **SPH needs spatial hashing** – O(n²) is not okay
2. **Test on target platform early** – GPU drivers are not the same
3. **Scope smaller** – cut mesh rendering earlier
4. **Optimize first, then polish** – we did the reverse
5. **Don't trust AI for low-level GPU code** – write it yourself

---

## Final Thoughts

We built a working fluid simulation. Particles move like water at 60 FPS. Collisions mostly work. Mesh render is slow but functional.

But we learned that optimization and cross-platform testing matter more than features. Next time: smaller scope, test everywhere, optimize from day one.

---

**GitHub:** https://github.com/Amy-ZS/Computer-Graphic-Project  
**Build:** `Start_FluidSim.exe`
