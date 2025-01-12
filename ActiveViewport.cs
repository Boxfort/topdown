using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ActiveViewport : SubViewport
{
    [Export(PropertyHint.NodeType, "TileMapLayer")]
    public TileMapLayer staticTiles;

    [Export(PropertyHint.NodeType, "TileMapLayer")]
    public TileMapLayer destructableTiles;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetProcessUnhandledInput(true);
    }

    public override void _Input(InputEvent @event)
    {
        foreach(Node child in GetChildren())
        {   
            if (@event is InputEventMouse mouseEvent)
            {
                InputEventMouse duplicateEvent = (InputEventMouse)mouseEvent.Duplicate();
                duplicateEvent.Position = GlobalCanvasTransform.AffineInverse() * duplicateEvent.Position;
                child._UnhandledInput(duplicateEvent);
            } else {
                child._UnhandledInput(@event);
            }
        }
    }
}
