﻿using System.Collections.Generic;
using UnityEngine;
using Domain;
using Services;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Road : MapObject
{
    private List<Vector3> _leftVerticePositions;
    private List<Vector3> _rightVerticePositions;
    
    public void Build(TerrainHeightService terrainHeightService, MapElement mapElement, List<Vector3> leftVerticePositions, List<Vector3> rightVerticePositions) {
        mapId = mapElement.Id;
        _leftVerticePositions = leftVerticePositions;
        _rightVerticePositions = rightVerticePositions;

        GameObject meshParent = new GameObject("Mesh");
        meshParent.transform.parent = transform;
        
        //FIXME: Manually triangulate entire mesh at once, so it's not a bunch of tiny separate plane meshes
        //Needs to be manually triangulated to force neat triangles. Triangulator utility seems to suck at this with large vertex counts.
        for (int i = 0; i < _leftVerticePositions.Count - 1; i++)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> vertices2D = new List<Vector2>();
            
            Vector3 botLeft = _leftVerticePositions[i];
            Vector3 topLeft = _leftVerticePositions[i + 1];
            Vector3 topRight = _rightVerticePositions[i + 1];
            Vector3 botRight = _rightVerticePositions[i];

            //FIXME: probably want to do this earlier
            botLeft.y = terrainHeightService.GetHeightForPoint(botLeft) + 0.3f;
            topLeft.y = terrainHeightService.GetHeightForPoint(topLeft) + 0.3f;
            topRight.y = terrainHeightService.GetHeightForPoint(topRight) + 0.3f;
            botRight.y = terrainHeightService.GetHeightForPoint(botRight) + 0.3f;
            
            Vector2 v2BotLeft = new Vector2(botLeft.x, botLeft.z);
            Vector2 v2topLeft = new Vector2(topLeft.x, topLeft.z);
            Vector2 v2topRight = new Vector2(topRight.x, topRight.z);
            Vector2 v2botRight = new Vector2(botRight.x, botRight.z);
            
            vertices.AddRange(new List<Vector3>{botLeft, topLeft, topRight, botRight});
            vertices2D.AddRange(new List<Vector2>{v2BotLeft, v2topLeft, v2topRight, v2botRight});
            
            MakePlane(vertices, vertices2D, $"Surface {i}").transform.parent = meshParent.transform;
        }
    }

    public static float GuessRoadWidth(string type) {
        switch(type) {
            case "residential":
                return 4f;
            case "footway":
                return 2f;
            case "path":
                return 2f;
            case "service":
                return 3f;
            default:
                Debug.LogWarning("Don't have a guess for road width of type: " + type);
                return 4f;
        }
    }
}
