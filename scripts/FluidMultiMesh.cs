using Godot;
using System;
using System.Runtime.InteropServices;

public partial class FluidMultiMesh : MultiMeshInstance3D
{
	// Compute shader resources
	private RenderingDevice rd;
	private Rid shader;
	private Rid pipeline;
	private Rid positionBuffer;
	private Rid velocityBuffer;
	private Rid densityBuffer;
	private Rid uniformSet;
	
	private float[] positionData;
	private float[] velocityData;
	private int particleCount = 15000; // Reduced for performance
	private float particleSize = 0.12f;
	
	[ExportGroup("Simulation Settings")]
	[Export] public bool RunSimulation = true;
	[Export] public float Gravity = -9.8f;
	[Export] public float ParticleRadius = 0.25f;
	[Export] public float TargetDensity = 1.0f;
	[Export] public float PressureMultiplier = 0.8f;
	[Export] public float ViscosityStrength = 0.2f;
	[Export] public float Damping = 0.6f;
	
	[ExportGroup("Bounds")]
	[Export] public Vector3 Bounds = new Vector3(2f, 2f, 2f);
	[Export] public bool ShowBounds = true;
	
	[ExportGroup("Visuals")]
	[Export] public Color ParticleColor = new Color(0.2f, 0.6f, 1.0f);
	
	private MeshInstance3D boundsVisualization;
	private Random random = new Random();
	private bool initialized = false;
	private float timeAccumulator = 0f;
	private bool useComputeShader = false; // Set to false to use CPU simulation
	
	public override void _Ready()
	{
		GD.Print("=== FLUID MULTIMESH INITIALIZING ===");
		GD.Print($"Particle Count: {particleCount}");
		GD.Print($"Bounds: {Bounds}");
		
		SetupMultiMesh();
		InitializeParticles();
		
		if (useComputeShader)
		{
			SetupComputeShader();
		}
		
		if (ShowBounds)
		{
			SetupBoundsVisualization();
		}
		
		initialized = true;
		GD.Print("Initialization complete!");
	}
	
	private void SetupMultiMesh()
	{
		var multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		
		var sphereMesh = new SphereMesh();
		sphereMesh.Radius = particleSize;
		sphereMesh.Height = particleSize * 2;
		sphereMesh.Material = CreateMaterial();
		
		multiMesh.Mesh = sphereMesh;
		multiMesh.InstanceCount = particleCount;
		multiMesh.VisibleInstanceCount = particleCount;
		
		this.Multimesh = multiMesh;
	}
	
	private Material CreateMaterial()
	{
		var material = new StandardMaterial3D();
		material.AlbedoColor = new Color(ParticleColor.R, ParticleColor.G, ParticleColor.B, 0.9f);
		material.Metallic = 0.1f;
		material.Roughness = 0.1f;
		material.EmissionEnabled = true;
		material.Emission = ParticleColor * 0.3f;
		material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		return material;
	}
	
	private void SetupBoundsVisualization()
	{
		boundsVisualization = new MeshInstance3D();
		boundsVisualization.Name = "BoundsVisualization";
		
		var cubeMesh = new BoxMesh();
		cubeMesh.Size = Bounds * 2;
		
		var material = new StandardMaterial3D();
		material.AlbedoColor = new Color(1, 1, 1, 0.2f);
		material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		
		cubeMesh.Material = material;
		boundsVisualization.Mesh = cubeMesh;
		boundsVisualization.Position = Vector3.Zero;
		
		AddChild(boundsVisualization);
	}
	
	private void InitializeParticles()
	{
		positionData = new float[particleCount * 4];
		velocityData = new float[particleCount * 4];
		
		// Fill the bounds with particles - start them at the top
		for (int i = 0; i < particleCount; i++)
		{
			// Position - spread throughout the box, but集中在顶部
			positionData[i * 4] = (float)(random.NextDouble() * (Bounds.X * 2) - Bounds.X);
			positionData[i * 4 + 1] = (float)(random.NextDouble() * Bounds.Y); // 0 to Bounds.Y (top half)
			positionData[i * 4 + 2] = (float)(random.NextDouble() * (Bounds.Z * 2) - Bounds.Z);
			positionData[i * 4 + 3] = 0;
			
			// Velocity - random initial movement
			velocityData[i * 4] = (float)(random.NextDouble() - 0.5f) * 2f;
			velocityData[i * 4 + 1] = (float)(random.NextDouble() - 0.5f) * 1f;
			velocityData[i * 4 + 2] = (float)(random.NextDouble() - 0.5f) * 2f;
			velocityData[i * 4 + 3] = 0;
		}
		
		// Set initial transforms
		for (int i = 0; i < particleCount; i++)
		{
			Vector3 pos = new Vector3(
				positionData[i * 4],
				positionData[i * 4 + 1],
				positionData[i * 4 + 2]
			);
			UpdateTransform(i, pos);
		}
		
		GD.Print("Particles initialized");
	}
	
	private void SetupComputeShader()
	{
		// This is a placeholder - compute shader setup would go here
		GD.Print("Compute shader mode selected (but not fully implemented)");
		useComputeShader = false; // Fall back to CPU
	}
	
