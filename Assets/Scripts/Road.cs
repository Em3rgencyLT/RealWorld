using System.Collections.Generic;
using UnityEngine;
using Domain;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Road : MapObject
{
    public void Build(MapElement mapElement, List<Vector3> baseVerticePositions) {
        this.mapId = mapElement.Id;
        this.baseVerticePositions = baseVerticePositions;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> vertices2D = new List<Vector2>();
        baseVerticePositions.ForEach(position => {
            vertices.Add(new Vector3(position.x, 0f, position.z));
            vertices2D.Add(new Vector2(position.x, position.z));
        });

        GameObject meshParent = new GameObject("Mesh");
        meshParent.transform.parent = transform;

        MakePlane(vertices, vertices2D, "Surface").transform.parent = meshParent.transform;
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
