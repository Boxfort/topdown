using Godot;
using System;
using System.Collections.Generic;

public abstract partial class MenuList : VBoxContainer
{
    [Signal]
    public delegate void OnMenuEntrySelectedEventHandler(string identifier);

    protected List<MenuEntry> menuEntries = [];

    protected MenuEntry currentHovering;

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

    private void OnMenuEntryClickedBefore(MenuEntry menuEntry)
    {
        if (!menuEntry.IsEnabled) return;

        OnMenuEntryClicked(menuEntry);
    }

    protected abstract void OnMenuEntryClicked(MenuEntry menuEntry);

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
