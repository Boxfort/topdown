using Godot;
using System;

public partial class Level : Node2D
{
    [Export(PropertyHint.NodeType, "TileMapLayer")]
    public TileMapLayer staticTiles;

    [Export(PropertyHint.NodeType, "TileMapLayer")]
    public TileMapLayer destructableTiles;

    [Export(PropertyHint.NodeType, "Camera2D")]
    public Camera2D mainCamera;

    [Export(PropertyHint.NodeType, "Node2D")]
    public PlayerController player;

    [Export(PropertyHint.NodeType, "Node2D")]
    public TilemapDestructionHandler tilemapDestructionHandler;

    public void SetupLevel(Node2D visionOccluderContainer)
    {
        tilemapDestructionHandler.MainOccludersContainer = visionOccluderContainer;
    }
}
