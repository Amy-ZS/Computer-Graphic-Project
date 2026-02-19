using Godot;
using System;
using System.Threading.Tasks;

public partial class Main : Node3D
{
	//this is using the Smoothed Particle Hydrodynamics way of simulating fluids
	//i learned this from Sebastion Lague on yt, u guys should check it out to better understand it

	//i followed a bit of https://personal.ems.psu.edu/~fkd/courses/EGEE520/2017Deliverables/SPH_2017.pdf
	//we can base the sim off the equations there

	//mass is uniform
	struct Particle {
		public Vector3 position;
		public Vector3 velocity;
		public float density;
	}

	[Export] public Mesh mesh;
	[Export] public Material material;

	[Export] public float size = 0.1f;
	[Export] public float damping = 0.8f;
	[Export] public float gravity = -9.8f;
	[Export] public int count = 1000;

	//these are part of the SPH
	[Export] public float radius = 0.5f;
	[Export] public float pressure = 5f;
	[Export] public float viscosity = 0.1f;

	[Export] public Vector3 bounds = new(4, 4, 4);

	private Particle[] particles;
	private MultiMeshInstance3D matrices;

	private Vector3 half_bounds;
	private float half_size;

	public override void _Ready()
	{
		particles = new Particle[count];
		half_bounds = bounds/2;
		half_size = size/2;

		matrices = new MultiMeshInstance3D();
		var mm = new MultiMesh {
			TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
			InstanceCount = count,
			Mesh = mesh
		};
		matrices.Multimesh = mm;
		matrices.MaterialOverride = material;
		AddChild(matrices);

		var random = new RandomNumberGenerator();
		for (int i = 0; i < count; i++)
		{
			particles[i].position = new Vector3(
				random.RandfRange(-half_bounds.X, half_bounds.X),
				random.RandfRange(-half_bounds.Y, half_bounds.Y),
				random.RandfRange(-half_bounds.Z, half_bounds.Z)
			);
		}
	}

	public override void _Process(double delta)
	{
		//calculate density
		//ngl this is mega unoptimized, the multithreading is helping but we need to ultimately put this in a shader
		//this is good for now though as idk how to do shaders
		//we should not do shaders yet cus we should work on this sim in c# unless one of you want to switch early
		Parallel.For(0, count, i => {
			float d = 0;
			for (int j = 0; j < count; j++) {
				float dist = particles[i].position.DistanceTo(particles[j].position);
				if (dist < radius) {
					float influence = 1f - (dist / radius);
					d += influence * influence;
				}
			}
			particles[i].density = d;
		});

		for (int i = 0; i < count; i++) {
			Vector3 pressureForce = Vector3.Zero;

			//not done
			//also kinda sucks performance wise, the pdf has optimization stuff
			for (int j = 0; j < count; j++) {
				if (i == j) continue;

				Vector3 diff = particles[i].position - particles[j].position;
				float dist = diff.Length();

				if (dist < radius) {
					float influence = 1f - (dist/radius);
					pressureForce += diff.Normalized() * influence * pressure;
				}
			}

			particles[i].velocity += (pressureForce + new Vector3(0, gravity, 0)) * (float)delta;
			particles[i].velocity *= 1f - viscosity;
			particles[i].position += particles[i].velocity * (float)delta;

			HandleBounds(ref particles[i]);

			Transform3D t = Transform3D.Identity;
			t = t.Scaled(new Vector3(size, size, size));
			t.Origin = particles[i].position;
			matrices.Multimesh.SetInstanceTransform(i, t);
		}
	}

	//check box collisions
	//this is very temporary cus im not sure how you would handle collisions with other objects with a sim like this
	//it would tank performance so much
	private void HandleBounds(ref Particle p) {
		if (Mathf.Abs(p.position.X) + half_size > half_bounds.X) {
			p.position.X = (half_bounds.X - half_size) * Mathf.Sign(p.position.X);
			p.velocity.X *= -damping;
		}
		if (Mathf.Abs(p.position.Y) + half_size > half_bounds.Y) {
			p.position.Y = (half_bounds.Y - half_size) * Mathf.Sign(p.position.Y);
			p.velocity.Y *= -damping;
		}
		if (Mathf.Abs(p.position.Z) + half_size > half_bounds.Z) {
			p.position.Z = (half_bounds.Z - half_size) * Mathf.Sign(p.position.Z);
			p.velocity.Z *= -damping;
		}
	}
}
