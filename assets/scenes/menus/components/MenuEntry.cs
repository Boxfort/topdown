using Godot;
using System;

public partial class MenuEntry : HBoxContainer
{
    [Signal]
    public delegate void ClickedEventHandler();

    Control leftArrow;
    Control rightArrow;
    Label entryLabel;

    [Export]
    bool isEnabled = true;

    [Export]
    string identifier = "";

    public bool IsEnabled { get => isEnabled; set => SetEnabled(value); }
    public string Identifier { get => identifier; }

    public override void _Ready()
    {
        leftArrow = GetNode<Control>("LeftArrow");
        rightArrow = GetNode<Control>("RightArrow");
        entryLabel = GetNode<Label>("Label");

        GuiInput += OnGuiInput;

        SetEnabled(isEnabled);
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
                //if (IsVisibleInTree()) 
                EmitSignal(SignalName.Clicked);
            }
        }
    }

    private void SetEnabled(bool value)
    {
        if (!IsEnabled)
        {
            entryLabel.ThemeTypeVariation = "LabelDisabled";
        }
        else
        {
            entryLabel.ThemeTypeVariation = "";
        }
    }


    public void SetHovering(bool isHovering)
    {
        if (isHovering)
        {
            leftArrow.Show();
            rightArrow.Show();
        }
        else
        {
            leftArrow.Hide();
            rightArrow.Hide();
        }
    }
}
