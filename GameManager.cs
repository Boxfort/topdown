using Godot;
using System;

public partial class GameManager : Node2D
{
    [Export(PropertyHint.NodeType, "SubViewport")]
    ActiveViewport activeViewport;

    [Export(PropertyHint.NodeType, "Node2D")]
    Node2D unlitMapContainer;

    [Export(PropertyHint.NodeType, "Node2D")]
    Node2D visibilityMapContainer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var staticTiles = activeViewport.staticTiles.Duplicate();
        var destructableTiles = activeViewport.destructableTiles.Duplicate();
        unlitMapContainer.AddChild(staticTiles);
        visibilityMapContainer.AddChild(staticTiles.Duplicate());
        unlitMapContainer.AddChild(destructableTiles);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
