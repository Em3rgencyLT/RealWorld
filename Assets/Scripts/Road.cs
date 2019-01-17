using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Domain;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Road : MonoBehaviour
{
    [SerializeField]
    private MapElement.ID mapId;
    [SerializeField]
    private List<Vector3> baseVerticePositions;

    public void Build(MapElement mapElement, List<Vector3> baseVerticePositions) {
        this.mapId = mapElement.Id;
        this.baseVerticePositions = baseVerticePositions;

        List<Vector2> vertices2D = new List<Vector2>();
        baseVerticePositions
            .ForEach(position => {
                vertices2D.Add(new Vector2(position.x, position.z));
            });

        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();
 
        Vector3[] vertices = new Vector3[vertices2D.Count];
        for (int i=0; i<vertices.Length; i++) {
            vertices[i] = new Vector3(vertices2D[i].x, 0f, vertices2D[i].y);
        }
 
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
 
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        filter.mesh = mesh;
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Diffuse"));
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
