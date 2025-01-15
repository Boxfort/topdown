using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class TilemapDestructionHandler : Node2D
{
    [Export(PropertyHint.NodeType, "TileMapLayer")]
    TileMapLayer tileMap;

    [Export(PropertyHint.NodeType, "Polygon2D")]
    Polygon2D polygonMask;

    [Export(PropertyHint.NodeType, "StaticBody2D")]
    StaticBody2D collisionContainer;

    [Export(PropertyHint.NodeType, "StaticBody2D")]
    StaticBody2D visionCollisionContainer;

    [Export(PropertyHint.NodeType, "Node2D")]
    Node2D mainOccludersContainer;

    [Export(PropertyHint.NodeType, "Node2D")]
    Node2D copyOccludersContainer;

    [Export(PropertyHint.NodeType, "NavigationRegion2D")]
    NavigationRegion2D navigationRegion;

    const float wallsLayer = 2.0f;
    Vector2I cellSize = new(8, 8);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        cellSize = tileMap.TileSet.TileSize;
        if (tileMap != null)
        {
            // Make the tileset data unique so we can fuck with it.
            var tileset = tileMap.TileSet;
            tileMap.TileSet = (TileSet)tileset.Duplicate();
            tileMap.CollisionEnabled = false;
            CombineOccluders(tileMap);
            ConstructColliders(tileMap);
        }
    }

    public void Carve(CollisionPolygon2D clippingPolygon)
    {
        CarveColliders(clippingPolygon);
        CarveOccluders(clippingPolygon);

        // TODO: Do this on a timer, its very slow.
        navigationRegion.BakeNavigationPolygon();
    }

    private void CarveOccluders(CollisionPolygon2D clippingPolygon)
    {
        foreach (Node child in mainOccludersContainer.GetChildren())
        {
            if (child is LightOccluder2D lightOccluder2D)
            {
                CarveNode<LightOccluder2D>(
                    clippingPolygon,
                    mainOccludersContainer,
                    lightOccluder2D,
                    (node) => node.Occluder.Polygon,
                    (node, polygon) => node.Occluder.Polygon = polygon,
                    (polygon) =>
                    {
                        LightOccluder2D newLightOccluder2D = new()
                        {
                            Occluder = new()
                        };
                        newLightOccluder2D.Occluder.Polygon = polygon;
                        return newLightOccluder2D;
                    },
                    true
                );
            }
        }

        // Clear occluder copies, and duplicate over the newly sliced occluders.
        foreach (Node child in copyOccludersContainer.GetChildren())
        {
            child.Free();
        }

        foreach (Node child in visionCollisionContainer.GetChildren())
        {
            child.Free();
        }

        foreach (LightOccluder2D child in mainOccludersContainer.GetChildren())
        {
            copyOccludersContainer.AddChild(child.Duplicate());
            CollisionPolygon2D collisionPolygon2D = new()
            {
                Polygon = child.Occluder.Polygon
            };
            visionCollisionContainer.AddChild(collisionPolygon2D);
        }
    }

    private void CarveColliders(CollisionPolygon2D clippingPolygon)
    {
        foreach (Node child in collisionContainer.GetChildren())
        {
            if (child is CollisionPolygon2D collisionPolygon2D)
            {
                CarveNode<CollisionPolygon2D>(
                    clippingPolygon,
                    collisionContainer,
                    collisionPolygon2D,
                    (node) => node.Polygon,
                    (node, polygon) => node.Polygon = polygon,
                    (polygon) =>
                    {
                        CollisionPolygon2D newCollisionPolygon2D = new()
                        {
                            Polygon = polygon
                        };
                        return newCollisionPolygon2D;
                    },
                    false
                );
            }
        }

        polygonMask.Polygons.Clear();
        int indicesCount = 0;
        List<Vector2> newPolygon = new();
        Godot.Collections.Array<Godot.Collections.Array<int>> polygonsIndices = new();

        foreach (CollisionPolygon2D collisionPolygon in collisionContainer.GetChildren())
        {
            newPolygon.AddRange(collisionPolygon.Polygon);

            Godot.Collections.Array<int> polygonIndices = new();
            for (int i = 0; i < collisionPolygon.Polygon.Length; i++)
            {
                polygonIndices.Add(indicesCount + i);
            }
            indicesCount += collisionPolygon.Polygon.Length;
            polygonsIndices.Add(polygonIndices);
        }

        polygonMask.Polygons = (Godot.Collections.Array)polygonsIndices;
        polygonMask.Polygon = newPolygon.ToArray();
    }


    /// <summary>
    /// Carves the clippingPolygon away from the collision and occluders in the tilemap.
    /// </summary>
    /// <param name="clippingPolygon">The polygon to carve out of the tilemap.</param>
    private static void CarveNode<T>(
        CollisionPolygon2D clippingPolygon,
        Node2D nodeContainer,
        T polygonNode,
        Func<T, Vector2[]> getPolygon,
        Action<T, Vector2[]> setPolygon,
        Func<Vector2[], T> constructNode,
        bool isOccluder
    )
        where T : Node
    {
        List<Vector2> globalClippingPolygon = (clippingPolygon.Polygon * clippingPolygon.GlobalTransform.AffineInverse()).ToList();

        List<Vector2> offsets = new() { Vector2.Zero, Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right };

        Godot.Collections.Array<Vector2[]> roundedClippedPolygons = new();
        int err = 0;

        foreach (Vector2 offset in offsets)
        {
            Vector2[] offsetClippingPolygon = globalClippingPolygon.Select((x) => x + offset).ToArray();
            Godot.Collections.Array<Vector2[]> clippedPolygons = Geometry2D.ClipPolygons(getPolygon(polygonNode), offsetClippingPolygon);

            Godot.Collections.Array<Vector2[]> roundedPolygons = new();
            foreach (Vector2[] polygon in clippedPolygons)
            {
                Vector2[] roundedPolygon = RoundPolygon(polygon, out err);
                if (err == 2) break;
                roundedPolygons.Add(roundedPolygon);
            }

            // We can't construct the polygon
            if (err == 2)
            {
                GD.PushWarning("Count not construct polygon, retrying with offset.");
                continue;
            }
            else
            {
                roundedClippedPolygons = roundedPolygons;
                break;
            }
        }

        // We can't find a way to do this clip so just exit.
        if (err == 2){
            GD.PushWarning("Could not find a way to clip polygon.");
            return;
        }

        switch (roundedClippedPolygons.Count)
        {
            case 0:
                polygonNode.Free();
                break;
            case 1:
                setPolygon(polygonNode, roundedClippedPolygons[0]);
                if (IsPolygonTooSmall(getPolygon(polygonNode), isOccluder))
                {
                    polygonNode.Free();
                }
                break;
            case 2:
                if (IsHole(roundedClippedPolygons.ToList()))
                {
                    // NOTE: maybe just ignore
                    /*
                    foreach (Vector2[] polygon in SplitPolygon(globalClippingPolygon))
                    {
                        T newNode = constructNode(Geometry2D.IntersectPolygons(polygon, getPolygon(polygonNode))[0]);

                        if (!IsPolygonTooSmall(getPolygon(newNode), true))
                        {
                            nodeContainer.AddChild(newNode);
                        }
                    }
                    polygonNode.Free();
                    */
                }
                else
                {
                    setPolygon(polygonNode, roundedClippedPolygons[0]);
                    if (IsPolygonTooSmall(getPolygon(polygonNode), isOccluder))
                    {
                        polygonNode.Free();
                    }
                    for (int i = 1; i < roundedClippedPolygons.Count; i++)
                    {
                        T newNode = constructNode( roundedClippedPolygons[i]);
                        if (!IsPolygonTooSmall(getPolygon(newNode), isOccluder))
                        {
                            nodeContainer.AddChild(newNode);
                        }
                    }
                }
                break;
            default:
                setPolygon(polygonNode, roundedClippedPolygons[0]);
                if (IsPolygonTooSmall(getPolygon(polygonNode), isOccluder))
                {
                    polygonNode.Free();
                }
                for (int i = 1; i < roundedClippedPolygons.Count; i++)
                {
                    T newNode = constructNode( roundedClippedPolygons[i]);
                    if (!IsPolygonTooSmall(getPolygon(newNode), isOccluder))
                    {
                        nodeContainer.AddChild(newNode);
                    }
                }
                break;
        }
    }

    private static Vector2[] RoundPolygon(Vector2[] polygon, out int err)
    {
        List<Vector2> newPolygon = new();
        foreach (Vector2 vertex in polygon)
        {
            Vector2 roundedVertex = vertex.Round();
            if (!newPolygon.Contains(roundedVertex))
            {
                newPolygon.Add(roundedVertex);
            }
        }
        if (newPolygon.Count < 3)
        {
            GD.PushWarning("Rounded polygon invalid.");
            err = 1;
            return Array.Empty<Vector2>();
        }
        else
        {
            // Confirm we can actually build the polygon.
            var res = Geometry2D.DecomposePolygonInConvex(newPolygon.ToArray());
            if (res.Count < 1)
            {
                err = 2;
                return Array.Empty<Vector2>();
            }
            err = 0;
            return newPolygon.ToArray();
        }
    }

    private static bool IsHole(List<Vector2[]> clippedPolygons)
    {
        return Geometry2D.IsPolygonClockwise(clippedPolygons[0]) || Geometry2D.IsPolygonClockwise(clippedPolygons[1]);
    }

    // TODO: what does this do, its defo going to crash
    private static List<Vector2[]> SplitPolygon(Vector2[] polygon)
    {
        float avgXPos = AvgPos(polygon).X;
        float minY = polygon.MinBy((x) => x.Y).Y;
        float maxY = polygon.MaxBy((x) => x.Y).Y;

        Vector2[] leftSubquadrant = (Vector2[])polygon.Clone();
        leftSubquadrant[1] = new Vector2(avgXPos, leftSubquadrant[1].Y);
        leftSubquadrant[2] = new Vector2(avgXPos, leftSubquadrant[2].Y);

        Vector2[] rightSubquadrant = (Vector2[])polygon.Clone();
        rightSubquadrant[0] = new Vector2(avgXPos, rightSubquadrant[0].Y);
        rightSubquadrant[3] = new Vector2(avgXPos, rightSubquadrant[3].Y);

        Vector2[] polygonA = Geometry2D.ClipPolygons(leftSubquadrant, polygon)[0];
        Vector2[] polygonB = Geometry2D.ClipPolygons(rightSubquadrant, polygon)[0];

        return new List<Vector2[]> { polygonA, polygonB };
    }

    private static Vector2 AvgPos(Vector2[] positions)
    {
        Vector2 sum = new();
        foreach (Vector2 pos in positions)
        {
            sum += pos;
        }
        return sum / positions.Length;
    }

    private static bool IsPolygonTooSmall(Vector2[] polygon, bool isOccluder = false)
    {
        if (polygon.Length < 3) return true;

        float maxX = 0;
        float minX = polygon[0].X;
        float maxY = 0;
        float minY = polygon[0].Y;

        foreach (Vector2 vertex in polygon)
        {
            if (vertex.X > maxX)
            {
                maxX = vertex.X;
            }
            if (vertex.Y > maxY)
            {
                maxY = vertex.Y;
            }
            if (vertex.X < minX)
            {
                minX = vertex.X;
            }
            if (vertex.Y < minY)
            {
                minY = vertex.Y;
            }
        }

        return (maxX - minX) * (maxY - minY) < (isOccluder ? 5 : 30);
    }


    private void ConstructColliders(TileMapLayer tilemap)
    {
        GD.Print("Constructing colliders");
        List<Vector2[]> collisionPolygons = new();
        Godot.Collections.Array<Vector2I> usedCellCoords = tilemap.GetUsedCells();

        foreach (Vector2I cellCoord in usedCellCoords)
        {
            Vector2[] polygon = GetTilePolygon(GetTilePoints(cellCoord, cellSize));
            collisionPolygons.Add(polygon);

            // Remove the collision polygons, we're using our own
            TileData tileData = tileMap.GetCellTileData(cellCoord);
            for(int i = 0; i < tileData.GetCollisionPolygonsCount(0); i++) {
                tileData.SetCollisionPolygonPoints(0, i, null);
            }
        }

        collisionPolygons = CombinePolygons(collisionPolygons);

        foreach (Vector2[] polygon in collisionPolygons)
        {
            CollisionPolygon2D collisionPolygon2D = new()
            {
                Polygon = polygon
            };
            collisionContainer.AddChild(collisionPolygon2D);
        }
    }

    private void CombineOccluders(TileMapLayer tilemap)
    {
        GD.Print("Combining occluders");
        List<Vector2[]> occluderPolygons = new();
        Godot.Collections.Array<Vector2I> usedCellCoords = tilemap.GetUsedCells();

        // Collect the global positions of every occluder polygon in the tileMap
        foreach (Vector2I cellCoords in usedCellCoords)
        {
            TileData tileData = tileMap.GetCellTileData(cellCoords);
            OccluderPolygon2D occluder = tileData.GetOccluder(0);
            if (occluder != null)
            {
                List<Vector2> adjustedPolygon = new();
                foreach (Vector2 pos in occluder.Polygon)
                {
                    Vector2 globalCellPos = tileMap.GlobalPosition + ((cellCoords * cellSize) + (cellSize / 2));
                    adjustedPolygon.Add(pos + globalCellPos);
                }
                occluderPolygons.Add(adjustedPolygon.ToArray());
            }
        }

        // Remove the occluders from the tileMap as we're about to construct our own
        foreach (Vector2I cellCoords in usedCellCoords)
        {
            TileData tileData = tileMap.GetCellTileData(cellCoords);
            tileData.SetOccluder(0, null);
        }

        occluderPolygons = CombinePolygons(occluderPolygons);

        foreach (Vector2[] occluderPolygon in occluderPolygons)
        {
            LightOccluder2D lightOccluder = new();
            OccluderPolygon2D occluderPolygon2D = new()
            {
                Polygon = occluderPolygon
            };
            lightOccluder.Occluder = occluderPolygon2D;
            mainOccludersContainer.AddChild(lightOccluder);
            copyOccludersContainer.AddChild(lightOccluder.Duplicate());

            // TODO: Create a StaticBody2D for each occluder, put it on collision layer 2, and we can use that for raycasting?
            //       this is because when we destroy the occluders the line of sight will change.
            CollisionPolygon2D collisionPolygon2D = new()
            {
                Polygon = occluderPolygon
            };
            visionCollisionContainer.AddChild(collisionPolygon2D);
        }
    }

    private static List<Vector2[]> CombinePolygons(List<Vector2[]> polygons)
    {
        GD.Print("Combining polygons");
        List<Vector2[]> polygonsToRemove = new();
        Dictionary<int, bool> indexToRemove = new();

        while (true)
        {
            polygonsToRemove.Clear();
            indexToRemove.Clear();

            for (int i = 0; i < polygons.Count; i++)
            {
                if (indexToRemove.GetValueOrDefault(i, false)) continue;

                Vector2[] polygonA = polygons[i];

                for (int j = 0; j < i; j++)
                {
                    if (indexToRemove.GetValueOrDefault(j, false)) continue;

                    Vector2[] polygonB = polygons[j];
                    List<Vector2[]> mergedPolygons = Geometry2D.MergePolygons(polygonA, polygonB).ToList();

                    // The polygons did not merge so skip.
                    if (mergedPolygons.Count != 1) continue;

                    polygons[j] = mergedPolygons[0];

                    polygonsToRemove.Add(polygonA);
                    indexToRemove[i] = true;
                    break;
                }
            }

            if (polygonsToRemove.Count == 0) break;

            foreach (Vector2[] occluderPolygon in polygonsToRemove)
            {
                int index = polygons.IndexOf(occluderPolygon);
                polygons.RemoveAt(index);
            }
        }

        return polygons;
    }

    private static Vector2[] GetTilePolygon(List<Vector2> points)
    {
        return new Vector2[] { points[0], points[1], points[1], points[2], points[2], points[3], points[3], points[0] };
    }

    private static List<Vector2> GetTilePoints(Vector2 position, Vector2 cellSize)
    {
        return new List<Vector2>() {
            new(position.X * cellSize.X, position.Y * cellSize.Y + cellSize.Y),
            new(position.X * cellSize.X, position.Y * cellSize.Y),
            new(position.X * cellSize.X + cellSize.X, position.Y * cellSize.Y),
            new(position.X * cellSize.X + cellSize.X, position.Y * cellSize.Y + cellSize.Y),
        };
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