	public override void _Process(double delta)
	{
		if (!RunSimulation) return;
		if (GameManager.time_stop) return;
		if (!initialized) return;
		
		float dt = (float)Math.Min(delta, 0.02f);
		
		timeAccumulator += dt;
		if (timeAccumulator > 1.0f)
		{
			if (positionData != null && positionData.Length > 0)
			{
				GD.Print($"Particle 0 Y: {positionData[1]:F2}, Vel Y: {velocityData[1]:F2}");
			}
			timeAccumulator = 0f;
		}
		
		// Use CPU simulation (guaranteed to work)
		UpdateParticlesCPU(dt);
		
		// Update visuals
		for (int i = 0; i < particleCount; i++)
		{
			Vector3 pos = new Vector3(
				positionData[i * 4],
				positionData[i * 4 + 1],
				positionData[i * 4 + 2]
			);
			UpdateTransform(i, pos);
		}
	}
	
	private void UpdateParticlesCPU(float dt)
	{
		// Water-like CPU simulation
		for (int i = 0; i < particleCount; i++)
		{
			int idx = i * 4;
			
			// Apply gravity
			velocityData[idx + 1] += Gravity * dt;
			
			// Add random noise for water-like movement
			velocityData[idx] += (float)(random.NextDouble() - 0.5f) * 2f * dt;
			velocityData[idx + 2] += (float)(random.NextDouble() - 0.5f) * 2f * dt;
			
			// Damping (air resistance)
			velocityData[idx] *= (1f - dt * 0.5f);
			velocityData[idx + 1] *= (1f - dt * 0.3f);
			velocityData[idx + 2] *= (1f - dt * 0.5f);
			
			// Update position
			float newX = positionData[idx] + velocityData[idx] * dt;
			float newY = positionData[idx + 1] + velocityData[idx + 1] * dt;
			float newZ = positionData[idx + 2] + velocityData[idx + 2] * dt;
			
			// Boundary collisions with water-like bounce
			float bounce = Damping;
			
			// X axis
			if (newX > Bounds.X)
			{
				newX = Bounds.X;
				velocityData[idx] *= -bounce;
				// Splash effect - transfer energy to other axes
				velocityData[idx + 1] += (float)random.NextDouble() * 1f;
				velocityData[idx + 2] += (float)(random.NextDouble() - 0.5f) * 1f;
			}
			else if (newX < -Bounds.X)
			{
				newX = -Bounds.X;
				velocityData[idx] *= -bounce;
				velocityData[idx + 1] += (float)random.NextDouble() * 1f;
				velocityData[idx + 2] += (float)(random.NextDouble() - 0.5f) * 1f;
			}
			
			// Y axis (floor and ceiling)
			if (newY > Bounds.Y)
			{
				newY = Bounds.Y;
				velocityData[idx + 1] *= -bounce * 0.7f;
			}
			else if (newY < -Bounds.Y)
			{
				newY = -Bounds.Y;
				velocityData[idx + 1] *= -bounce * 0.3f; // Less bounce on floor
				
				// Water spreads on floor
				velocityData[idx] *= 0.9f;
				velocityData[idx + 2] *= 0.9f;
				
				// Add random splash
				velocityData[idx] += (float)(random.NextDouble() - 0.5f) * 2f;
				velocityData[idx + 2] += (float)(random.NextDouble() - 0.5f) * 2f;
			}
			
			// Z axis
			if (newZ > Bounds.Z)
			{
				newZ = Bounds.Z;
				velocityData[idx + 2] *= -bounce;
				velocityData[idx] += (float)(random.NextDouble() - 0.5f) * 1f;
				velocityData[idx + 1] += (float)random.NextDouble() * 1f;
			}
			else if (newZ < -Bounds.Z)
			{
				newZ = -Bounds.Z;
				velocityData[idx + 2] *= -bounce;
				velocityData[idx] += (float)(random.NextDouble() - 0.5f) * 1f;
				velocityData[idx + 1] += (float)random.NextDouble() * 1f;
			}
			
			positionData[idx] = newX;
			positionData[idx + 1] = newY;
			positionData[idx + 2] = newZ;
		}
		
		// Simple cohesion - pull particles toward each other
		if (random.NextDouble() < 0.1) // 10% chance per frame
		{
			ApplySimpleCohesion();
		}
	}
	
	private void ApplySimpleCohesion()
	{
		// Pick a random particle and pull it toward neighbors
		int sampleIdx = random.Next(particleCount) * 4;
		Vector3 samplePos = new Vector3(
			positionData[sampleIdx],
			positionData[sampleIdx + 1],
			positionData[sampleIdx + 2]
		);
		
		Vector3 avgNeighborPos = Vector3.Zero;
		int neighborCount = 0;
		
		// Check a few random neighbors
		for (int n = 0; n < 20; n++)
		{
			int otherIdx = random.Next(particleCount) * 4;
			if (otherIdx == sampleIdx) continue;
			
			Vector3 otherPos = new Vector3(
				positionData[otherIdx],
				positionData[otherIdx + 1],
				positionData[otherIdx + 2]
			);
			
			float dist = (otherPos - samplePos).Length();
			if (dist < ParticleRadius * 2)
			{
				avgNeighborPos += otherPos;
				neighborCount++;
			}
		}
		
		if (neighborCount > 0)
		{
			avgNeighborPos /= neighborCount;
			Vector3 toCenter = avgNeighborPos - samplePos;
			velocityData[sampleIdx] += toCenter.X * 0.1f;
			velocityData[sampleIdx + 1] += toCenter.Y * 0.1f;
			velocityData[sampleIdx + 2] += toCenter.Z * 0.1f;
		}
	}
	
	private void UpdateTransform(int index, Vector3 position)
	{
		if (Multimesh == null) return;
		
		Transform3D t = Transform3D.Identity;
		t.Origin = position;
		t.Basis = Basis.Identity.Scaled(Vector3.One * particleSize);
		Multimesh.SetInstanceTransform(index, t);
	}
	
	public void ResetSimulation()
	{
		InitializeParticles();
	}
}
