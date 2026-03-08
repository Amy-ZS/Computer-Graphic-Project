using Godot;
using System;

public partial class MenuUI : CanvasLayer
{
<<<<<<< HEAD
	public override void _Ready()
	{
		GetNode<Button>("StartMenu/Panel/VBoxContainer/StartButton").Pressed += OnStartPressed;
		GetNode<Button>("StartMenu/Panel/VBoxContainer/QuitButton").Pressed += OnQuitPressed;

		GetNode<Button>("EndMenu/Panel/VBoxContainer/RestartButton").Pressed += OnRestartPressed;
		GetNode<Button>("EndMenu/Panel/VBoxContainer/QuitButton").Pressed += OnQuitPressed;
	}

	private void OnStartPressed()
	{
		GameManager.StartGame();
	}

	private void OnRestartPressed()
	{
		GameManager.RestartGame();
	}

	private void OnQuitPressed()
	{
		GameManager.QuitGame();
	}
=======
    public override void _Ready()
    {
        GetNode<Button>("StartMenu/Panel/VBoxContainer/StartButton").Pressed += OnStartPressed;
        GetNode<Button>("StartMenu/Panel/VBoxContainer/QuitButton").Pressed += OnQuitPressed;

        GetNode<Button>("EndMenu/Panel/VBoxContainer/RestartButton").Pressed += OnRestartPressed;
        GetNode<Button>("EndMenu/Panel/VBoxContainer/QuitButton").Pressed += OnQuitPressed;
    }

    private void OnStartPressed()
    {
        GameManager.StartGame();
    }

    private void OnRestartPressed()
    {
        GameManager.RestartGame();
    }

    private void OnQuitPressed()
    {
        GameManager.QuitGame();
    }
>>>>>>> 373465ce9f0711d142a6dbb246b5e5dab5554b57
}
