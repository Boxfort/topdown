using Godot;
using System;
using System.Collections.Generic;

public abstract partial class State : Node
{
    [Signal]
    public delegate void FinishedEventHandler(string nextState, Godot.Collections.Dictionary data);

    public static readonly Godot.Collections.Dictionary NO_DATA = new();

    public bool hasExited = false;

    public void PreEnter(string previousState, Godot.Collections.Dictionary data)
    {
        hasExited = false;
        Enter(previousState, data);
    }

    public void PreExit()
    {
        hasExited = true;
        Exit();
    }

    public void PreUnhandledInput(InputEvent @event)
    {
        if (hasExited) return;

        UnhandledInput(@event);
    }

    public void PreProcess(double delta)
    {
        if (hasExited) return;

        Process(delta);
    }

    public async void PrePhysicsProcess(double delta)
    {
        if (hasExited) return;

        PhysicsProcess(delta);
    }

    public abstract void Enter(string previousState, Godot.Collections.Dictionary data);
    public abstract void Exit();
    public abstract void UnhandledInput(InputEvent @event);
    public abstract void Process(double delta);
    public abstract void PhysicsProcess(double delta);
}
