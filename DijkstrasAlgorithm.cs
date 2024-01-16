using Godot;
using System;
using System.Collections.Generic;

namespace WiaPathfinding;

public partial class DijkstrasAlgorithm : GraphAlgorithm
{
    [Export] int _target;
    PriorityQueue<int, int> _queue;
    int[] _distances, _prev;

    public override void _Ready()
    {
        base._Ready();
        _distances = new int[Vertices.Length];
        _prev = new int[Vertices.Length];
        Array.Fill(_distances, int.MaxValue);
        _distances[0] = 0;
        _queue = new PriorityQueue<int, int>(
            Vertices.Length, 
            Comparer<int>.Create((x, y) => x - y));
        
        for (int i = 1; i < Vertices.Length; i++)
        {
            _queue.Enqueue(i, 
                Edges.TryGetValue(
                    (0, i), 
                    out var val) ? val.Weight : int.MaxValue);
        }
        _queue.Enqueue(0, 0);
    }

    protected override async void Simulate()
    {
        SimulateNextStep -= Simulate;
        
        GraphEdge edge = null;
        while (_queue.Count > 0)
        {
            var index = _queue.Dequeue();
            var vertex = Vertices[index];
            
            foreach (var neighbour in vertex.ConnectedTo)
            {
                await ToSignal(this, GraphAlgorithm.SignalName.SimulateNextStep);
                vertex.SelfModulate = Colors.Gold;
                if (edge is not null)
                    edge.Modulate = Colors.White;
                int alt;
                try
                {
                    alt = checked(_distances[index] + Edges[(index, neighbour)].Weight);
                }
                catch
                {
                    alt = int.MaxValue;
                }

                edge = Edges[(index, neighbour)]!;

                if (alt >= _distances[neighbour])
                {
                    edge.Modulate = Colors.Red;
                    continue;
                }

                edge.Modulate = Colors.Gold;
                _distances[neighbour] = alt;
                Vertices[neighbour].Info = alt.ToString();
                _prev[neighbour] = index;
            }
            vertex.SelfModulate = Colors.Purple;
        }
        edge!.Modulate = Colors.White;
    }
}
