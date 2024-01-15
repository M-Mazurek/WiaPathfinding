using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace WiaPathfinding;

public partial class FloydWarshallAlgorithm : GraphAlgorithm
{
    [Export] CanvasLayer _stepsLayer;
    [Export] TextureRect _graphPreview;
    [Export] ColorRect _bottomPanel;
    [Export] Label _matrixLabel;
    [Export] GridContainer _gridA, _gridB;

    int[,] _matrixA, _matrixB;

    public override void _Ready()
    {
        _stepsLayer.Hide();
        _bottomPanel.Modulate = _bottomPanel.Modulate with { A = 0f };
        _matrixLabel = _matrixLabel.Duplicate() as Label;
        base._Ready();
    }

    protected override async void Simulate()
    {
        SimulateNextStep -= Simulate;
        
        var scr = MainScene.Singleton.MainCamera.GetViewport().GetTexture();
        _graphPreview.Texture = ImageTexture.CreateFromImage(scr.GetImage());
        _stepsLayer.Show();
        MainScene.Singleton.MainCamera.Enabled = false;

        var tween = GetTree().CreateTween();
        tween.SetParallel(false);
        tween.TweenProperty(
            _graphPreview, 
            "scale", 
            _graphPreview.Scale * 0.5f,
            0.5f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(
            _bottomPanel, 
            "modulate:a", 
            1f,
            0.5f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

        CreateGrid(true);
        CreateGrid(false);

        for (int i = 0; i < Vertices.Length; i++)
        {
            FadeGridIn(_gridA);
            await ToSignal(this, GraphAlgorithm.SignalName.SimulateNextStep);
            CopyMatrix(true, i);
            _gridB.GetChild<Label>(0).Text = $"k={i+1}";
            PerformNextStep(i);

            if (i == Vertices.Length - 1)
                break;
            
            await ToSignal(this, GraphAlgorithm.SignalName.SimulateNextStep);
            _gridA.Modulate = new Color(1f, 1f, 1f, 0f);
            _gridB.Modulate = new Color(1f, 1f, 1f, 0f);
            await Task.Delay(400);
            CopyMatrix(false, i);
        }
    }

    void PerformNextStep(int i)
    {
        for (int y = 0; y < Vertices.Length; y++)
        {
            for (int x = 0; x < Vertices.Length; x++)
            {
                if (_matrixB[x, i] == int.MaxValue || _matrixB[i, y] == int.MaxValue)
                    continue;

                var sum = _matrixB[x, i] + _matrixB[i, y];
                if (_matrixB[x, y] > sum)
                {
                    _matrixB[x, y] = sum;
                    var label = _gridB.GetNode<Label>($"{x} {y}");
                    label.Text = sum.ToString();
                    HighlightLabelChanged(label);
                }
            }
        }
    }
    
    void CreateGrid(bool b)
    {
        GridContainer grid;

        grid = b ? _gridA : _gridB;
        
        var matrix = new int[Vertices.Length, Vertices.Length];
        grid.Columns = Vertices.Length + 1;

        for (int y = 0; y < grid.Columns; y++)
        {
            for (int x = 0; x < grid.Columns; x++)
            {
                var label = (_matrixLabel.Duplicate() as Label)!;
                label.LabelSettings = label.LabelSettings.Duplicate() as LabelSettings;

                var mY = y - 1;
                var mX = x - 1;

                if (y == 0)
                {
                    label.Text = x == 0 ? "k=0" : mX.ToString();
                    goto InvertColor;
                }

                if (x == 0)
                {
                    label.Text = mY.ToString();
                    goto InvertColor;
                }

                var weight = x == y
                    ? 0
                    : Edges.TryGetValue((mY, mX), out var edge)
                        ? edge.Weight
                        : int.MaxValue;

                matrix[mX, mY] = weight;
                label.Text = weight == int.MaxValue ? "∞" : weight.ToString();
                label.Name = $"{mX} {mY}";
                goto AddLabel;

                InvertColor:
                label.LabelSettings!.FontColor = Colors.Black;
                label.LabelSettings!.OutlineColor = Colors.White;
                AddLabel:
                grid.AddChild(label, true);
            }
        }

        if (b)
            _matrixA = matrix;
        else
            _matrixB = matrix;
    }

    void CopyMatrix(bool fromAToB, int highlightIndex)
    {
        GridContainer gridTo;
        int[,] matrixTo;

        if (fromAToB)
        {
            gridTo = _gridB;
            _matrixB = _matrixA;
            matrixTo = _matrixB;
        }
        else
        {
            gridTo = _gridA;
            _matrixA = _matrixB;
            matrixTo = _matrixA;
        }

        for (int y = 0; y < Vertices.Length; y++)
        {
            for (int x = 0; x < Vertices.Length; x++)
            {
                var label = gridTo.GetNode<Label>($"{x} {y}");
                label.LabelSettings!.FontColor = Colors.White;
                label.LabelSettings!.OutlineColor = Colors.Black;
                var weight = matrixTo[x, y];
                label.Text = weight == int.MaxValue ? "∞" : weight.ToString();
            }
        }
        
        for (int i = 0; i < Vertices.Length; i++)
        {
            HighlightLabelIteration(gridTo.GetNode<Label>($"{i} {highlightIndex}"));
            HighlightLabelIteration(gridTo.GetNode<Label>($"{highlightIndex} {i}"));
        }
        
        FadeGridIn(gridTo);
    }
    
    static void HighlightLabelIteration(Label label)
    {
        label.LabelSettings!.FontColor = Colors.Red;
        label.LabelSettings!.OutlineColor = Colors.DarkRed;
    }
    
    static void HighlightLabelChanged(Label label)
    {
        label.LabelSettings!.FontColor = Colors.Goldenrod;
        label.LabelSettings!.OutlineColor = Colors.DarkGoldenrod;
    }
    
    void FadeGridIn(GridContainer grid) => 
        GetTree().CreateTween().TweenProperty(
            grid, 
            "modulate:a", 
            1f,
            0.5f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out); 
}