using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Domain;
using Domain.Tuples;
using Services;
using UnityEngine;
using Utility;

namespace Managers
{
    public class MapObjectPlacementManager : Singleton<MapObjectPlacementManager>
    {
        protected MapObjectPlacementManager()
        {
        }

        [SerializeField] private double locationLatitude = 54.899737;
        [SerializeField] private double locationLongitude = 23.900396;
        [SerializeField] [Range(2,5)] private int worldSize = 1;
        [SerializeField] private Material terrainMaterial;

        private AreaBounds<Coordinates> _mapDataAreaBounds;
        private AreaBounds<Coordinates> _terrainAreaBounds;
        [SerializeField] private GameObject structurePrefab;
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private PlayerSpawner _playerSpawner;
        private GameObject _structureParentObject;
        private GameObject _roadParentObject;

        private CoordinatePositionService _coordinatePositionService;
        private SRTMDataService _srtmDataService;
        private OSMDataService _osmDataService;
        private HeightmapService _heightmapService;
        private OSMParserService _osmParserService;
        private TerrainHeightService _terrainHeightService;

        public Coordinates MapObjectOrigin { get; private set; }

        public Dictionary<MapElement.ID, MapElement> WorldObjectData { get; private set; }

        private void Awake()
        {
            //Make sure doubles accept . for decimal instead of ,
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            
            var offset = worldSize * Parameters.WORLD_SIZE_MULTIPLIER;
            var projectionOrigin = Coordinates.of(locationLatitude - offset, locationLongitude - offset);
        
            _coordinatePositionService = new CoordinatePositionService(projectionOrigin);
            _srtmDataService = new SRTMDataService();
            _osmDataService = new OSMDataService();
            _heightmapService = new HeightmapService(worldSize, _srtmDataService);
            _osmParserService = new OSMParserService(_srtmDataService, _coordinatePositionService);
        
            SetupAreas(offset);

            //get rid of the private field not set warnings and throw proper errors if they actually aren't set
            if (structurePrefab == null)
            {
                structurePrefab = null;
                throw new ArgumentException(
                    "Structure Prefab cannot be null! Please set in inspector of MapObjectPlacementManager!");
            }

            if (roadPrefab == null)
            {
                roadPrefab = null;
                throw new ArgumentException(
                    "Road Prefab cannot be null! Please set in inspector of MapObjectPlacementManager!");
            }
        }

        void Start()
        {
            BuildWorld();
            var centerCoordinates = Coordinates.of(locationLatitude, locationLongitude);
            var playerPosition = _coordinatePositionService.GetCoordinatesWithPosition(centerCoordinates, 300).Position;
            _playerSpawner.SpawnPlayer(playerPosition);
        }

        private void SetupAreas(float offset)
        {
            MapObjectOrigin = Coordinates.of(locationLatitude - offset / 2, locationLongitude - offset / 2);

            Coordinates mapDataBottomCoordinates = MapObjectOrigin;
            Coordinates mapDataTopCoordinates =
                Coordinates.of(MapObjectOrigin.Latitude + offset, MapObjectOrigin.Longitude + offset);
            _mapDataAreaBounds = AreaBounds<Coordinates>.of(mapDataBottomCoordinates, mapDataTopCoordinates);

            Coordinates terrainBottomCoordinates = _coordinatePositionService.ProjectionOrigin;
            Coordinates terrainTopCoordinates =
                Coordinates.of(terrainBottomCoordinates.Latitude + offset * 2, terrainBottomCoordinates.Longitude + offset * 2);
            _terrainAreaBounds = AreaBounds<Coordinates>.of(terrainBottomCoordinates, terrainTopCoordinates);
        }

        private void BuildWorld()
        {
            var heightmap = _heightmapService.GetHeightmapMatrix(_terrainAreaBounds);
            var osmData = _osmDataService.GetDataForArea(_mapDataAreaBounds);
            WorldObjectData = _osmParserService.Parse(osmData);
            var terrainTopPoint = _coordinatePositionService.PositionFromCoordinates(_terrainAreaBounds.TopPoint);

            Terrain terrain = new TerrainBuilder(heightmap, terrainTopPoint, terrainMaterial).Build();
            _terrainHeightService = new TerrainHeightService(terrain); //FIXME: should be called in awake, or not be globally available
            StartCoroutine("PlaceBuildings");
            StartCoroutine("PlaceRoads");
        }

        private IEnumerator PlaceBuildings()
        {
            int built = 0;
            _structureParentObject = new GameObject("Structures");
            foreach (MapElement mapElement in WorldObjectData.Values)
            {
                if (!mapElement.Data.ContainsKey(MapNodeKey.KeyType.Building))
                {
                    continue;
                }

                List<Vector3> verticePositions = new List<Vector3>();
                mapElement.References.ForEach(reference =>
                {
                    verticePositions.Add(WorldObjectData[reference].CoordinatesWithPosition.Position);
                });
                if (verticePositions.Count < 3)
                {
                    continue;
                }

                verticePositions.RemoveAt(verticePositions.Count - 1);
                GameObject structureObject = Instantiate(structurePrefab, _structureParentObject.transform);
                structureObject.name = string.IsNullOrWhiteSpace(mapElement.GetAddress())
                    ? "Building"
                    : mapElement.GetAddress();
                Structure structureScript = structureObject.GetComponent<Structure>();
                structureScript.Build(_terrainHeightService, mapElement, verticePositions);
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
            _roadParentObject = new GameObject("Roads");
            foreach (MapElement mapElement in WorldObjectData.Values)
            {
                if (!mapElement.Data.ContainsKey(MapNodeKey.KeyType.Highway))
                {
                    continue;
                }

                List<CoordinatesWithPosition> waypoints = new List<CoordinatesWithPosition>();
                mapElement.References
                    .ForEach(reference => { waypoints.Add(WorldObjectData[reference].CoordinatesWithPosition); });

                List<Vector3> leftVerticePositions = new List<Vector3>();
                List<Vector3> rightVerticePositions = new List<Vector3>();
                float width = Road.GuessRoadWidth(mapElement.Data[MapNodeKey.KeyType.Highway]);

                //TODO: pad waypoint list to have a points at set intervals
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

                if (leftVerticePositions.Count > 1 && rightVerticePositions.Count > 1 && leftVerticePositions.Count == rightVerticePositions.Count)
                {
                    GameObject roadObject = Instantiate(roadPrefab, _roadParentObject.transform);
                    roadObject.name = string.IsNullOrWhiteSpace(mapElement.GetRoadName())
                        ? "Road"
                        : mapElement.GetRoadName();
                    Road roadScript = roadObject.GetComponent<Road>();
                    roadScript.Build(_terrainHeightService, mapElement, leftVerticePositions, rightVerticePositions);
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
}