using Godot;
using System;
using System.Collections.Generic;

public partial class MenuList : VBoxContainer
{
    List<MenuEntry> menuEntries = [];

    MenuEntry currentHovering;

    public override void _Ready()
    {
        foreach (Node n in GetChildren())
        {
            if (n is MenuEntry m)
            {
                m.SetHovering(false);
                m.MouseEntered += () => OnMouseEnteredMenuEntry(m);
                m.Clicked += () => OnMenuEntryClicked(m);
                menuEntries.Add(m);
            }
        }
    }

    private void OnMenuEntryClicked(MenuEntry menuEntry)
    {
        if (!menuEntry.IsEnabled) return;

        switch (menuEntry.Identifier)
        {
            case "NEW_GAME":
                break;
            case "LEVEL_SELECT":
                break;
            case "EXIT":
                GetTree().Quit();
                break;
            default:
                GD.PushWarning("Unhandled menulist item: " + menuEntry.Identifier);
                break;
        }
    }

    private void OnMouseEnteredMenuEntry(MenuEntry menuEntry)
    {
        if (!menuEntry.IsEnabled) return;

        if (currentHovering != menuEntry)
        {
            currentHovering?.SetHovering(false);
            currentHovering = menuEntry;
            currentHovering.SetHovering(true);
        }
    }
}
