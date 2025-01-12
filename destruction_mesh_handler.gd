# https://stackoverflow.com/questions/67412060/godot-engine-merging-more-than-two-polygons-using-geometrys-merge-polygons-2d
# https://github.com/a327ex/a327ex-template/blob/dd0a63aefbc3cae268d165deef8fdf931f776f55/engine/map/tilemap.lua
# https://github.com/a327ex/blog/issues/5
# https://godotengine.org/qa/30006/build-a-collision-polygon-from-tilemap-outline
# https://gist.github.com/afk-mario/15b5855ccce145516d1b458acfe29a28
extends Node2D

@export var tilemap: TileMapLayer
@export var polygon_mask: Polygon2D
@export var occluders_container: Node2D
@export var occluders_container_copy: Node2D

const walls_layer := 2
const cell_size = Vector2i(8,8) 

func _ready() -> void:
    if tilemap:
        combine_occluders(tilemap)
        handle_walls(tilemap)


func carve_occluders(clipping_polygon: CollisionPolygon2D):
    """
    Carves the clipping_polygon away from the quadrant
    """
    for maybe_light_occluder in occluders_container.get_children():
        if (!maybe_light_occluder is LightOccluder2D):
            return

        var light_occluder: LightOccluder2D = maybe_light_occluder

        var global_clipping_polygon = clipping_polygon.polygon * clipping_polygon.transform.affine_inverse()
        var clipped_polygons = Geometry2D.clip_polygons(light_occluder.occluder.polygon, global_clipping_polygon)
        var n_clipped_polygons = len(clipped_polygons)
        print("CLIPPED: " + str(n_clipped_polygons))
        match n_clipped_polygons:
            0:
                # clipping_polygon completely overlaps colpol
                remove_occluder(light_occluder)
                #light_occluder.free()
            1:
                # Clipping produces only one polygon
                light_occluder.occluder.polygon = clipped_polygons[0]
                if (is_too_small(light_occluder.occluder.polygon, true)):
                    remove_occluder(light_occluder)
                    #light_occluder.free()
            2:
                # Check if you carved a hole (one of the two polygons
                # is clockwise). If so, split the polygon in two that
                # together make a "hollow" collision shape
                if _is_hole(clipped_polygons):
                    # split and add
                    for p in _split_polygon(global_clipping_polygon):
                        var colpol = LightOccluder2D.new()
                        colpol.occluder = OccluderPolygon2D.new()
                        colpol.occluder.polygon = Geometry2D.intersect_polygons(p,light_occluder.occluder.polygon)[0]
                        if (!is_too_small(colpol.occluder.polygon, true)):
                            occluders_container.add_child(colpol)
                    remove_occluder(light_occluder)
                    #light_occluder.free()
                # if its not a hole, behave as in match _
                else:
                    light_occluder.occluder.polygon = clipped_polygons[0]
                    if (is_too_small(light_occluder.occluder.polygon, true)):
                        remove_occluder(light_occluder)
                        #light_occluder.free()
                    for i in range(n_clipped_polygons-1):
                        var colpol = LightOccluder2D.new()
                        colpol.occluder = OccluderPolygon2D.new()
                        colpol.occluder.polygon = clipped_polygons[i+1]
                        if (!is_too_small(colpol.occluder.polygon, true)):
                            occluders_container.add_child(colpol)
            
            # if more than two polygons, simply add all of
            # them to the quadrant
            _:
                light_occluder.occluder.polygon = clipped_polygons[0]
                if (is_too_small(light_occluder.occluder.polygon, true)):
                    remove_occluder(light_occluder)
                    #light_occluder.free()
                for i in range(n_clipped_polygons-1):
                    var colpol = LightOccluder2D.new()
                    colpol.occluder = OccluderPolygon2D.new()
                    colpol.occluder.polygon = clipped_polygons[i+1]
                    if (!is_too_small(colpol.occluder.polygon, true)):
                        occluders_container.add_child(colpol)

    for child in occluders_container_copy.get_children():
        child.free();

    for child in occluders_container.get_children():
        var child_copy = child.duplicate()
        occluders_container_copy.add_child(child_copy)


