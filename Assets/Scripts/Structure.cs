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
            vertices[i] = new Vector3(vertices2D[i].x, 5f, vertices2D[i].y);
        }

        List<Vector3> vertexList = vertices.ToList();
        List<int> indexList = indices.ToList();

        vertices2D.ForEach(vertex => {
            vertexList.Add(new Vector3(vertex.x, 0f, vertex.y));
        });

        for (int i=0; i<vertices2D.Count; i++) {
            indexList.Add(i);
            indexList.Add(i + vertices2D.Count);
            if(i == vertices2D.Count - 1) {
                indexList.Add(vertices2D.Count);
                indexList.Add(vertices2D.Count);
            } else {
                indexList.Add(i + vertices2D.Count + 1);
                indexList.Add(i + vertices2D.Count + 1);
            }
            indexList.Add((i + 1) % vertices2D.Count);
            indexList.Add(i);
        }
 
        Mesh mesh = new Mesh();
        mesh.vertices = vertexList.ToArray();
        mesh.triangles = indexList.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
 
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        filter.mesh = mesh;
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Diffuse"));
    }
}
