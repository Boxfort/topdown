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

    public override void _Input(InputEvent @event)
    {
        // TODO: WE NEED TO ADJUST THE MOUSE INPUT EVENT DUE TO THE RESIZING SHENANIGANS
        //       ONCE WE GET TO SOME UI JUNK, RE-WRITE THIS TO BE LESS SHITTY
        if (@event is InputEventMouse mouseEvent)
        {
            var viewport = GetNode<SubViewport>("UIViewport");
            var resizer = viewport.GetNode<UIResizeListener>("ResizeListener");

            mouseEvent.Position *= resizer.currentFactor;
            viewport.PushInput(@event);
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
