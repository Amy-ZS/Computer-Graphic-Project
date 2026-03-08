using Godot;
using System;

public partial class RayMarchController : Node
{
	private Camera3D camera;
	private ShaderMaterial rayMarchMaterial;
	private MeshInstance3D quad;
	
	// Remove 'new' keyword - just use a normal property
	public bool Visible 
	{ 
		get => quad != null && quad.Visible;
		set { if (quad != null) quad.Visible = value; }
	}
	
	[Export] public int MaxSteps = 64;
	[Export] public float MaxDistance = 100f;
	[Export] public float WaterDensity = 0.5f;
	[Export] public Color WaterColor = new Color(0.2f, 0.5f, 0.8f);
	[Export] public Color DeepWaterColor = new Color(0.0f, 0.1f, 0.3f);
	
	public override void _Ready()
	{
		SetupFullscreenQuad();
		// Get camera safely
		camera = GetViewport()?.GetCamera3D();
	}
	
	private void SetupFullscreenQuad()
	{
		// Create a fullscreen quad
		quad = new MeshInstance3D();
		quad.Name = "RayMarchQuad";
		
		var mesh = new QuadMesh();
		mesh.Size = new Vector2(2.0f, 2.0f);
		quad.Mesh = mesh;
		
		// Try to load shader
		try
		{
			rayMarchMaterial = new ShaderMaterial();
			var shader = GD.Load<Shader>("res://shaders/ray_march.gdshader");
			if (shader != null)
			{
				rayMarchMaterial.Shader = shader;
			}
			else
			{
				// Create a simple fallback material if shader doesn't exist
				var fallbackMaterial = new StandardMaterial3D();
				fallbackMaterial.AlbedoColor = new Color(0.2f, 0.5f, 0.8f, 0.5f);
				fallbackMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
				quad.MaterialOverride = fallbackMaterial;
				GD.Print("Ray march shader not found, using fallback material");
			}
		}
		catch (Exception e)
		{
			GD.PrintErr("Error setting up ray march material: " + e.Message);
			// Create a simple material as fallback
			var fallbackMaterial = new StandardMaterial3D();
			fallbackMaterial.AlbedoColor = new Color(0.2f, 0.5f, 0.8f, 0.5f);
			fallbackMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
			quad.MaterialOverride = fallbackMaterial;
		}
		
		if (rayMarchMaterial != null)
		{
			quad.MaterialOverride = rayMarchMaterial;
		}
		
		AddChild(quad);
		
		// Position in front of camera
		quad.Position = new Vector3(0, 0, -1);
	}
	
	public override void _Process(double delta)
	{
		if (camera == null)
		{
			camera = GetViewport()?.GetCamera3D();
			return;
		}
		
		if (rayMarchMaterial == null) return;
		
		try
		{
			// Update shader uniforms
			var resolution = GetViewport().GetVisibleRect().Size;
			rayMarchMaterial.SetShaderParameter("resolution", resolution);
			rayMarchMaterial.SetShaderParameter("time", Time.GetTicksMsec() / 1000f);
			rayMarchMaterial.SetShaderParameter("cam_pos", camera.GlobalPosition);
			
			// Calculate camera inverse matrix
			var camTransform = camera.GlobalTransform;
			var camBasis = camTransform.Basis;
			var camInv = camBasis.Orthonormalized().Transposed();
			rayMarchMaterial.SetShaderParameter("cam_inv", camInv);
			
			// Update parameters
			rayMarchMaterial.SetShaderParameter("max_steps", MaxSteps);
			rayMarchMaterial.SetShaderParameter("max_distance", MaxDistance);
			rayMarchMaterial.SetShaderParameter("water_density", WaterDensity);
			rayMarchMaterial.SetShaderParameter("water_color", WaterColor);
			rayMarchMaterial.SetShaderParameter("deep_color", DeepWaterColor);
		}
		catch (Exception e)
		{
			GD.PrintErr("Error updating ray march parameters: " + e.Message);
		}
		
		// Keep quad in front of camera
		if (quad != null)
		{
			quad.GlobalPosition = camera.GlobalPosition;
			quad.GlobalRotation = camera.GlobalRotation;
		}
	}
}
