using Godot;
using System;

public partial class Level : Node2D
{
    [Export(PropertyHint.NodeType, "Node2D")]
    public Node2D staticTilesContainer;

    [Export]
    public TileMapLayer mainStaticTiles;

    [Export(PropertyHint.NodeType, "TileMapLayer")]
    public TileMapLayer destructableTiles;

    [Export(PropertyHint.NodeType, "Camera2D")]
    public CameraController mainCamera;

    [Export(PropertyHint.NodeType, "Node2D")]
    public PlayerController player;

    [Export(PropertyHint.NodeType, "Node2D")]
    public TilemapDestructionHandler tilemapDestructionHandler;

    public void SetupLevel(Node2D visionOccluderContainer, Node2D doorVisionOccludersContainer, CombinedView combinedView)
    {
        tilemapDestructionHandler.SetupTilemapDestructionHandler(visionOccluderContainer);
        mainCamera.CombinedView = combinedView;

        foreach (DoorScript door in GetTree().GetNodesInGroup("door"))
        {
            door.SetupDoor(doorVisionOccludersContainer);
        }
    }
}
