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
        float height = GuessBuildingHeight(mapElement.Data[MapNodeKey.KeyType.Building]);
        for (int i=0; i<vertices.Length; i++) {
            vertices[i] = new Vector3(vertices2D[i].x, height, vertices2D[i].y);
        }

        List<Vector3> vertexList = vertices.ToList();
        List<int> indexList = indices.ToList();

        float area = Triangulator.Area(vertices2D);
        vertices2D.ForEach(vertex => {
            vertexList.Add(new Vector3(vertex.x, 0f, vertex.y));
        });

        //FIXME: techdebt and lazyness
        if(area < 0) {
            for (int i=0; i<vertices2D.Count; i++) {
                if(i == vertices2D.Count - 1) {
                    indexList.Add(i);
                    indexList.Add(i + vertices2D.Count);
                    indexList.Add(vertices2D.Count);
                    indexList.Add(vertices2D.Count);
                    indexList.Add((i + 1) % vertices2D.Count);
                    indexList.Add(i);
                } else {
                    indexList.Add(i);
                    indexList.Add(i + vertices2D.Count);
                    indexList.Add(i + vertices2D.Count + 1);
                    indexList.Add(i + vertices2D.Count + 1);
                    indexList.Add((i + 1) % vertices2D.Count);
                    indexList.Add(i);
                }
            }
        } else {
            for (int i=0; i<vertices2D.Count; i++) {
                if(i == vertices2D.Count - 1) {
                    indexList.Add(vertices2D.Count);
                    indexList.Add(i + vertices2D.Count);
                    indexList.Add(i);
                    indexList.Add(i);
                    indexList.Add((i + 1) % vertices2D.Count);
                    indexList.Add(vertices2D.Count);
                } else {
                    indexList.Add(i + vertices2D.Count + 1);
                    indexList.Add(i + vertices2D.Count);
                    indexList.Add(i);
                    indexList.Add(i);
                    indexList.Add((i + 1) % vertices2D.Count);
                    indexList.Add(i + vertices2D.Count + 1);
                }
            }
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
