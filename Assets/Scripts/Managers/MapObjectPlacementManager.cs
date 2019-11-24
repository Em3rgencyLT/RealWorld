using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
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
    [SerializeField]
    private double offset = 0.03;

    private Coordinates bottomCoordinates;
    private Coordinates topCoordinates;
    [SerializeField]
    private GameObject structurePrefab;
    [SerializeField]
    private GameObject roadPrefab;
    [SerializeField]
    private GameObject terrainSegmentPrefab;
    private GameObject terrainObject;
    private GameObject structureParentObject;
    private GameObject roadParentObject;

    public double OriginLongitude { get { return originLongitude; } }
    public double OriginLatitude { get { return originLatitude; } }
    //TODO: setting the projection origin to something else should also wipe and redraw the entire world
    public Coordinates ProjectionOrigin { get; set; }

    public Dictionary<MapElement.ID, MapElement> WorldObjectData { get; private set; }
    public Vector3[,] WorldElevationData { get; private set; }

    private void Awake()
    {
        //Make sure doubles accept . for decimal instead of ,
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        
        //get rid of the private field not set warnings and throw proper errors if they actually aren't set
        if (structurePrefab == null)
        {
            structurePrefab = null;
            throw new ArgumentNullException("Structure Prefab cannot be null! Please set in inspector of MapObjectPlacementManager!");
        }
        if (roadPrefab == null)
        {
            roadPrefab = null;
            throw new ArgumentNullException("Road Prefab cannot be null! Please set in inspector of MapObjectPlacementManager!");
        }
        if (terrainSegmentPrefab == null)
        {
            terrainSegmentPrefab = null;
            throw new ArgumentNullException("Terrain Sement Prefab cannot be null! Please set in inspector of MapObjectPlacementManager!");
        }
    }

    void Start()
    {
        BuildWorld();
    }

    private void BuildWorld()
    {
        this.topCoordinates = Coordinates.of(OriginLatitude + offset * 0.89, OriginLongitude + offset * 1.55);
        this.bottomCoordinates = ProjectionOrigin;

        this.WorldElevationData = MapData.GetElevationData(bottomCoordinates, topCoordinates);
        this.WorldObjectData = MapData.GetObjectData(bottomCoordinates, topCoordinates);

        StartCoroutine("PlaceTerrain");
        StartCoroutine("PlaceBuildings");
        StartCoroutine("PlaceRoads");
    }

    private IEnumerator PlaceTerrain() {
        int built = 0;
        this.terrainObject = new GameObject("Terrain");

        int dataSizeX = WorldElevationData.GetLength(0);
        int dataSizeY = WorldElevationData.GetLength(1);

        //max 11x11 verts = 200 triangles. Keeping under convex mesh collider 255 triangle limit
        int maxChunkSizeX = 11;
        int maxChunkSizeY = 11;

        //Divide entire elevation dataset into chunks
        for(int x = 0; x < dataSizeX; x += maxChunkSizeX - 1)
        {
            for(int y = 0; y < dataSizeY; y += maxChunkSizeY -1)
            {
                int xEnd = x + maxChunkSizeX;
                int yEnd = y + maxChunkSizeY;
                if(xEnd >= dataSizeX)
                {
                    xEnd = dataSizeX;
                }
                if(yEnd >= dataSizeY)
                {
                    yEnd = dataSizeY;
                }

                int sizeX = xEnd - x;
                int sizeY = yEnd - y;

                if(sizeX < 2 || sizeY < 2)
                {
                    continue;
                }

                Vector3[,] chunkData = new Vector3[sizeX, sizeY];

                //Copy over data points of desired chunk
                for(int x0 = 0, x1 = x; x1 < xEnd; x0++, x1++)
                {
                    for (int y0 = 0, y1 = y; y1 < yEnd; y0++, y1++)
                    {
                        chunkData[x0, y0] = WorldElevationData[x1, y1];
                    }
                }

                //Instantiate chunk and build mesh
                GameObject terrainSegment = Instantiate(terrainSegmentPrefab, terrainObject.transform);
                TerrainSegment segment = terrainSegment.GetComponent<TerrainSegment>();
                segment.Build(chunkData);
                built++;

                if (built % 100 == 0)
                {
                    yield return null;
                }
            }
        }

        yield return null;
    }

    private IEnumerator PlaceBuildings() {
        int built = 0;
        this.structureParentObject = new GameObject("Structures");
        foreach (MapElement mapElement in WorldObjectData.Values)
        {
            if(mapElement.Data.ContainsKey(MapNodeKey.KeyType.Building)) {
                List<Vector3> verticePositions = new List<Vector3>();
                mapElement.References.ForEach(reference => {
                    verticePositions.Add(WorldObjectData[reference].Coordinates.Position);
                });
                if(verticePositions.Count > 3) {
                    verticePositions.RemoveAt(verticePositions.Count - 1);
                    GameObject structureObject = Instantiate(structurePrefab, structureParentObject.transform);
                    structureObject.name = string.IsNullOrWhiteSpace(mapElement.GetAddress()) ? "Building" : mapElement.GetAddress();
                    Structure structureScript = structureObject.GetComponent<Structure>();
                    structureScript.Build(mapElement, verticePositions);
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
        foreach (MapElement mapElement in WorldObjectData.Values)
        {
            if(!mapElement.Data.ContainsKey(MapNodeKey.KeyType.Highway)) {
                continue;
            }

            List<Coordinates> waypoints = new List<Coordinates>();
            mapElement.References
            .ForEach(reference => {
                waypoints.Add(WorldObjectData[reference].Coordinates);
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
                GameObject roadObject = Instantiate(roadPrefab, roadParentObject.transform);
                roadObject.name = string.IsNullOrWhiteSpace(mapElement.GetRoadName()) ? "Road" : mapElement.GetRoadName();
                Road roadScript = roadObject.GetComponent<Road>();
                List<Vector3> verticePositions = new List<Vector3>();
                verticePositions.AddRange(leftVerticePositions);
                rightVerticePositions.Reverse();
                verticePositions.AddRange(rightVerticePositions);
                roadScript.Build(mapElement, verticePositions);
                built++;
            }
                
            if(built % 250 == 0) {
                yield return null;
            }
        }

        yield return null;
    }
}
