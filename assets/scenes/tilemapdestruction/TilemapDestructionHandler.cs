using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class TilemapDestructionHandler : Node
{
    [Export(PropertyHint.NodeType, "TileMapLayer")]
    TileMapLayer tileMap;

    [Export(PropertyHint.NodeType, "Polygon2D")]
    Polygon2D polygonMask;

    [Export(PropertyHint.NodeType, "StaticBody2D")]
    StaticBody2D collisionContainer;

    [Export(PropertyHint.NodeType, "Node2D")]
    Node2D mainOccludersContainer;

    [Export(PropertyHint.NodeType, "Node2D")]
    Node2D copyOccludersContainer;

    const float wallsLayer = 2.0f;
    Vector2I cellSize = new(8, 8);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (tileMap != null)
        {
            CombineOccluders(tileMap);
            ConstructColliders(tileMap);
        }
    }

    public void CarveOccluders(CollisionPolygon2D clippingPolygon) {
        foreach (Node child in mainOccludersContainer.GetChildren()) {
            if (child is LightOccluder2D lightOccluder2D) {
                Carve(clippingPolygon);
            }
        }
    }

    public void CarveColliders(CollisionPolygon2D clippingPolygon) {
        Carve(clippingPolygon);
    }

    /// <summary>
    /// Carves the clippingPolygon away from the collision and occluders in the tilemap.
    /// </summary>
    /// <param name="clippingPolygon">The polygon to carve out of the tilemap.</param>
    public void Carve(CollisionPolygon2D clippingPolygon)
    {
        foreach (Node child in mainOccludersContainer.GetChildren())
        {
            if (child is LightOccluder2D lightOccluder2D)
            {
                Vector2[] globalClippingPolygon = clippingPolygon.Polygon * clippingPolygon.Transform.AffineInverse();
                List<Vector2[]> clippedPolygons = Geometry2D.ClipPolygons(lightOccluder2D.Occluder.Polygon, globalClippingPolygon).ToList();

                switch (clippedPolygons.Count)
                {
                    case 0:
                        lightOccluder2D.Free();
                        break;
                    case 1:
                        lightOccluder2D.Occluder.Polygon = clippedPolygons[0];
                        if (IsPolygonTooSmall(lightOccluder2D.Occluder.Polygon, true))
                        {
                            lightOccluder2D.Free();
                        }
                        break;
                    case 2:
                        if (IsHole(clippedPolygons))
                        {
                            foreach (Vector2[] polygon in SplitPolygon(globalClippingPolygon))
                            {
                                LightOccluder2D newLightOccluder2D = new()
                                {
                                    Occluder = new()
                                };
                                newLightOccluder2D.Occluder.Polygon = Geometry2D.IntersectPolygons(polygon, lightOccluder2D.Occluder.Polygon)[0];
                                if (!IsPolygonTooSmall(newLightOccluder2D.Occluder.Polygon, true))
                                {
                                    mainOccludersContainer.AddChild(newLightOccluder2D);
                                }
                            }
                            lightOccluder2D.Free();
                        }
                        else
                        {
                            lightOccluder2D.Occluder.Polygon = clippedPolygons[0];
                            if (IsPolygonTooSmall(lightOccluder2D.Occluder.Polygon, true))
                            {
                                lightOccluder2D.Free();
                            }
                            for (int i = 1; i < clippedPolygons.Count; i++)
                            {
                                LightOccluder2D newLightOccluder2D = new()
                                {
                                    Occluder = new()
                                };
                                newLightOccluder2D.Occluder.Polygon = clippedPolygons[i];
                                if (!IsPolygonTooSmall(newLightOccluder2D.Occluder.Polygon, true))
                                {
                                    mainOccludersContainer.AddChild(newLightOccluder2D);
                                }
                            }
                        }
                        break;
                    default:
                        lightOccluder2D.Occluder.Polygon = clippedPolygons[0];
                        if (IsPolygonTooSmall(lightOccluder2D.Occluder.Polygon, true))
                        {
                            lightOccluder2D.Free();
                        }
                        for (int i = 1; i < clippedPolygons.Count; i++)
                        {
                            LightOccluder2D newLightOccluder2D = new()
                            {
                                Occluder = new()
                            };
                            newLightOccluder2D.Occluder.Polygon = clippedPolygons[i];
                            if (!IsPolygonTooSmall(newLightOccluder2D.Occluder.Polygon, true))
                            {
                                mainOccludersContainer.AddChild(newLightOccluder2D);
                            }
                        }
                        break;
                }
            }
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
        Vector2[] leftSubquadrant = Array.Empty<Vector2>();
        leftSubquadrant[1] = new Vector2(avgXPos, leftSubquadrant[1].Y);
        leftSubquadrant[2] = new Vector2(avgXPos, leftSubquadrant[2].Y);

        Vector2[] rightSubquadrant = Array.Empty<Vector2>();
        rightSubquadrant[0] = new Vector2(avgXPos, rightSubquadrant[1].Y);
        rightSubquadrant[3] = new Vector2(avgXPos, rightSubquadrant[2].Y);

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

        return (maxX - minX) * (maxY - minY) < (isOccluder ? 5 : 25);
    }


    private void ConstructColliders(TileMapLayer tilemap)
    {
        List<Vector2[]> collisionPolygons = new();
        Godot.Collections.Array<Vector2I> usedCellCoords = tilemap.GetUsedCells();

        foreach (Vector2 cellCoord in usedCellCoords)
        {
            Vector2[] polygon = GetTilePolygon(GetTilePoints(cellCoord, cellSize));
            _ = collisionPolygons.Append(polygon);
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
                    _ = adjustedPolygon.Append(pos + globalCellPos);
                }
                _ = occluderPolygons.Append(adjustedPolygon.ToArray());
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
        }
    }

    private static List<Vector2[]> CombinePolygons(List<Vector2[]> polygons)
    {
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

                    _ = polygonsToRemove.Append(polygonA);
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
