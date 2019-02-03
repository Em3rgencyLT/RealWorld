using UnityEngine;
using System.Linq;
using Utility;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class TerrainSegment : MonoBehaviour
{
    private Vector3[,] points;
    public Material terrainMaterial;

    public void Build(Vector3[,] points) {
        this.points = points;

        int pointCountX = points.GetLength(1);
        int pointCountY = points.GetLength(0);

        int[] triangles = new int[(pointCountX - 1) * (pointCountY - 1) * 6];
        for (int ti = 0, vi = 0, y = 0; y < pointCountY - 1; y++, vi++) {
            for (int x = 0; x < pointCountX - 1; x++, ti += 6, vi++) {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + pointCountX;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi + 1;
                triangles[ti + 4] = vi + pointCountX;
                triangles[ti + 5] = vi + pointCountX + 1;
            }
        }

        Vector3[] pointsArr = points.Cast<Vector3>().ToArray();

        Mesh mesh = new Mesh();
        mesh.vertices = pointsArr;
        mesh.triangles = triangles;
        mesh.uv = UvCalculator.CalculateUVs(pointsArr);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        gameObject.GetComponent<MeshRenderer>().material = terrainMaterial;
    }
}
