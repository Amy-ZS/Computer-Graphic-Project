using Godot;
using System;

public partial class MenuUI : CanvasLayer
{
	public override void _Ready()
	{
		GetNode<Button>("StartMenu/VBoxContainer/StartButton").Pressed += GameManager.StartGame;
		GetNode<Button>("StartMenu/VBoxContainer/QuitButton").Pressed += GameManager.QuitGame;

		GetNode<Button>("EndMenu/VBoxContainer/RestartButton").Pressed += GameManager.RestartGame;
		GetNode<Button>("EndMenu/VBoxContainer/QuitButton").Pressed += GameManager.QuitGame;
	}
}
