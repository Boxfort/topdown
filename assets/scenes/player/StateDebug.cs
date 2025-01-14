using Godot;
using System;

public partial class StateDebug : Label
{
    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        await ToSignal(Owner, Node.SignalName.Ready);
        StateMachine stateMachine = (StateMachine)Owner.GetNode("StateMachine");
        Text = stateMachine.CurrentStateName;
        stateMachine.OnStateChanged += OnStateChanged;
    }

    private void OnStateChanged(string stateName) 
    {
        Text = stateName;
    }
}
