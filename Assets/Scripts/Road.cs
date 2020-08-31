using System.Collections.Generic;
using UnityEngine;
using Domain;
using GSD.Roads;
using Services;

[RequireComponent(typeof(GSDRoad))]
public class Road : MapObject
{
    private List<Vector3> _splineVertexPositions;
    private GSDRoad _gsdRoad;

    public void Build(TerrainHeightService terrainHeightService, MapElement mapElement, List<Vector3> splineVertexPositions) {
        mapId = mapElement.Id;
        _splineVertexPositions = splineVertexPositions;
        _gsdRoad = GetComponent<GSDRoad>();

        for (int i = 1; i < splineVertexPositions.Count - 1; i++)
        {
            float height = terrainHeightService.GetHeightForPoint(_splineVertexPositions[i]);
            Vector3 position = new Vector3(_splineVertexPositions[i].x, height, _splineVertexPositions[i].z);
            GSDConstruction.CreateNode(_gsdRoad, false, position, false);
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
