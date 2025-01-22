using Godot;
using System;

public partial class DoorCollisionArea : Area2D
{
    [Export]
    RigidBody2D parentRigidBody;

    public RigidBody2D ParentRigidBody { get => parentRigidBody; }
}
