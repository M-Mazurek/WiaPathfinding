using Godot;

namespace WiaPathfinding;

public partial class MainScene : Node2D
{
    public static MainScene Singleton { get; private set; }
    
    [Export] public Camera2D MainCamera;

    public override void _Ready()
    {
        Singleton = this;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
            GetTree().Quit();
    }
}