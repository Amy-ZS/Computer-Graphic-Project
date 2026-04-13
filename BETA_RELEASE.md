#  Assignment 5

**Source code:** https://github.com/Amy-ZS/Computer-Graphic-Project
Import the godot project to godot and export or run it

Our second pillar, which is the rendering, has already been implemented. We still only have the render for the fluids but we are close to getting one for the world.

We are also in the process of making our UI look better by using Godot's built in components.

## Our main problem

Our collisions are not really working with the environment yet and only works in a bounding box.

If I make water bounce less on a surface and the fps drops even a little bit, the particles just phase through the floor.

I have tried making the distance between the particle and the collision hitbox higher and that seems to work. However, that causes the water to float above objects, making it very unrealistic.

I also tried stopping the bouncing completely but even if the water particle stops at the floor, the next iteration would put it below the floor because of gravity.

If I keep the bouncing high, it looks less and less realistic because of the jittery-ness. And even with it, some water particles still find a way to phase through the floor. I am doing this fluid simulation in a compute shader to make it go faster so its also very hard to transfer this texture information.

## Stability

The build has gotten a little less optimized since our MVP but that is because we are trying to fix our collisions. However, it can still run 20k+ particles at 60+ fps. We already had the second pillar so it was not an issue keeping it in the constraints.
