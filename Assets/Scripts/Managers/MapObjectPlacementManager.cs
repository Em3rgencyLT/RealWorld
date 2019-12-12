using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private GameObject _terrainParentObject;
        private GameObject _playerObject;

        private CoordinatePositionService _coordinatePositionService;
        private SRTMDataService _srtmDataService;
        private OSMDataService _osmDataService;
        private HeightmapService _heightmapService;
        private OSMParserService _osmParserService;
        private TerrainHeightService _terrainHeightService;

        private List<TerrainChunk> _terrainChunks = new List<TerrainChunk>();

        public Coordinates MapObjectOrigin { get; private set; }

        public Dictionary<MapElement.ID, MapElement> WorldObjectData { get; private set; }

        private void Awake()
        {
            //Make sure doubles accept . for decimal instead of ,
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            
            var worldCenter = Coordinates.of(locationLatitude, locationLongitude);
        
            _coordinatePositionService = new CoordinatePositionService(worldCenter);
            _srtmDataService = new SRTMDataService();
            _osmDataService = new OSMDataService();
            _heightmapService = new HeightmapService(_srtmDataService);
            _osmParserService = new OSMParserService(_srtmDataService, _coordinatePositionService);
        
            SetupAreas();

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
            //BuildWorld();
            var centerCoordinates = Coordinates.of(locationLatitude, locationLongitude);
            var playerPosition = _coordinatePositionService.GetCoordinatesWithPosition(centerCoordinates, 300).Position;
            _playerObject = _playerSpawner.SpawnPlayer(playerPosition);
            _terrainParentObject = new GameObject("Terrain");
        }

        private void Update()
        {
            Vector3 playerPosition = _playerObject.transform.position;
            int playerChunkPositionX = (int)Mathf.Floor(playerPosition.x / Parameters.CHUNK_SIZE);
            int playerChunkPositionY = (int)Mathf.Floor(playerPosition.z / Parameters.CHUNK_SIZE);

            for (int i = playerChunkPositionX - Parameters.TERRAIN_CHUNK_DISTANCE;
                i < playerChunkPositionX + Parameters.TERRAIN_CHUNK_DISTANCE;
                i++)
            {
                for (int j = playerChunkPositionY - Parameters.TERRAIN_CHUNK_DISTANCE;
                    j < playerChunkPositionY + Parameters.TERRAIN_CHUNK_DISTANCE;
                    j++)
                {
                    if (_terrainChunks.Any(chunk => chunk.X == i && chunk.Y == j))
                    {
                        continue;
                    }

                    int chunkXmin = i * Parameters.CHUNK_SIZE - Parameters.CHUNK_SIZE / 2;
                    int chunkYmin = j * Parameters.CHUNK_SIZE - Parameters.CHUNK_SIZE / 2;
                    int chunkXmax = i * Parameters.CHUNK_SIZE + Parameters.CHUNK_SIZE / 2;
                    int chunkYmax = j * Parameters.CHUNK_SIZE + Parameters.CHUNK_SIZE / 2;
                    
                    Vector3 minPoint = new Vector3(chunkXmin, 0f, chunkYmin);
                    Vector3 maxPoint = new Vector3(chunkXmax, 0f, chunkYmax);

                    Coordinates minCoordinates = _coordinatePositionService.CoordinatesFromPosition(minPoint);
                    Coordinates maxCoordinates = _coordinatePositionService.CoordinatesFromPosition(maxPoint);
                    
                    AreaBounds<Coordinates> terrainAreaBounds = AreaBounds<Coordinates>.of(minCoordinates, maxCoordinates);

                    var heightmap = _heightmapService.GetHeightmapMatrix(terrainAreaBounds);
                    var terrain = new TerrainBuilder(terrainMaterial).Build($"Terrain X:{i} Y:{j}" ,heightmap, Vector3.Lerp(minPoint, maxPoint, 0.5f));

                    terrain.transform.parent = _terrainParentObject.transform;
                    var terrainChunk = new TerrainChunk(i, j, terrain);
                    _terrainChunks.Add(terrainChunk);
                }
            }
        }

        private void SetupAreas()
        {
            var offset = worldSize * Parameters.WORLD_SIZE_MULTIPLIER;
            MapObjectOrigin = Coordinates.of(locationLatitude - offset / 2, locationLongitude - offset / 2);

            Coordinates mapDataBottomCoordinates = MapObjectOrigin;
            Coordinates mapDataTopCoordinates =
                Coordinates.of(MapObjectOrigin.Latitude + offset, MapObjectOrigin.Longitude + offset);
            _mapDataAreaBounds = AreaBounds<Coordinates>.of(mapDataBottomCoordinates, mapDataTopCoordinates);

            Coordinates terrainBottomCoordinates = _coordinatePositionService.WorldCenter;
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

            //Terrain terrain = new TerrainBuilder(heightmap, terrainTopPoint, terrainMaterial).Build();
            //_terrainHeightService = new TerrainHeightService(terrain); //FIXME: should be called in awake, or not be globally available
            //StartCoroutine("PlaceBuildings");
            //StartCoroutine("PlaceRoads");
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