using Godot;
using System;

public partial class FluidSim : Node
{
	private RayMarchController rayMarchController;
	private FluidMultiMesh fluidParticles;
	
	[Export] public bool EnableRayMarching = true;
	[Export] public bool EnableParticles = true;
	
	public override void _Ready()
	{
		// Find nodes
		fluidParticles = GetNodeOrNull<FluidMultiMesh>("FluidMultiMesh");
		rayMarchController = GetNodeOrNull<RayMarchController>("RayMarchController");
		
		if (rayMarchController == null && EnableRayMarching)
		{
			rayMarchController = new RayMarchController();
			rayMarchController.Name = "RayMarchController";
			AddChild(rayMarchController);
		}
	}
	
	public override void _Process(double delta)
	{
		if (fluidParticles != null)
		{
			fluidParticles.RunSimulation = EnableParticles && !GameManager.time_stop;
		}
	}
	
	public void ToggleRayMarching(bool enabled)
	{
		EnableRayMarching = enabled;
		if (rayMarchController != null)
		{
			// This should now work with the property we added
			rayMarchController.Visible = enabled;
		}
	}
}
