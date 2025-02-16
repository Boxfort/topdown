using Godot;
using System;
using System.Threading.Tasks;

public partial class FogOfWarViewport : SubViewport
{
    FogOfWarTexture fogOfWarMap;

    public FogOfWarTexture FogOfWarMap { get => fogOfWarMap; }

    public override void _Ready()
    {
        fogOfWarMap = GetNode<FogOfWarTexture>("FogOfWarMap");
    }

    public void SetupFogOfWarViewport(VisibilityViewport visibilityViewport, TileMapLayer staticTiles, Camera2D mainCamera)
    {
        fogOfWarMap.VisibilityViewport = visibilityViewport;
        fogOfWarMap.MainCamera = mainCamera;
        SetFogOfWarMapSize(staticTiles);
        _ = ResetFogOfWar();
    }

    public async Task ResetFogOfWar()
    {
        RenderTargetClearMode = ClearMode.Always;

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        RenderTargetClearMode = ClearMode.Once;
    }

    public void SetFogOfWarMapSize(TileMapLayer tileMap)
    {
        int maxX = 0;
        int maxY = 0;

        foreach (Vector2I coord in tileMap.GetUsedCells())
        {
            maxX = Math.Max(coord.X, maxX);
            maxY = Math.Max(coord.Y, maxY);
        }

        Size = new Vector2I(maxX + 1, maxY + 1) * tileMap.TileSet.TileSize;
    }
}
