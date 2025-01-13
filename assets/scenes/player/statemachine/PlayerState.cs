using Godot;
using System;

public abstract partial class PlayerState : State
{
    public enum PlayerStates 
    {
        Idle,
        Running,
        Diving
    }

    protected PlayerController player;

    public override async void _Ready()
    {
        await ToSignal(Owner, Node.SignalName.Ready);
        if (Owner is PlayerController playerController) {
            player = playerController;
        } else {
            GD.PushError("PlayerState must only be used in the PlayerController scene. It requires that the Owner is a PlayerController node.");
        }
    }
}
