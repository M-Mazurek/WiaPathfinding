using Godot;

namespace WiaPathfinding;

public partial class GraphEdge : Line2D
{
    const string SCENE_PATH = "res://GraphEdge.tscn";
    const float ORBIT_RADIUS = 14f;

    int __weight;
    
    [Export] Sprite2D _arrowA, _arrowB;
    [Export] Label _weightLabel;

    public int Weight
    {
        get => __weight;
        set
        {
            _weightLabel.Text = value.ToString();
            __weight = value;
        }
    }

    public void ShowSecondArrow()
    {
        _arrowB.Texture = _arrowA.Texture.Duplicate() as Texture2D;
    }
    
    public static GraphEdge CreateFor(GraphAlgorithm graph, GraphVertex from, GraphVertex to)
    {
        var edge = ResourceLoader.Load<PackedScene>(SCENE_PATH).Instantiate<GraphEdge>();
        
        var posA = from.GlobalPosition.MoveToward(to.GlobalPosition, ORBIT_RADIUS);
        var posB = to.GlobalPosition.MoveToward(from.GlobalPosition, ORBIT_RADIUS);
        edge._arrowA.GlobalPosition = posB;
        edge._arrowB.GlobalPosition = posA;
        edge._arrowA.LookAt(from.GlobalPosition);
        edge._arrowB.LookAt(to.GlobalPosition);

        var midVec = (posA + posB) / 2f;
        edge._weightLabel.GlobalPosition = midVec + new Vector2(-2f, -4f);
        
        edge.Points = new[] { posA, posB };
        return edge;
    }
}