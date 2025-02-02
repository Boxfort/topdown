using Godot;
using System;

public partial class GameManager : Node2D
{
    [Export(PropertyHint.NodeType)]
    Level currentLevel;

    FogOfWarViewport fogOfWarViewport;
    VisibilityViewport visibilityViewport;
    UnlitViewport unlitViewport;
    UiViewport uiViewport;
    CombinedView combinedView;
    Node2D levelContainer;

    // TODO: HUD setup

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        fogOfWarViewport = GetNode<FogOfWarViewport>("FogOfWarViewport");
        visibilityViewport = GetNode<VisibilityViewport>("VisibilityViewport");
        unlitViewport = GetNode<UnlitViewport>("UnlitViewport");
        uiViewport = GetNode<UiViewport>("UIViewport");
        combinedView = GetNode<CombinedView>("CanvasLayer/CombinedView");
        levelContainer = GetNode<Node2D>("SubViewportContainer/ActiveViewport/LevelContainer");

        LoadLevel();
    }

    public void LoadLevel()
    {
        // TODO: I kind of hate this scene switching style, can we do something different?
        string levelName = SceneSwitcher.Instance.GetParameter("level_path").ToString();
        PackedScene levelScene = (PackedScene)ResourceLoader.Load(levelName);
        Level level = (Level)levelScene.Instantiate();
        levelContainer.AddChild(level);
        currentLevel = level;
        SetupLevel();
    }

    private void SetupLevel()
    {
        Node2D staticTiles = (Node2D)currentLevel.staticTilesContainer.Duplicate();
        TileMapLayer destructableTiles = (TileMapLayer)currentLevel.destructableTiles.Duplicate();

        currentLevel.SetupLevel(visibilityViewport.OccludersContainer, visibilityViewport.DoorOccludersContainer);

        unlitViewport.SetupViewport(currentLevel.mainCamera, staticTiles, destructableTiles);
        visibilityViewport.SetupViewport(currentLevel.player, currentLevel.mainCamera, (Node2D)staticTiles.Duplicate());
        fogOfWarViewport.SetupFogOfWarViewport(visibilityViewport, currentLevel.mainCamera);
        uiViewport.SetupUIViewport(currentLevel.player);

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