func carve_geometry(clipping_polygon: CollisionPolygon2D):
    """
    Carves the clipping_polygon away from the quadrant
    """
    var staticbody = tilemap.get_child(0)
    for maybePolygon in staticbody.get_children():
        if (!maybePolygon is CollisionPolygon2D):
            return

        var collisionShape: CollisionPolygon2D = maybePolygon

        var global_clipping_polygon = clipping_polygon.polygon * clipping_polygon.transform.affine_inverse()
        var clipped_polygons = Geometry2D.clip_polygons(collisionShape.polygon, global_clipping_polygon)
        var n_clipped_polygons = len(clipped_polygons)
        match n_clipped_polygons:
            0:
                # clipping_polygon completely overlaps colpol
                remove_polygon(collisionShape)
                #collisionShape.free()
            1:
                # Clipping produces only one polygon
                collisionShape.polygon = clipped_polygons[0]
                if (is_too_small(collisionShape.polygon)):
                    remove_polygon(collisionShape)
                    #collisionShape.free()
                print(clipped_polygons)
            2:
                # Check if you carved a hole (one of the two polygons
                # is clockwise). If so, split the polygon in two that
                # together make a "hollow" collision shape
                if _is_hole(clipped_polygons):
                    # split and add
                    for p in _split_polygon(global_clipping_polygon):
                        var colpol = CollisionPolygon2D.new()
                        colpol.polygon = Geometry2D.intersect_polygons(p, collisionShape.polygon)[0]
                        if (!is_too_small(colpol.polygon)):
                            staticbody.add_child(colpol)
                    remove_polygon(collisionShape)
                    #collisionShape.free()
                # if its not a hole, behave as in match _
                else:
                    collisionShape.polygon = clipped_polygons[0]
                    if (is_too_small(collisionShape.polygon)):
                        remove_polygon(collisionShape)
                        #collisionShape.free()
                    for i in range(n_clipped_polygons-1):
                        var colpol = CollisionPolygon2D.new()
                        colpol.polygon = clipped_polygons[i+1]
                        if (!is_too_small(colpol.polygon)):
                            staticbody.add_child(colpol)
            
            # if more than two polygons, simply add all of
            # them to the quadrant
            _:
                collisionShape.polygon = clipped_polygons[0]
                if (is_too_small(collisionShape.polygon)):
                    remove_polygon(collisionShape)
                    #collisionShape.free()
                for i in range(n_clipped_polygons-1):
                    var colpol = CollisionPolygon2D.new()
                    colpol.polygon = clipped_polygons[i+1]
                    if (!is_too_small(colpol.polygon)):
                        staticbody.add_child(colpol)

    polygon_mask.polygon.clear()
    polygon_mask.polygons.clear()

    var indices: int = 0;
    var polygon_count: int = 0;
    var new_polygon: PackedVector2Array = [];

    await get_tree().process_frame

    # Construct the visiblity mask
    for maybePolygon in staticbody.get_children():
        new_polygon.append_array(maybePolygon.polygon)

        var a: PackedInt32Array = []
        polygon_mask.polygons.insert(polygon_count, a)
        for i in range(maybePolygon.polygon.size()):
            polygon_mask.polygons[polygon_count].append(indices + i)
        indices += maybePolygon.polygon.size()
        polygon_count += 1

    polygon_mask.polygon = new_polygon


func remove_polygon(polygon: CollisionPolygon2D):
    polygon.free()

func remove_occluder(occluder: LightOccluder2D):
    occluder.free()

