using Godot;
using System;

public partial class LevelList : MenuList
{
    protected override void OnMenuEntryClicked(MenuEntry menuEntry)
    {
         switch (menuEntry.Identifier)
        {
            case "TEST_LEVEL":
                EmitSignal(MenuList.SignalName.OnMenuEntrySelected, menuEntry.Identifier);
                break;
            case "TUTORIAL":
                EmitSignal(MenuList.SignalName.OnMenuEntrySelected, menuEntry.Identifier);
                break;
            case "BACK":
                EmitSignal(MenuList.SignalName.OnMenuEntrySelected, menuEntry.Identifier);
                break;
            default:
                GD.PushWarning("Unhandled menulist item: " + menuEntry.Identifier);
                break;
        }
    }
}
