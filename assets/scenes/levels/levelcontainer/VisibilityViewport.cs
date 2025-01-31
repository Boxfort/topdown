using Godot;
using System;

public partial class VisibilityViewport : SubViewport
{
    FollowTarget visibilityLight;
    DummyCamera visibilityCamera;
    Node2D mapContainer;
    Node2D occludersContainer;
    Node2D doorOccludersContainer;

    public FollowTarget VisibilityLight { get => visibilityLight; }
    public DummyCamera VisibilityCamera { get => visibilityCamera; }
    public Node2D MapContainer { get => mapContainer; }
    public Node2D OccludersContainer { get => occludersContainer; }
    public Node2D DoorOccludersContainer { get => doorOccludersContainer; }

    public override void _Ready()
    {
        visibilityLight = GetNode<FollowTarget>("VisibilityLight");
        visibilityCamera = GetNode<DummyCamera >("VisibilityCamera");
        mapContainer = GetNode<Node2D>("Map");
        occludersContainer = GetNode<Node2D>("Occluders");
        doorOccludersContainer = GetNode<Node2D>("DoorOccluders");
    }

    public void SetupViewport(PlayerController player, Camera2D mainCamera, Node2D staticTilesContainerCopy)
    {
        visibilityLight.target = player;
        visibilityCamera.mainCamera = mainCamera;
        MapContainer.AddChild(staticTilesContainerCopy);
    }
}
