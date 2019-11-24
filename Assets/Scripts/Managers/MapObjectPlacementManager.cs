using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;
using Domain;
using Domain.Tuples;
using Utility;

public class MapObjectPlacementManager : Singleton<MapObjectPlacementManager>
{
    protected MapObjectPlacementManager() { }

    [SerializeField]
    private double locationLatitude = 54.899737;
    [SerializeField]
    private double locationLongitude = 23.900396;
    [SerializeField]
    [Range(1, 2)]
    private int worldSize = 1;
    [SerializeField] private Material terrainMaterial;

    //FIXME: Related to MapData#GetRawElevationData, the ratio between this multiplier and step must be a power of 2
    private readonly float WORLD_SIZE_MULTIPLIER = 0.03225f;
    private CoordinateBounding mapDataCoordinateBounding;
    private CoordinateBounding terrainCoordinateBounding;
    [SerializeField]
    private GameObject structurePrefab;
    [SerializeField]
    private GameObject roadPrefab;
    private GameObject unityTerrainObject;
    private GameObject structureParentObject;
    private GameObject roadParentObject;

    //TODO: setting the projection origin to something else should also wipe and redraw the entire world
    public Coordinates ProjectionOrigin { get; private set; }
    public Coordinates MapObjectOrigin { get; private set; }

    public Dictionary<MapElement.ID, MapElement> WorldObjectData { get; private set; }
    public Vector3[,] WorldElevationData { get; private set; }

    private void Awake()
    {
        //Make sure doubles accept . for decimal instead of ,
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        SetupCoordinates();
        
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
    }

    void Start()
    {
        BuildWorld();
    }

    private void SetupCoordinates()
    {
        float offset = worldSize * WORLD_SIZE_MULTIPLIER;
        ProjectionOrigin = Coordinates.projectionOriginOf(locationLatitude - offset, locationLongitude - offset);
        MapObjectOrigin = Coordinates.of(locationLatitude - offset / 2, locationLongitude - offset / 2);
        
        Coordinates mapDataBottomCoordinates = MapObjectOrigin;
        Coordinates mapDataTopCoordinates = Coordinates.of(MapObjectOrigin.Latitude + offset, MapObjectOrigin.Longitude + offset);
        mapDataCoordinateBounding = CoordinateBounding.of(mapDataBottomCoordinates, mapDataTopCoordinates);
        
        Coordinates terrainBottomCoordinates = ProjectionOrigin;
        Coordinates terrainTopCoordinates = Coordinates.of(ProjectionOrigin.Latitude + offset * 2, ProjectionOrigin.Longitude + offset * 2);
        terrainCoordinateBounding = CoordinateBounding.of(terrainBottomCoordinates, terrainTopCoordinates);
    }

    private void BuildWorld()
    {
        WorldElevationData = MapData.GetElevationData(terrainCoordinateBounding);
        WorldObjectData = MapData.GetObjectData(mapDataCoordinateBounding);
        
        PlaceUnityTerrain();
        StartCoroutine("PlaceBuildings");
        StartCoroutine("PlaceRoads");
    }

    private void PlaceUnityTerrain()
    {
        unityTerrainObject = new GameObject("Unity Terrain");
        unityTerrainObject.layer = 15;
        TerrainData terrainData = new TerrainData();
        
        //
        terrainData.size = new Vector3(terrainCoordinateBounding.TopCoordinates.Position.x/Mathf.Pow(2, worldSize+1), 8848f, terrainCoordinateBounding.TopCoordinates.Position.z/Mathf.Pow(2, worldSize+1));
        terrainData.heightmapResolution = (int)Math.Pow(2, 6 + worldSize) + 1;
        terrainData.baseMapResolution = 1024;
        terrainData.SetDetailResolution(1024, 32);
        
        float[,] heightmap = new float[terrainData.heightmapResolution,terrainData.heightmapResolution];
        for (int i = 0; i < terrainData.heightmapResolution; i++)
        {
            for (int j = 0; j < terrainData.heightmapResolution; j++)
            {
                heightmap[i,j] = WorldElevationData[i, j].y / 8848f;
            }
        }
        terrainData.SetHeights(0,0, heightmap);
        
        TerrainCollider terrainCollider = unityTerrainObject.AddComponent<TerrainCollider>();
        Terrain terrain = unityTerrainObject.AddComponent<Terrain>();
        terrain.materialTemplate = terrainMaterial;
 
        terrainCollider.terrainData = terrainData;
        terrain.terrainData = terrainData;
        unityTerrainObject.isStatic = true;
    }

    private IEnumerator PlaceBuildings() {
        int built = 0;
        structureParentObject = new GameObject("Structures");
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
