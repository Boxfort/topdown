using Godot;
using System;
using System.Collections.Generic;

public enum PathType
{
    CIRCULAR,
    PING_PONG
}

[GlobalClass]
public partial class NpcPath : Node
{
    [Export]
    PathType pathType;

    int pathNodeCount = 0;
    int childCount = 0;

    public int PathNodeCount { get => pathNodeCount; }

    public override void _Ready()
    {
        childCount = GetChildCount();
        pathNodeCount = pathType == PathType.CIRCULAR ? childCount : (childCount * 2 ) - 1;
    }

    public PathNode GetNodeAtIdx(int idx)
    {
        if (pathType == PathType.CIRCULAR) {
            return (PathNode)GetChild(idx);
        } else {
            if (idx >= childCount) {
                // count backwards
                var a = childCount - ((idx % childCount) + 1);
                return (PathNode)GetChild(a);
            } else {
                return (PathNode)GetChild(idx);
            }
        }
    }
}
