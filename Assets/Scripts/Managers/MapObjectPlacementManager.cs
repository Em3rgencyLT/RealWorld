using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Domain;
using Utility;

public class MapObjectPlacementManager : Singleton<MapObjectPlacementManager>
{
    protected MapObjectPlacementManager() { }

    [SerializeField]
    private double originLatitude = 55.237884;
    [SerializeField]
    private double originLongitude = 22.276198;
    private Coordinates projectionOrigin;
    [SerializeField]
    private double offset = 0.03;

    private Coordinates bottomCoordinates;
    private Coordinates topCoordinates;
    private Dictionary<MapElement.ID, MapElement> worldObjectData;
    private Dictionary<Coordinates, double> worldElevationData;
    
    public GameObject structurePrefab;
    public GameObject roadPrefab;
    private GameObject terrainObject;
    private GameObject structureParentObject;
    private GameObject roadParentObject;

    public double OriginLongitude { get { return originLongitude; } }
    public double OriginLatitude { get { return originLatitude; } }
    public Coordinates ProjectionOrigin { 
        get { return projectionOrigin; } 
        //TODO: setting the projection origin to something else should also wipe and redraw the entire world
        set { this.projectionOrigin = value; }
    }

    public Dictionary<MapElement.ID, MapElement> WorldObjectData { get { return worldObjectData; } }
    public Dictionary<Coordinates, double> WorldElevationData { get { return worldElevationData; } }

    void Start() {
        this.topCoordinates = Coordinates.of(OriginLatitude + offset*0.89, OriginLongitude + offset*1.55);
        this.bottomCoordinates = ProjectionOrigin;

        this.worldObjectData = MapData.GetObjectData(bottomCoordinates, topCoordinates);
        this.worldElevationData = MapData.GetElevationData(bottomCoordinates, topCoordinates);

        PlaceTerrain();
        StartCoroutine("PlaceBuildings");
        StartCoroutine("PlaceRoads");
    }

    private void PlaceTerrain() {
        this.terrainObject = new GameObject("Terrain");

        MeshFilter filter = (MeshFilter)terrainObject.AddComponent(typeof(MeshFilter));
        MeshRenderer renderer = (MeshRenderer)terrainObject.AddComponent(typeof(MeshRenderer));
        Mesh mesh = new Mesh();

        int pointCountX = 0;
        int pointCountY = 0;
        
        double dummyLat = worldElevationData.First().Key.Latitude;
        foreach(KeyValuePair<Coordinates, double> entry in worldElevationData)
        {
            if(!entry.Key.Latitude.Equals7DigitPrecision(dummyLat)){
                break;
            }
            pointCountX++;
        }
        pointCountY = worldElevationData.Count / pointCountX;
        List<Vector3> terrainPoints = new List<Vector3>();

        foreach(KeyValuePair<Coordinates, double> entry in worldElevationData)
        {
            Vector3 point = CoordinateMath.CoordinatesToWorldPosition(entry.Key);
            point.y = (float)entry.Value;
            terrainPoints.Add(point);
        }

        int[] triangles = new int[(pointCountX - 1) * (pointCountY - 1) * 6];
        for (int ti = 0, vi = 0, y = 0; y < pointCountY - 1; y++, vi++) {
			for (int x = 0; x < pointCountX - 1; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + pointCountX;
				triangles[ti + 5] = vi + pointCountX + 1;
			}
		}

        mesh.vertices = terrainPoints.ToArray();
        mesh.triangles = triangles;
        mesh.uv = UvCalculator.CalculateUVs(terrainPoints.ToArray());
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.mesh = mesh;
        renderer.material = new Material(Shader.Find("Standard"));
    }

    private IEnumerator PlaceBuildings() {
        int built = 0;
        this.structureParentObject = new GameObject("Structures");
        foreach (MapElement mapElement in worldObjectData.Values)
        {
            if(mapElement.Data.ContainsKey(MapNodeKey.KeyType.Building)) {
                List<Vector3> verticePositions = new List<Vector3>();
                mapElement.References.ForEach(reference => {
                    verticePositions.Add(worldObjectData[reference].Coordinates.Position);
                });
                if(verticePositions.Count > 3) {
                    verticePositions.RemoveAt(verticePositions.Count - 1);
                    GameObject structureObject = Instantiate(structurePrefab, Vector3.zero, Quaternion.identity);
                    structureObject.name = string.IsNullOrWhiteSpace(mapElement.GetAddress()) ? "Building" : mapElement.GetAddress();
                    Structure structureScript = structureObject.GetComponent<Structure>();
                    structureScript.Build(mapElement, verticePositions);
                    structureObject.transform.parent = structureParentObject.transform;
                    built++;
                }
                
                if(built % 500 == 0) {
                    yield return null;
                }
            }
        }

        yield return null;
    }

    private IEnumerator PlaceRoads() {
        int built = 0;
        this.roadParentObject = new GameObject("Roads");
        foreach (MapElement mapElement in worldObjectData.Values)
        {
            if(!mapElement.Data.ContainsKey(MapNodeKey.KeyType.Highway)) {
                continue;
            }

            List<Coordinates> waypoints = new List<Coordinates>();
            mapElement.References
            .ForEach(reference => {
                waypoints.Add(worldObjectData[reference].Coordinates);
            });

            List<Vector3> leftVerticePositions = new List<Vector3>();
            List<Vector3> rightVerticePositions = new List<Vector3>();
            float width = Road.GuessRoadWidth(mapElement.Data[MapNodeKey.KeyType.Highway]);

            waypoints
            .ForEach(waypoint => {
                int index = waypoints.IndexOf(waypoint);
                Vector3 forward = Vector3.zero;
                    
                if(index < waypoints.Count - 1) {
                    forward += waypoints[index + 1].Position - waypoint.Position; 
                }
                if(index > 0) { 
                    forward += waypoint.Position - waypoints[index - 1].Position; 
                }

                forward.Normalize();
                Vector3 position = waypoint.Position;
                if(index == 0) {
                    position -= forward * width / 10;
                }
                if(index == waypoints.Count - 1) {
                    position += forward * width / 10;
                }

                Vector3 left = new Vector3(-forward.z, 0f, forward.x);                       
                leftVerticePositions.Add(position + left * width);
                rightVerticePositions.Add(position - left * width);
            });

            if(leftVerticePositions.Count > 1 && rightVerticePositions.Count > 1) {
                GameObject roadObject = Instantiate(roadPrefab, Vector3.zero, Quaternion.identity);
                roadObject.name = string.IsNullOrWhiteSpace(mapElement.GetRoadName()) ? "Road" : mapElement.GetRoadName();
                Road roadScript = roadObject.GetComponent<Road>();
                List<Vector3> verticePositions = new List<Vector3>();
                verticePositions.AddRange(leftVerticePositions);
                rightVerticePositions.Reverse();
                verticePositions.AddRange(rightVerticePositions);
                roadScript.Build(mapElement, verticePositions);
                roadObject.transform.parent = roadParentObject.transform;
                built++;
            }
                
            if(built % 250 == 0) {
                yield return null;
            }
        }

        yield return null;
    }
}
