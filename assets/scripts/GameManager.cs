using Godot;
using System;

public partial class GameManager : Node2D
{
    [Export(PropertyHint.NodeType)]
    Level currentLevel;

    FogOfWarViewport fogOfWarViewport;
    VisibilityViewport visibilityViewport;
    UnlitViewport unlitViewport;
    CombinedView combinedView;

    // TODO: HUD setup

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        fogOfWarViewport = GetNode<FogOfWarViewport>("FogOfWarViewport");
        visibilityViewport = GetNode<VisibilityViewport>("VisibilityViewport");
        unlitViewport = GetNode<UnlitViewport>("UnlitViewport");
        combinedView = GetNode<CombinedView>("CanvasLayer/CombinedView");
        SetupLevel();
    }

    private void SetupLevel()
    {
        TileMapLayer staticTiles = (TileMapLayer)currentLevel.staticTiles.Duplicate();
        TileMapLayer destructableTiles = (TileMapLayer)currentLevel.destructableTiles.Duplicate();

        unlitViewport.SetupViewport(currentLevel.mainCamera, staticTiles, destructableTiles);
        visibilityViewport.SetupViewport(currentLevel.player, currentLevel.mainCamera, (TileMapLayer)staticTiles.Duplicate());
        fogOfWarViewport.SetupFogOfWarViewport(visibilityViewport, currentLevel.mainCamera);

        combinedView.SetupCombinedView(currentLevel.mainCamera, fogOfWarViewport);
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
