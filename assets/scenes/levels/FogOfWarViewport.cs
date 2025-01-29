using Godot;
using System;

public partial class FogOfWarViewport : SubViewport
{
    // TODO: get tilemap size x tilesize and set the viewport to that size.
    [Export]
    TileMapLayer staticTilemap;

    public override void _Ready()
    {
        int maxX = 0;
        int maxY = 0;

        foreach (Vector2I coord in staticTilemap.GetUsedCells())
        {
            maxX = Math.Max(coord.X, maxX);
            maxY = Math.Max(coord.Y, maxY);
        }

        Size = new Vector2I(maxX + 1, maxY + 1) * staticTilemap.TileSet.TileSize;
    }
}
