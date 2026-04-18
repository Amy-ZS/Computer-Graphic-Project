using Godot;
using System;

public partial class Camera : Camera3D
{
	[Export] public float speed = 10f;
	[Export] public float sens = 0.2f;

	private float x = 0f;
	private float y = 0f;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.Escape)
			{
				if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
					ToggleCursor();
			}
			else if (keyEvent.Keycode == Key.F)
			{
				GameManager.Instance.ToggleTime();
			}
			else if (keyEvent.Keycode == Key.K)
			{
				// 测试结束菜单
				GameManager.Instance.ShowEndMenu();
			}
		}

		if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
			return;

		if (@event is InputEventMouseMotion mouse && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			y -= mouse.Relative.X * sens;
			x -= mouse.Relative.Y * sens;
			x = Mathf.Clamp(x, -90f, 90f);

			RotationDegrees = new Vector3(x, y, 0);
		}
	}

	public override void _Process(double delta)
	{
		if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
			return;

		if (Input.MouseMode != Input.MouseModeEnum.Captured)
			return;

		Vector3 moveDirection = Vector3.Zero;

		if (Input.IsKeyPressed(Key.W)) moveDirection -= Transform.Basis.Z;
		if (Input.IsKeyPressed(Key.S)) moveDirection += Transform.Basis.Z;
		if (Input.IsKeyPressed(Key.A)) moveDirection -= Transform.Basis.X;
		if (Input.IsKeyPressed(Key.D)) moveDirection += Transform.Basis.X;
		if (Input.IsKeyPressed(Key.E)) moveDirection += Vector3.Up;
		if (Input.IsKeyPressed(Key.Q)) moveDirection += Vector3.Down;

		if (moveDirection.Length() > 0)
			Position += moveDirection.Normalized() * speed * (float)delta;
	}

	private void ToggleCursor()
	{
		if (Input.MouseMode == Input.MouseModeEnum.Captured)
			Input.MouseMode = Input.MouseModeEnum.Visible;
		else
			Input.MouseMode = Input.MouseModeEnum.Captured;
	}
}
