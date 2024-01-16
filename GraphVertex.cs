using System;
using Godot;

namespace WiaPathfinding;

public partial class GraphVertex : Sprite2D
{
    int __index;

    [Export] Label _infoLabel;
    [Export] Label _indexLabel;
    [Export] public int[] ConnectedTo = Array.Empty<int>();
    
    public int Index
    {
        get => __index;
        set
        {
            _indexLabel.Text = value.ToString();
            __index = value;
        }
    }

    public string Info
    {
        set => _infoLabel.Text = value;
    }
}