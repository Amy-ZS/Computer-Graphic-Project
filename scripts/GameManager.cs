using Godot;
using System;

public partial class GameManager : Node3D
{
	public static bool time_stop = true;
	public override void _Ready() {
		base._Ready();
	}

	public static void ToggleTime() {
		time_stop = !time_stop;
	}
}
