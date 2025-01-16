using System;
using System.Collections;
using Godot;
using Godot.Collections;

public partial class StateMachine : Node
{
    [Signal]
    public delegate void OnStateChangedEventHandler(string stateName);

    [Export(PropertyHint.NodeType, "Node")]
    State initialState = null;
    State currentState = null;

    public string CurrentStateName { get => currentState.Name; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        currentState = initialState ?? (State)GetChild(0);

        foreach (State state in GetChildren())
        {
            state.Finished += OnStateFinished;
        }
    }

    private void OnStateFinished(string nextState, Godot.Collections.Dictionary data)
    {
        if (!HasNode(nextState))
        {
            GD.PushError(Owner.Name + ": trying to transition to state '" + nextState + "' but it does not exist.");
            return;
        }

        string previousState = currentState.Name;
        currentState.PreExit();

        currentState = (State)GetNode(nextState);
        currentState.PreEnter(previousState, data);

        EmitSignal(SignalName.OnStateChanged, currentState.Name);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        currentState.PreProcess(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        currentState.PrePhysicsProcess(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        currentState.PreUnhandledInput(@event);
    }
}
