using Godot;
using System;

public partial class DoorScript : Node2D
{
    [Export]
    Node2D doorVisionOccluderContainer;

    RigidBody2D door;
    PinJoint2D pinJoint;
    StaticBody2D hinge;
    Vector2 doorPosition;
    LightOccluder2D occluder;
    LightOccluder2D occluderCopy;

    public override void _Ready()
    {
        door = GetNode<RigidBody2D>("RigidbodyDoor");
        pinJoint = GetNode<PinJoint2D>("PinJoint2D");
        hinge = GetNode<StaticBody2D>("StaticHinge");
        occluder = door.GetNode<LightOccluder2D>("LightOccluder2D");

        occluderCopy = (LightOccluder2D)occluder.Duplicate();
        doorVisionOccluderContainer.AddChild(occluderCopy);
        occluder.TreeExiting += () => occluderCopy.Free();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Keep the visionOccluder in line with this occluder
        occluderCopy.GlobalTransform = occluder.GlobalTransform;

        // Stop the door from over rotating
        if (door.Rotation > pinJoint.AngularLimitUpper) door.Rotation = pinJoint.AngularLimitUpper;
        if (door.Rotation < pinJoint.AngularLimitLower) door.Rotation = pinJoint.AngularLimitLower;

        // Stop the door from flying away from its hinge
        var offsetFromHinge = hinge.Position.Rotated(door.Rotation) - hinge.Position + door.Position;
        door.Position -= offsetFromHinge;
    }
}
