using Godot;
using System;

public partial class GameManager : Node3D
{
	public static bool time_stop = false; // Start with time running
	
	public override void _Ready() 
	{
		base._Ready();
		GD.Print("GameManager ready. Time running: " + !time_stop);
	}

	public static void ToggleTime() 
	{
		time_stop = !time_stop;
		GD.Print("Time stopped: " + time_stop);
	}
}
