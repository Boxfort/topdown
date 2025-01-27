using Godot;
using System;

public abstract partial class GuardState : State
{
    public enum GuardStates 
    {
        Idle,
        Investigating,
        Chase,
        MoveTowards,
        Patrol,
        Attacking,
        KnockedOut,
        Dead,
        Alert
    }

    protected PlayerController player;
    protected GuardController guard;
    protected CombinedView combinedView;
    protected Node2D weaponContainer;


    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        await ToSignal(Owner, Node.SignalName.Ready);
        if (Owner is GuardController guardController) {
            guard = guardController;
        } else {
            GD.PushError("GuardState must only be used in the GuardController scene. It requires that the Owner is a GuardController node.");
        }

        combinedView = (CombinedView)GetTree().GetFirstNodeInGroup("combined_view");
        player = (PlayerController)GetTree().GetFirstNodeInGroup("player");
        weaponContainer = (Node2D)guard.GetNode("WeaponContainer");
    }
}