func is_too_small(polygon: PackedVector2Array, is_occluder: bool = false) -> bool:
    var maxX = 0;
    var minX = polygon[0].x;
    var maxY = 0;
    var minY = polygon[0].y;
    for point in polygon:
        if point.x > maxX:
            maxX = point.x
        if point.y > maxY:
            maxY = point.y
        if point.y < minY:
            minY = point.y
        if point.x < minX:
            minX = point.x

    print("is_occluder " + str(is_occluder) + " size: " + str((maxX - minX) * (maxY - minY)))
    return (maxX - minX) * (maxY - minY) < (5 if is_occluder else 25)

func _is_hole(clipped_polygons):
    """
    If either of the two polygons after clipping
    are clockwise, then you have carved a hole
    """
    return Geometry2D.is_polygon_clockwise(clipped_polygons[0]) or Geometry2D.is_polygon_clockwise(clipped_polygons[1])


var default_quadrant_polygon: Array = []

func _split_polygon(clip_polygon: Array):
    """
    Returns two polygons produced by vertically
    splitting split_polygon in half
    """
    var avg_x = _avg_position(clip_polygon).x
    var left_subquadrant = default_quadrant_polygon.duplicate()
    left_subquadrant[1] = Vector2(avg_x, left_subquadrant[1].y)
    left_subquadrant[2] = Vector2(avg_x, left_subquadrant[2].y)
    var right_subquadrant = default_quadrant_polygon.duplicate()
    right_subquadrant[0] = Vector2(avg_x, right_subquadrant[0].y)
    right_subquadrant[3] = Vector2(avg_x, right_subquadrant[3].y)
    var pol1 = Geometry2D.clip_polygons(left_subquadrant, clip_polygon)[0]
    var pol2 = Geometry2D.clip_polygons(right_subquadrant, clip_polygon)[0]
    return [pol1, pol2]


func _avg_position(array: Array):
    """
    Average 2D position in an
    array of positions
    """
    var sum = Vector2()
    for p in array:
        sum += p
    return sum/len(array)


func combine_occluders(tile_map: TileMapLayer) -> void:
    # This static body will have all the polygons
    # genereted as childs
    #var occluder_container = Node2D.new()
    #tile_map.add_child(occluder_container)

    var occluder_polygons: Array[PackedVector2Array] = []
    var used_cells_coords := tile_map.get_used_cells()

    for cell_coords in used_cells_coords:
        var tile_data = tile_map.get_cell_tile_data(cell_coords)
        var occluder: OccluderPolygon2D = tile_data.get_occluder(0)
        if (occluder):
            var adjusted_polygon: PackedVector2Array = []
            for pos in occluder.polygon:
                var global_cell_pos: Vector2 = tilemap.global_position + Vector2((cell_coords * cell_size) + (cell_size/2))
                adjusted_polygon.append(pos + global_cell_pos)
            occluder_polygons.append(adjusted_polygon)
        #tile_map._tile_data_runtime_update(cell_coords, tile_data)

    for cell_coords in used_cells_coords:
        var tile_data = tile_map.get_cell_tile_data(cell_coords)
        tile_data.set_occluder(0, null) # Remove the occluder, were going to construct a new one

    var occluders_to_remove := []
    var index_to_remove := {}

    while true:
        occluders_to_remove = []
        index_to_remove = {}

        for i in occluder_polygons.size():
            if index_to_remove.get(i, false) == true:
                continue
            
            var a: PackedVector2Array = occluder_polygons[i]

            # Loop from the start of the array to
            # the current polygon
            for j in i:
                # Skip if the polygon is due to remove
                if index_to_remove.get(j, false) == true:
                    continue

                var b: PackedVector2Array = occluder_polygons[j]
                var merged_occluders: Array[PackedVector2Array] = Geometry2D.merge_polygons(a, b)

                # The polygons dind't merge so skip to the next loop
                if merged_occluders.size() != 1:
                    continue

                # Replace the polygon with the merged one
                occluder_polygons[j] = merged_occluders[0]
                
                # Mark to remove the already merged polygon
                occluders_to_remove.append(a)
                index_to_remove[i] = true
                break

        # There is no polygon to remove so we finished
        if occluders_to_remove.size() == 0:
            break

        # Remove the polygons marked to be removed
        for occluder in occluders_to_remove:
            var index = occluder_polygons.find(occluder)
            occluder_polygons.pop_at(index)

    # Create all the polygon shapes from the result
    # and add them to the static body
    for occluder in occluder_polygons:
        var light_occluder = LightOccluder2D.new()
        var occluder_polygon_2d = OccluderPolygon2D.new()
        occluder_polygon_2d.polygon = occluder
        light_occluder.occluder = occluder_polygon_2d
        occluders_container.add_child(light_occluder)

        var light_occluder_copy = LightOccluder2D.new()
        var occluder_polygon_2d_copy = OccluderPolygon2D.new()
        occluder_polygon_2d_copy.polygon = occluder
        light_occluder_copy.occluder = occluder_polygon_2d_copy
        occluders_container_copy.add_child(light_occluder_copy)


