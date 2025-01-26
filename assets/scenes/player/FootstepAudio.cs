using Godot;
using Godot.Collections;
using System;

public partial class FootstepAudio : AudioStreamPlayer2DCustom
{
    [Export(PropertyHint.File)]
    Array<AudioStream> grassFootstepAudio;

    [Export(PropertyHint.Range, "-20, 20")]
    float grassFootstepDb;

    [Export(PropertyHint.File)]
    Array<AudioStream> gravelFootstepAudio;

    [Export(PropertyHint.Range, "-20, 20")]
    float gravelFootstepDb;

    [Export(PropertyHint.File)]
    Array<AudioStream> brickFootstepAudio;

    [Export(PropertyHint.Range, "-20, 20")]
    float brickFootstepDb;

    TileMapLayer staticTiles;

    public override void _Ready()
    {
        staticTiles = (TileMapLayer)GetTree().GetFirstNodeInGroup("static_tiles");
    }

    public void PlayFootstep()
    {
        Vector2I tileCoord = staticTiles.LocalToMap(staticTiles.ToLocal(GlobalPosition + (Vector2.Down * 6)));
        TileData tileData = staticTiles.GetCellTileData(tileCoord);

        string footstepType = "grass";

        if (tileData != null && tileData.HasCustomData("footstep"))
        {
            footstepType = (string)tileData.GetCustomData("footstep");
        }

        switch (footstepType)
        {
            case "grass":
                Stream = grassFootstepAudio[rng.Next(0, grassFootstepAudio.Count)];
                VolumeDb = grassFootstepDb;
                Play();
                break;
            case "gravel":
                Stream = gravelFootstepAudio[rng.Next(0, gravelFootstepAudio.Count)];
                VolumeDb = gravelFootstepDb;
                Play();
                break;
            case "brick":
                Stream = brickFootstepAudio[rng.Next(0, brickFootstepAudio.Count)];
                VolumeDb = brickFootstepDb;
                Play();
                break;
        }
    }
}
