# Final Crit – What We Learned

**Group 7:** Amy Shi, Larry He, Qinan Shen

---

## Quick Recap

We built a **fluid simulation** in a forest environment. Fly around with WASD + mouse, watch water particles move, press F to pause and render the water surface.

**Two pillars:**
- **Physics** – SPH particle simulation on GPU compute shaders
- **Rendering** – Metaball / marching cubes mesh from particle data

---

## What Went Wrong (And What We Learned)

### 1. SPH is heavy
Simulating fluids with particles is expensive. We should have optimized earlier instead of making it look pretty. Too many features, not enough perf work.

### 2. Cross-platform GPU is a nightmare
Our simulation runs differently on Mac, Windows, and Linux. GPU drivers, compute shader support, export settings – everything breaks differently. **Always test on your target platform early.**

### 3. Our scope was too large
We wanted sound, full environment collision, real-time mesh generation, WebGL export. Cut most of it. Kept the core water simulation.

### 4. Render view still lags
Particles run fine (60 FPS). Press F to render mesh? Drops to <1 FPS. Marching cubes is slow. Should have used a different approach or cut it earlier.

---

## AI Tools – Helpful and Not

**Helpful:**
- Translating C# to GLSL (Gemini)
- Generating 3D models from images
- Ray marching shader boilerplate

**Not helpful:**
- Debugging Godot RenderingDevice API – AI gave confident wrong answers
- Compute shader debugging – wasted hours on hallucinations

---

## Bottom Line

We built a working GPU fluid simulation. It runs okay on good hardware. But we learned that **optimization and cross-platform testing matter more than features** – and our scope was too big from the start.

Next time: smaller scope, test on target platform from week one, optimize before adding polish.

---

**Thanks for watching!**
