using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Domain;
using Services;
using Utility;

public class Structure : MapObject
{
    private TerrainHeightService _terrainHeightService;
    private float buildingHeight;

    /*Note to future self: the base vertex positions only contain x and z data. First the height for the roof is calculated,
    based on terrain height at given point and assumed building height. Then the walls are extruded downwards towards the terrain. 
    The highest Y value is chosen for the roof and propagated across all other top vertices, to prevent roofs that slope 
    with the terrain. The lowest point of the terrain is found and propagated across all bottom wall vertices, to prevent 
    floating buildings.*/
    public void Build(TerrainHeightService terrainHeightService, MapElement mapElement, List<Vector3> baseVerticePositions)
    {
        _terrainHeightService = terrainHeightService;
        mapId = mapElement.Id;
        this.baseVerticePositions = baseVerticePositions;
        buildingHeight = GuessBuildingHeight(mapElement.Data[MapNodeKey.KeyType.Building]);
        List<Vector3> verticesTop = UnifyStructureRoofLevel(this.baseVerticePositions);

        if (this.baseVerticePositions.Count < 3) {
            throw new ArgumentException("Can't built a structure with less than 3 vertices at it's base!");
        }

        GameObject meshParent = new GameObject("Mesh");
        meshParent.transform.parent = transform;
        MakeRoof(verticesTop).transform.parent = meshParent.transform;
        MakeWalls(verticesTop).ForEach(wall => {
            wall.transform.parent = meshParent.transform;
        });
    }

    private List<Vector3> UnifyStructureRoofLevel(List<Vector3> baseVerticePositions)
    {
        List<Vector3> points = new List<Vector3>();
        float highestPoint = baseVerticePositions.Max(position => _terrainHeightService.GetHeightForPoint(position) + buildingHeight);
        baseVerticePositions.ForEach(point => points.Add(new Vector3(point.x, highestPoint, point.z)));
        return points;
    }

    private GameObject MakeRoof(List<Vector3> vertices) {
        List<Vector2> vertices2D = new List<Vector2>();
        vertices.ForEach(position => {
            vertices2D.Add(new Vector2(position.x, position.z));
        });

        return MakePlane(vertices, vertices2D, "Roof");
    }

    private List<GameObject> MakeWalls(List<Vector3> vertices) {
        List<GameObject> walls = new List<GameObject>();
        List<Vector2> vertices2D = new List<Vector2>();

        vertices.ForEach(vertex => {
            vertices2D.Add(new Vector2(vertex.x, vertex.z));
        });
        
        float lowestPoint = baseVerticePositions.Min(position => _terrainHeightService.GetHeightForPoint(position));

        for(int i = 0; i < vertices.Count; i++) {
            int i0 = i;
            int i1 = i + 1;

            if(i == vertices.Count - 1) {
                i1 = 0;
            }

            //Actual 3d position of each vertex
            List<Vector3> wallVertices = new List<Vector3>();
            wallVertices.Add(vertices[i0]);
            wallVertices.Add(vertices[i1]);
            wallVertices.Add(new Vector3(vertices[i1].x, lowestPoint, vertices[i1].z));
            wallVertices.Add(new Vector3(vertices[i0].x, lowestPoint, vertices[i0].z));
            
            //Dummy data, but in correct order and relative position. Which is all that matters for texture mapping.
            List<Vector2> relativeVertices = new List<Vector2>();
            if(Triangulator.Area(vertices2D) > 0) {
                relativeVertices.Add(new Vector2(0f, 1f));
                relativeVertices.Add(new Vector2(1f, 1f));
                relativeVertices.Add(new Vector2(1f, 0f));
                relativeVertices.Add(new Vector2(0f, 0f));
            } else {
                relativeVertices.Add(new Vector2(0f, 1f));
                relativeVertices.Add(new Vector2(0f, 0f));
                relativeVertices.Add(new Vector2(1f, 0f));
                relativeVertices.Add(new Vector2(1f, 1f));
            }

            walls.Add(MakePlane(wallVertices, relativeVertices, "Wall"));
        }
        
        return walls;
    }

    private float GuessBuildingHeight(string type) {
        switch(type) {
            case "yes":
                return 5f;
            case "apartments":
                return 27f;
            case "school":
                return 12f;
            case "church":
                return 12f;
            case "house":
                return 4f;
            case "commercial":
                return 9f;
            case "garages":
                return 2.5f;
            case "garage":
                return 2.5f;
            case "retail":
                return 9f;
            case "industrial":
                return 18f;
            case "collapsed":
                return 2f;
            case "residential":
                return 5f;
            case "greenhouse":
                return 2f;
            default:
                Debug.LogWarning("Don't have a guess for building height of type: " + type);
                return 5f;
        }
    }
}
