using Godot;
using System;

public partial class UnlitViewport : SubViewport
{
    DummyCamera unlitCamera;
    Node2D mapContainer;

    public DummyCamera UnlitCamera { get => unlitCamera; }
    public Node2D MapContainer { get => mapContainer; set => mapContainer = value; }

    public override void _Ready()
    {
        unlitCamera = GetNode<DummyCamera>("UnlitCamera");
        mapContainer = GetNode<Node2D>("Map");
    }

    public void SetupViewport(Camera2D mainCamera, TileMapLayer staticTilesCopy, TileMapLayer destructibleTilesCopy)
    {
        UnlitCamera.mainCamera = mainCamera;
        MapContainer.AddChild(staticTilesCopy);
        MapContainer.AddChild(destructibleTilesCopy);
    }
}
