using System;
using System.Collections.Generic;
using UnityEngine;
using Domain;
using Utility;

public class MapObject : MonoBehaviour
{
    [SerializeField]
    protected MapElement.ID mapId;
    [SerializeField]
    protected List<Vector3> baseVerticePositions;

    [SerializeField]
    protected Material testMaterial;

    protected GameObject MakePlane(List<Vector3> verticePositions, List<Vector2> projectionPositions, String name = "Object") {
        Triangulator tr = new Triangulator(projectionPositions);
        int[] indices = tr.Triangulate();

        GameObject obj = new GameObject(name);
        MeshFilter filter = (MeshFilter)obj.AddComponent(typeof(MeshFilter));
        MeshRenderer renderer = (MeshRenderer)obj.AddComponent(typeof(MeshRenderer));
        Mesh mesh = new Mesh();

        mesh.vertices = verticePositions.ToArray();
        mesh.triangles = indices;
        mesh.uv = UvCalculator.CalculateUVs(verticePositions.ToArray());
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.mesh = mesh;
        renderer.material = testMaterial;

        return obj;
    }
    
    protected float FindTerrainHeight(Vector3 point) {
        int layerMask = 1 << 10;
        point.y = 10000;

        RaycastHit hit;

        if (!Physics.Raycast(point, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            Debug.LogWarning("No terrain found at x:" + point.x + " z:" + point.z);
            return 0f;
        }

        return hit.point.y;
    }
}
