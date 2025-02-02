using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenuList : MenuList
{
    override protected void OnMenuEntryClicked(MenuEntry menuEntry)
    {
        switch (menuEntry.Identifier)
        {
            case "NEW_GAME":
                break;
            case "LEVEL_SELECT":
                EmitSignal(MenuList.SignalName.OnMenuEntrySelected, menuEntry.Identifier);
                break;
            case "EXIT":
                GetTree().Quit();
                break;
            default:
                GD.PushWarning("Unhandled menulist item: " + menuEntry.Identifier);
                break;
        }
    }
}