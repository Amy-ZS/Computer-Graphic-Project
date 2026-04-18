using Godot;
using System;

public partial class MenuUI : CanvasLayer
{
	public override void _Ready()
	{
		GetNode<Button>("StartMenu/VBoxContainer/StartButton").Pressed += GameManager.Instance.StartGame;
		GetNode<Button>("StartMenu/VBoxContainer/QuitButton").Pressed += GameManager.Instance.QuitGame;

		GetNode<Button>("EndMenu/VBoxContainer/RestartButton").Pressed += GameManager.Instance.RestartGame;
		GetNode<Button>("EndMenu/VBoxContainer/QuitButton").Pressed += GameManager.Instance.QuitGame;
	}
}
