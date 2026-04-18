using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class GameManager : Node3D
{
	public enum GameState
	{
		StartMenu,
		Playing,
		EndMenu
	}

	public static GameManager Instance { get; private set; }

	public bool time_stop = true;
	public GameState CurrentState = GameState.StartMenu;

	[Export] public CanvasLayer uiLayer;

	public override void _Ready()
	{
		Instance = this;
		ShowStartMenu();
	}

	public void StartGame()
	{

		LineEdit label1 = GetTree().Root.GetNode<LineEdit>("Main/UI/StartMenu/VBoxContainer2/radius");

		if (!label1.Text.Equals("")) {
			if (int.TryParse(label1.Text, out int result)) {
				FluidSim.Instance.radius = result;
			}
		}

		LineEdit label2 = GetTree().Root.GetNode<LineEdit>("Main/UI/StartMenu/VBoxContainer2/density");

		if (!label2.Text.Equals("")) {
			if (int.TryParse(label2.Text, out int result)) {
				FluidSim.Instance.target_density = result;
			}
		}

		LineEdit label3 = GetTree().Root.GetNode<LineEdit>("Main/UI/StartMenu/VBoxContainer2/pressure");

		if (!label3.Text.Equals("")) {
			if (int.TryParse(label3.Text, out int result)) {
				FluidSim.Instance.pressure = result;
			}
		}

		LineEdit label4 = GetTree().Root.GetNode<LineEdit>("Main/UI/StartMenu/VBoxContainer2/velocity");

		if (!label4.Text.Equals("")) {
			if (int.TryParse(label4.Text, out int result)) {
				FluidSim.Instance.viscosity = result;
			}
		}

		LineEdit label5 = GetTree().Root.GetNode<LineEdit>("Main/UI/StartMenu/VBoxContainer2/count");

		if (!label5.Text.Equals("")) {
			if (int.TryParse(label5.Text, out int result)) {
				FluidSim.Instance.count = result;
			}
		}

		LineEdit label6 = GetTree().Root.GetNode<LineEdit>("Main/UI/StartMenu/VBoxContainer2/c_count");

		if (!label6.Text.Equals("")) {
			if (int.TryParse(label6.Text, out int result)) {
				FluidSim.Instance.surface_particle_count = result;
			}
		}

		LineEdit label7 = GetTree().Root.GetNode<LineEdit>("Main/UI/StartMenu/VBoxContainer2/c_step");

		if (!label7.Text.Equals("")) {
			if (int.TryParse(label7.Text, out int result)) {
				FluidSim.Instance.surface_particle_step = result;
			}
		}

		Input.MouseMode = Input.MouseModeEnum.Captured;
		Instance.SetMenuVisibility(false, false);

		FluidSim.Instance.START();
		CurrentState = GameState.Playing;
		time_stop = false;
	}

	public void ShowStartMenu()
	{
		CurrentState = GameState.StartMenu;
		time_stop = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		Instance.SetMenuVisibility(true, false);
	}

	public void ShowEndMenu()
	{
		CurrentState = GameState.EndMenu;
		time_stop = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		Instance.SetMenuVisibility(false, true);
	}

	public void RestartGame() { Instance.GetTree().ReloadCurrentScene(); }
	public void QuitGame() { Instance.GetTree().Quit(); }

	public void ToggleTime()
	{
		if (CurrentState != GameState.Playing) return;
		time_stop = !time_stop;
	}

	private void SetMenuVisibility(bool showStart, bool showEnd)
	{
		var startMenu = uiLayer.GetNode<Control>("StartMenu");
		var endMenu = uiLayer.GetNode<Control>("EndMenu");

		startMenu.Visible = showStart;
		endMenu.Visible = showEnd;
	}
}
