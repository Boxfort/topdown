using Godot;
using System;

public abstract partial class GuardState : State
{
    public enum GuardStates 
    {
        Following
    }

    protected GuardController guard;
    protected CombinedView combinedView;


    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        GD.Print("getting ready");
        await ToSignal(Owner, Node.SignalName.Ready);
        if (Owner is GuardController guardController) {
            guard = guardController;
            GD.Print("set guard controller");
        } else {
            GD.PushError("PlayerState must only be used in the PlayerController scene. It requires that the Owner is a PlayerController node.");
        }

        combinedView = (CombinedView)GetTree().GetFirstNodeInGroup("combined_view");
    }
}
