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

        private Bounds<Coordinates> _mapDataBounds;
        private Bounds<Coordinates> _terrainBounds;
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
        private PlayerChunkService _playerChunkService;

        private List<Chunk<Terrain>> _terrainChunks = new List<Chunk<Terrain>>();
        private List<Chunk<List<MapObject>>> _mapObjectChunks = new List<Chunk<List<MapObject>>>();

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
            _playerChunkService = new PlayerChunkService(_playerObject);
            _terrainParentObject = new GameObject("Terrain");
            _structureParentObject = new GameObject("Structures");
            _roadParentObject = new GameObject("Roads");
        }

        private void Update()
        {
            Vector2 playerChunkPosition = _playerChunkService.GetPlayerChunkCoordinates();

            UpdateTerrainChunks((int) playerChunkPosition.x, (int) playerChunkPosition.y);
            UpdateMapObjectChunks((int) playerChunkPosition.x, (int) playerChunkPosition.y);
        }

        private void SetupAreas()
        {
            var offset = worldSize * Parameters.WORLD_SIZE_MULTIPLIER;
            MapObjectOrigin = Coordinates.of(locationLatitude - offset / 2, locationLongitude - offset / 2);

            Coordinates mapDataBottomCoordinates = MapObjectOrigin;
            Coordinates mapDataTopCoordinates =
                Coordinates.of(MapObjectOrigin.Latitude + offset, MapObjectOrigin.Longitude + offset);
            _mapDataBounds = Bounds<Coordinates>.of(mapDataBottomCoordinates, mapDataTopCoordinates);

            Coordinates terrainBottomCoordinates = _coordinatePositionService.WorldCenter;
            Coordinates terrainTopCoordinates =
                Coordinates.of(terrainBottomCoordinates.Latitude + offset * 2, terrainBottomCoordinates.Longitude + offset * 2);
            _terrainBounds = Bounds<Coordinates>.of(terrainBottomCoordinates, terrainTopCoordinates);
        }

        private void UpdateTerrainChunks(int playerChunkX, int playerChunkY)
        {
            for (int i = playerChunkX - Parameters.TERRAIN_CHUNK_UNIT_RADIUS;
                i < playerChunkX + Parameters.TERRAIN_CHUNK_UNIT_RADIUS;
                i++)
            {
                for (int j = playerChunkY - Parameters.TERRAIN_CHUNK_UNIT_RADIUS;
                    j < playerChunkY + Parameters.TERRAIN_CHUNK_UNIT_RADIUS;
                    j++)
                {
                    if (_terrainChunks.Any(chunk => chunk.X == i && chunk.Y == j))
                    {
                        continue;
                    }

                    Bounds<Vector3> chunkBounds = ChunkHelper.GetChunkBounds(i, j);
                    Coordinates minCoordinates = _coordinatePositionService.CoordinatesFromPosition(chunkBounds.MinPoint);
                    Coordinates maxCoordinates = _coordinatePositionService.CoordinatesFromPosition(chunkBounds.MaxPoint);
                    Bounds<Coordinates> terrainBounds = Bounds<Coordinates>.of(minCoordinates, maxCoordinates);

                    var heightmap = _heightmapService.GetHeightmapMatrix(terrainBounds);
                    var terrain = new TerrainBuilder(terrainMaterial).Build($"Terrain X:{i} Y:{j}" ,heightmap, chunkBounds.MinPoint);

                    terrain.transform.parent = _terrainParentObject.transform;
                    var terrainChunk = new Chunk<Terrain>(i, j, terrain);
                    _terrainChunks.Add(terrainChunk);
                }
            }
        }

        private void UpdateMapObjectChunks(int playerChunkX, int playerChunkY)
        {
            for (int i = playerChunkX - Parameters.MAP_CHUNK_UNIT_RADIUS;
                i < playerChunkX + Parameters.MAP_CHUNK_UNIT_RADIUS;
                i++)
            {
                for (int j = playerChunkY - Parameters.MAP_CHUNK_UNIT_RADIUS;
                    j < playerChunkY + Parameters.MAP_CHUNK_UNIT_RADIUS;
                    j++)
                {
                    if (_mapObjectChunks.Any(chunk => chunk.X == i && chunk.Y == j))
                    {
                        continue;
                    }

                    Terrain terrain = _terrainChunks.Find(chunk => chunk.X == i && chunk.Y == j).Data;
                    if (terrain == null)
                    {
                        throw new Exception($"Could not find terrain for chunk X:{i} Y:{j}");
                    }
                    
                    var heightService = new TerrainHeightService(terrain);
                    
                    Bounds<Vector3> chunkBounds = ChunkHelper.GetChunkBounds(i, j);
                    Coordinates minCoordinates = _coordinatePositionService.CoordinatesFromPosition(chunkBounds.MinPoint);
                    Coordinates maxCoordinates = _coordinatePositionService.CoordinatesFromPosition(chunkBounds.MaxPoint);
                    Bounds<Coordinates> mapDataBounds = Bounds<Coordinates>.of(minCoordinates, maxCoordinates);

                    var osmData = _osmDataService.GetDataForArea(mapDataBounds);
                    var parsedData = _osmParserService.Parse(osmData);
                    var vertexData = StructureVertexHelper.GetStructuresWithVertices(parsedData);
                    var structures = new List<MapObject>();
                    foreach (StructureWithVertices structureWithVertices in vertexData)
                    {
                        MapElement mapElement = structureWithVertices.MapElement;
                        GameObject structureObject = Instantiate(structurePrefab, _structureParentObject.transform);
                        structureObject.name = string.IsNullOrWhiteSpace(mapElement.GetAddress())
                            ? "Building"
                            : mapElement.GetAddress();
                        Structure structureScript = structureObject.GetComponent<Structure>();
                        structureScript.Build(heightService, mapElement, structureWithVertices.Vertices);
                        structures.Add(structureScript);
                    }
                    
                    var mapObjectChunk = new Chunk<List<MapObject>>(i, j, structures);
                    _mapObjectChunks.Add(mapObjectChunk);
                }
            }
        }

        private IEnumerator PlaceRoads()
        {
            int built = 0;
            
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
                    //roadScript.Build(_terrainHeightService, mapElement, leftVerticePositions, rightVerticePositions);
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