using System;
using System.Linq;
using Godot;
using System.Collections.Generic;

namespace WiaPathfinding;

public abstract partial class GraphAlgorithm : Node2D
{
    [Signal] public delegate void SimulateNextStepEventHandler();
    
    [Export] Node2D _verticesRoot, _edgesRoot;

    protected bool HasFinished;
    protected GraphVertex[] Vertices;
    protected Dictionary<(int From, int To), GraphEdge> Edges = new();

    public override void _Ready()
    {
        InitializeGraph();
        SimulateNextStep += Simulate;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept"))
            EmitSignal(SignalName.SimulateNextStep);
    }

    protected abstract void Simulate();
    
    void InitializeGraph()
    {
        var vertices = _verticesRoot.GetChildren().Cast<GraphVertex>().ToArray();
        Vertices = vertices;
        
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].Index = i;
            GD.Print(i);
        }
        DrawEdges();
    }

    void DrawEdges()
    {
        foreach (var vertex in Vertices)
        {
            if (!vertex.ConnectedTo.Any())
                continue;

            foreach (var target in vertex.ConnectedTo)
            {
                GraphEdge edge = null;
                if (Edges.ContainsKey((target, vertex.Index)))
                {
                    edge = Edges[(target, vertex.Index)];
                    edge.ShowSecondArrow();
                }
                else
                {
                    edge = GraphEdge.CreateFor(this, vertex, Vertices[target]);
                    edge.Weight = new Random(DateTime.Now.Millisecond / (target + 1)).Next(1, 10);
                    _edgesRoot.AddChild(edge);
                }

                Edges.Add((vertex.Index, target), edge);
            }
        }
    }
}