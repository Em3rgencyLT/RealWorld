using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Domain;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Structure : MonoBehaviour
{
    [SerializeField]
    private MapElement.ID mapId;
    [SerializeField]
    private List<Vector3> baseVerticePositions;

    public void Build(MapElement.ID id, List<Vector3> baseVerticePositions) {
        this.mapId = id;
        this.baseVerticePositions = baseVerticePositions;

        List<Vector2> vertices2D = new List<Vector2>();
        baseVerticePositions
            .ForEach(position => {
                vertices2D.Add(new Vector2(position.x, position.z));
            });

        Vector2[] vertices2DArray = vertices2D.ToArray();
        Triangulator tr = new Triangulator(vertices2DArray);
        int[] indices = tr.Triangulate();
 
        Vector3[] vertices = new Vector3[vertices2DArray.Length];
        for (int i=0; i<vertices.Length; i++) {
            vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
        }
 
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
 
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        filter.mesh = mesh;
    }
}
