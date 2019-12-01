using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;
using Domain;
using Domain.Tuples;
using Services;
using Utility;

public class MapObjectPlacementManager : Singleton<MapObjectPlacementManager>
{
    protected MapObjectPlacementManager()
    {
    }

    [SerializeField] private double locationLatitude = 54.899737;
    [SerializeField] private double locationLongitude = 23.900396;
    [SerializeField] [Range(1, 4)] private int worldSize = 1;
    [SerializeField] private Material terrainMaterial;

    private CoordinateBox _mapDataCoordinateBox;
    private CoordinateBox _terrainCoordinateBox;
    [SerializeField] private GameObject structurePrefab;
    [SerializeField] private GameObject roadPrefab;
    private GameObject terrainObject;
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
            throw new ArgumentNullException(
                "Structure Prefab cannot be null! Please set in inspector of MapObjectPlacementManager!");
        }

        if (roadPrefab == null)
        {
            roadPrefab = null;
            throw new ArgumentNullException(
                "Road Prefab cannot be null! Please set in inspector of MapObjectPlacementManager!");
        }
    }

    void Start()
    {
        BuildWorld();
    }

    private void SetupCoordinates()
    {
        float offset = worldSize * Parameters.WORLD_SIZE_MULTIPLIER;
        ProjectionOrigin = Coordinates.projectionOriginOf(locationLatitude - offset, locationLongitude - offset);
        MapObjectOrigin = Coordinates.of(locationLatitude - offset / 2, locationLongitude - offset / 2);

        Coordinates mapDataBottomCoordinates = MapObjectOrigin;
        Coordinates mapDataTopCoordinates =
            Coordinates.of(MapObjectOrigin.Latitude + offset, MapObjectOrigin.Longitude + offset);
        _mapDataCoordinateBox = CoordinateBox.of(mapDataBottomCoordinates, mapDataTopCoordinates);

        Coordinates terrainBottomCoordinates = ProjectionOrigin;
        Coordinates terrainTopCoordinates =
            Coordinates.of(ProjectionOrigin.Latitude + offset * 2, ProjectionOrigin.Longitude + offset * 2);
        _terrainCoordinateBox = CoordinateBox.of(terrainBottomCoordinates, terrainTopCoordinates);
    }

    private void BuildWorld()
    {
        var heightmapService = new HeighmapService(worldSize);
        var heightmapResolution = heightmapService.GetHeightmapResolution();
        var elevationService = new ElevationService(heightmapResolution);
        var osmParserService = new OSMParserService();
        
        WorldElevationData = elevationService.GetElevationMap(_terrainCoordinateBox);
        var osmData = new OSMDataService().GetDataForArea(_mapDataCoordinateBox);
        WorldObjectData = osmParserService.Parse(osmData);
        var heightmap = heightmapService.GetHeightmapMatrix(WorldElevationData);

        terrainObject =
            new TerrainBuilder(heightmap, _terrainCoordinateBox, terrainMaterial).Build();
        StartCoroutine("PlaceBuildings");
        StartCoroutine("PlaceRoads");
    }

    private IEnumerator PlaceBuildings()
    {
        int built = 0;
        structureParentObject = new GameObject("Structures");
        foreach (MapElement mapElement in WorldObjectData.Values)
        {
            if (!mapElement.Data.ContainsKey(MapNodeKey.KeyType.Building))
            {
                continue;
            }

            List<Vector3> verticePositions = new List<Vector3>();
            mapElement.References.ForEach(reference =>
            {
                verticePositions.Add(WorldObjectData[reference].Coordinates.Position);
            });
            if (verticePositions.Count < 3)
            {
                continue;
            }

            verticePositions.RemoveAt(verticePositions.Count - 1);
            GameObject structureObject = Instantiate(structurePrefab, structureParentObject.transform);
            structureObject.name = string.IsNullOrWhiteSpace(mapElement.GetAddress())
                ? "Building"
                : mapElement.GetAddress();
            Structure structureScript = structureObject.GetComponent<Structure>();
            structureScript.Build(mapElement, verticePositions);
            built++;
        }

        if (built % 500 == 0)
        {
            yield return null;
        }

        yield return null;
    }

    private IEnumerator PlaceRoads()
    {
        int built = 0;
        roadParentObject = new GameObject("Roads");
        foreach (MapElement mapElement in WorldObjectData.Values)
        {
            if (!mapElement.Data.ContainsKey(MapNodeKey.KeyType.Highway))
            {
                continue;
            }

            List<Coordinates> waypoints = new List<Coordinates>();
            mapElement.References
                .ForEach(reference => { waypoints.Add(WorldObjectData[reference].Coordinates); });

            List<Vector3> leftVerticePositions = new List<Vector3>();
            List<Vector3> rightVerticePositions = new List<Vector3>();
            float width = Road.GuessRoadWidth(mapElement.Data[MapNodeKey.KeyType.Highway]);

            waypoints
                .ForEach(waypoint =>
                {
                    int index = waypoints.IndexOf(waypoint);
                    Vector3 forward = Vector3.zero;

                    if (index < waypoints.Count - 1)
                    {
                        forward += waypoints[index + 1].Position - waypoint.Position;
                    }

                    if (index > 0)
                    {
                        forward += waypoint.Position - waypoints[index - 1].Position;
                    }

                    forward.Normalize();
                    Vector3 position = waypoint.Position;
                    if (index == 0)
                    {
                        position -= forward * width / 10;
                    }

                    if (index == waypoints.Count - 1)
                    {
                        position += forward * width / 10;
                    }

                    Vector3 left = new Vector3(-forward.z, 0f, forward.x);
                    leftVerticePositions.Add(position + left * width);
                    rightVerticePositions.Add(position - left * width);
                });

            if (leftVerticePositions.Count > 1 && rightVerticePositions.Count > 1)
            {
                GameObject roadObject = Instantiate(roadPrefab, roadParentObject.transform);
                roadObject.name = string.IsNullOrWhiteSpace(mapElement.GetRoadName())
                    ? "Road"
                    : mapElement.GetRoadName();
                Road roadScript = roadObject.GetComponent<Road>();
                List<Vector3> verticePositions = new List<Vector3>();
                verticePositions.AddRange(leftVerticePositions);
                rightVerticePositions.Reverse();
                verticePositions.AddRange(rightVerticePositions);
                roadScript.Build(mapElement, verticePositions);
                built++;
            }

            if (built % 250 == 0)
            {
                yield return null;
            }
        }

        yield return null;
    }
}