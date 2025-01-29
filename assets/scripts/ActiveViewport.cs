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
        //SetProcessUnhandledInput(true);
    }
}
