using Godot;
using System;

public partial class GameManager : Node3D
{
    public enum GameState
    {
        StartMenu,
        Playing,
        EndMenu
    }

    public static GameManager Instance { get; private set; }

    public static bool time_stop = true;
    public static GameState CurrentState = GameState.StartMenu;

    [Export] public CanvasLayer uiLayer;
    [Export] public float autoEndTime = 30.0f;

    private float elapsed = 0.0f;

    public override void _Ready()
    {
        Instance = this;
        ShowStartMenu();
    }

    public override void _Process(double delta)
    {
        if (CurrentState == GameState.Playing)
        {
            elapsed += (float)delta;

            if (elapsed >= autoEndTime)
            {
                ShowEndMenu();
            }
        }
    }

    public static void StartGame()
    {
        if (Instance == null) return;

        CurrentState = GameState.Playing;
        time_stop = false;
        Instance.elapsed = 0.0f;

        Input.MouseMode = Input.MouseModeEnum.Captured;
        Instance.SetMenuVisibility(false, false);
    }

    public static void ShowStartMenu()
    {
        if (Instance == null) return;

        CurrentState = GameState.StartMenu;
        time_stop = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;

        Instance.SetMenuVisibility(true, false);
    }

    public static void ShowEndMenu()
    {
        if (Instance == null) return;

        CurrentState = GameState.EndMenu;
        time_stop = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;

        Instance.SetMenuVisibility(false, true);
    }

    public static void RestartGame()
    {
        if (Instance == null) return;
        Instance.GetTree().ReloadCurrentScene();
    }

    public static void QuitGame()
    {
        if (Instance == null) return;
        Instance.GetTree().Quit();
    }

    public static void ToggleTime()
    {
        if (CurrentState != GameState.Playing) return;
        time_stop = !time_stop;
    }

    private void SetMenuVisibility(bool showStart, bool showEnd)
    {
        if (uiLayer == null) return;

        var startMenu = uiLayer.GetNode<Control>("StartMenu");
        var endMenu = uiLayer.GetNode<Control>("EndMenu");

        startMenu.Visible = showStart;
        endMenu.Visible = showEnd;
    }
}
