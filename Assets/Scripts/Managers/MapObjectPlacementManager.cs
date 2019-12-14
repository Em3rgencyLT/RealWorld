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
        [SerializeField] private Material terrainMaterial;

        [SerializeField] private GameObject structurePrefab;
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private PlayerSpawner _playerSpawner;
        private GameObject _mapDataParentObject;
        private GameObject _terrainParentObject;
        private GameObject _playerObject;

        private CoordinatePositionService _coordinatePositionService;
        private SRTMDataService _srtmDataService;
        private OSMDataService _osmDataService;
        private HeightmapService _heightmapService;
        private OSMParserService _osmParserService;
        private PlayerChunkService _playerChunkService;

        private List<Chunk<Terrain>> _terrainChunks = new List<Chunk<Terrain>>();
        private List<Chunk<WorldObjects>> _mapObjectChunks = new List<Chunk<WorldObjects>>();

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
            var centerCoordinates = Coordinates.of(locationLatitude, locationLongitude);
            var playerPosition = _coordinatePositionService.GetCoordinatesWithPosition(centerCoordinates, 300).Position;
            _playerObject = _playerSpawner.SpawnPlayer(playerPosition);
            _playerChunkService = new PlayerChunkService(_playerObject);
            _terrainParentObject = new GameObject("Terrain");
            _mapDataParentObject = new GameObject("Map Data");
        }

        private void Update()
        {
            Vector2 playerChunkPosition = _playerChunkService.GetPlayerChunkCoordinates();

            //TODO: The code for these should be in their own service(s)
            UpdateTerrainChunks((int) playerChunkPosition.x, (int) playerChunkPosition.y);
            UpdateMapObjectChunks((int) playerChunkPosition.x, (int) playerChunkPosition.y);
        }

        private void UpdateTerrainChunks(int playerChunkX, int playerChunkY)
        {
            for (int i = playerChunkX - Parameters.TERRAIN_CHUNK_UNIT_RADIUS;
                i < playerChunkX + Parameters.TERRAIN_CHUNK_UNIT_RADIUS + 1;
                i++)
            {
                for (int j = playerChunkY - Parameters.TERRAIN_CHUNK_UNIT_RADIUS;
                    j < playerChunkY + Parameters.TERRAIN_CHUNK_UNIT_RADIUS + 1;
                    j++)
                {
                    if (_terrainChunks.Any(chunk => chunk.X == i && chunk.Y == j))
                    {
                        continue;
                    }
                    //FIXME: terrain chunk edges almost always have gaps betwen them

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
                i < playerChunkX + Parameters.MAP_CHUNK_UNIT_RADIUS + 1;
                i++)
            {
                for (int j = playerChunkY - Parameters.MAP_CHUNK_UNIT_RADIUS;
                    j < playerChunkY + Parameters.MAP_CHUNK_UNIT_RADIUS + 1;
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
                    var structureVertexData = StructureVertexHelper.GetStructuresWithVertices(parsedData);
                    var wayVertexData = WayVertexHelper.GetWaysWithVertices(parsedData, chunkBounds);
                    var mapObjects = new List<MapObject>();
                    var chunkParent = new GameObject($"WorldObjects X:{i} Y:{j}");
                    chunkParent.transform.parent = _mapDataParentObject.transform;
                    
                    //TODO: maybe generalize and combine structureVertexData and wayVertexData? The XML they read from is the same, and these foreach loops look awfully similar.
                    foreach (StructureWithVertices structureWithVertices in structureVertexData)
                    {
                        MapElement mapElement = structureWithVertices.MapElement;
                        GameObject structureObject = Instantiate(structurePrefab, chunkParent.transform);
                        structureObject.name = string.IsNullOrWhiteSpace(mapElement.GetAddress())
                            ? "Building"
                            : mapElement.GetAddress();
                        Structure structureScript = structureObject.GetComponent<Structure>();
                        structureScript.Build(heightService, mapElement, structureWithVertices.Vertices);
                        mapObjects.Add(structureScript);
                    }
                    foreach (WayWithVertices wayWithVertices in wayVertexData)
                    {
                        if (wayWithVertices.LeftVertices.Count <= 1 || wayWithVertices.RightVertices.Count <= 1 ||
                            wayWithVertices.LeftVertices.Count != wayWithVertices.RightVertices.Count)
                        {
                            continue;
                        }

                        //TODO: ways can also be rivers and fuck knows what else. Need support for all of that too.
                        GameObject wayObject = Instantiate(roadPrefab, chunkParent.transform);
                        MapElement mapElement = wayWithVertices.MapElement;
                        wayObject.name = string.IsNullOrWhiteSpace(mapElement.GetRoadName())
                                ? "Road"
                                : mapElement.GetRoadName();
                        Road roadScript = wayObject.GetComponent<Road>();
                        roadScript.Build(heightService, mapElement, wayWithVertices.LeftVertices, wayWithVertices.RightVertices);
                        mapObjects.Add(roadScript);
                    }
                    
                    var worldObjects = new WorldObjects(mapObjects);
                    var mapObjectChunk = new Chunk<WorldObjects>(i, j, worldObjects);
                    _mapObjectChunks.Add(mapObjectChunk);
                }
            }
        }
    }
}