# Ignore all tilemaps not named walls
func handle_walls(tile_map: TileMapLayer) -> void:
    if tile_map.name != "Walls":
        return

    # This static body will have all the polygons
    # genereted as childs
    var node := StaticBody2D.new()
    node.collision_layer = walls_layer
    node.collision_mask = 0
    tile_map.add_child(node)

    var polygons := []
    var used_cells := tile_map.get_used_cells()

    # Create edges
    for cell in used_cells:
        var polygon = get_tile_polygon(get_points(cell, cell_size))
        polygons.append(polygon)

    # Polygons to remove will hold the actual polygons
    var polygons_to_remove := []
    # Index to remove is a dictionary so that searching is faster
    var index_to_remove := {}

    while true:
        # Clear the polygons to remove
        polygons_to_remove = []
        index_to_remove = {}
        
        # Start looping
        for i in polygons.size():
            # Skip if the polygon is due to remove
            if index_to_remove.get(i, false) == true:
                continue

            var a = polygons[i]

            # Loop from the start of the array to
            # the current polygon
            for j in i:
                # Skip if the polygon is due to remove
                if index_to_remove.get(j, false) == true:
                    continue

                var b = polygons[j]
                var merged_polygons = Geometry2D.merge_polygons(a, b)

                # The polygons dind't merge so skip to the next loop
                if merged_polygons.size() != 1:
                    continue

                # Replace the polygon with the merged one
                polygons[j] = merged_polygons[0]
                
                # Mark to remove the already merged polygon
                polygons_to_remove.append(a)
                index_to_remove[i] = true
                break

        # There is no polygon to remove so we finished
        if polygons_to_remove.size() == 0:
            break

        # Remove the polygons marked to be removed
        for polygon in polygons_to_remove:
            var index = polygons.find(polygon)
            polygons.pop_at(index)

    # Create all the polygon shapes from the result
    # and add them to the static body
    for polygon in polygons:
        var polygon_shape = CollisionPolygon2D.new()
        polygon_shape.polygon = polygon
        node.add_child(polygon_shape)


# Generate all the points in a tile
func get_points(position: Vector2, cell_size: Vector2) -> Array:
    var x = position.x
    var y = position.y
    #1   2
    #
    #0   3
    return [
        Vector2(x * cell_size.x, y * cell_size.y + cell_size.y),  # 0
        Vector2(x * cell_size.x, y * cell_size.y),  # 1
        Vector2(x * cell_size.x + cell_size.x, y * cell_size.y),  # 2
        Vector2(x * cell_size.x + cell_size.x, y * cell_size.y + cell_size.y)  # 3
    ]

# Generate the edges/polygon from a tile points
func get_tile_polygon(points) -> Array:
    return [points[0], points[1], points[1], points[2], points[2], points[3], points[3], points[0]]
