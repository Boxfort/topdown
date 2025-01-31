using Godot;
using Godot.Collections;
using System;

public partial class FootstepAudio : AudioStreamPlayer2DCustom
{
    [Export(PropertyHint.File)]
    Array<AudioStream> grassFootstepAudio;

    [Export(PropertyHint.Range, "-20, 20")]
    float grassFootstepDb;

    float grassFootstepNoiseValue = 10;

    [Export(PropertyHint.File)]
    Array<AudioStream> gravelFootstepAudio;

    [Export(PropertyHint.Range, "-20, 20")]
    float gravelFootstepDb;

    float gravelFootstepNoiseValue = 30;

    [Export(PropertyHint.File)]
    Array<AudioStream> brickFootstepAudio;

    [Export(PropertyHint.Range, "-20, 20")]
    float brickFootstepDb;

    float brickFootstepNoiseValue = 30;

    TileMapLayer floorTiles;

    public override void _Ready()
    {
        floorTiles = (TileMapLayer)GetTree().GetFirstNodeInGroup("floor_tiles");
    }

    public float PlayFootstep(float noiseFactor = 1)
    {
        Vector2I tileCoord = floorTiles.LocalToMap(floorTiles.ToLocal(GlobalPosition + (Vector2.Down * 6)));
        TileData tileData = floorTiles.GetCellTileData(tileCoord);

        string footstepType = "grass";

        if (tileData != null && tileData.HasCustomData("footstep"))
        {
            footstepType = (string)tileData.GetCustomData("footstep");
        }

        switch (footstepType)
        {
            case "grass":
                Stream = grassFootstepAudio[rng.Next(0, grassFootstepAudio.Count)];
                // TODO: make sounds quiet based on noise factor
                VolumeDb = grassFootstepDb;
                Play();
                return grassFootstepNoiseValue * noiseFactor;
            case "gravel":
                Stream = gravelFootstepAudio[rng.Next(0, gravelFootstepAudio.Count)];
                VolumeDb = gravelFootstepDb;
                Play();
                return gravelFootstepNoiseValue * noiseFactor;
            case "brick":
                Stream = brickFootstepAudio[rng.Next(0, brickFootstepAudio.Count)];
                VolumeDb = brickFootstepDb;
                Play();
                return brickFootstepNoiseValue * noiseFactor;
            default:
                return 0;
        }
    }
}
