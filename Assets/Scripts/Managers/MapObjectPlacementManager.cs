using System;
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
        [SerializeField] private Material terrainMaterial = null;

        [SerializeField] private GameObject structurePrefab = null;
        [SerializeField] private GameObject roadPrefab = null;
        [SerializeField] private PlayerSpawner _playerSpawner = null;
        private GameObject _mapDataParentObject;
        private GameObject _terrainParentObject;
        private GameObject _playerObject;

        private ConfigurationService _configurationService;
        private CoordinatePositionService _coordinatePositionService;
        private SRTMDataService _srtmDataService;
        private OSMDataService _osmDataService;
        private HeightmapService _heightmapService;
        private OSMParserService _osmParserService;
        private PlayerChunkService _playerChunkService;
        private ChunkUpdateBufferService _chunkUpdateBufferService;

        private List<Chunk> _chunks = new List<Chunk>();

        private void Awake()
        {
            //Make sure doubles accept . for decimal instead of ,
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var worldCenter = Coordinates.of(locationLatitude, locationLongitude);

            _configurationService = new ConfigurationService(FolderPaths.ConfigFile);
            _coordinatePositionService = new CoordinatePositionService(worldCenter);
            _srtmDataService = new SRTMDataService();
            _osmDataService =
                new OSMDataService(_configurationService.GetString(ConfigurationKeyString.OSM_DATA_API_URL));
            _heightmapService = new HeightmapService(_srtmDataService);
            _osmParserService = new OSMParserService(_srtmDataService, _coordinatePositionService);
            _chunkUpdateBufferService = new ChunkUpdateBufferService();

            if (structurePrefab == null)
            {
                throw new ArgumentException(
                    $"Structure Prefab cannot be null! Please set in inspector of {name}!");
            }

            if (roadPrefab == null)
            {
                throw new ArgumentException(
                    $"Road Prefab cannot be null! Please set in inspector of {name}!");
            }

            if (_playerSpawner == null)
            {
                throw new ArgumentException(
                    $"Player Spawner cannot be null! Please set in inspector of {name}!");
            }
        }

        void Start()
        {
            var centerCoordinates = Coordinates.of(locationLatitude, locationLongitude);
            var playerPosition = _coordinatePositionService.GetCoordinatesWithPosition(centerCoordinates, 300).Position;
            _playerObject = _playerSpawner.SpawnPlayer(playerPosition);
            _playerChunkService = new PlayerChunkService(_playerObject,
                _configurationService.GetInt(ConfigurationKeyInt.CHUNK_SIZE_METERS),
                _configurationService.GetInt(ConfigurationKeyInt.CHUNK_UNIT_RADIUS));
            _terrainParentObject = new GameObject("Terrain");
            _mapDataParentObject = new GameObject("Map Data");
        }

        private void Update()
        {
            ProcessChunkBuffer();
        }

        private void ProcessChunkBuffer()
        {
            _chunkUpdateBufferService.AddToBuffer(_playerChunkService.GetChunkUpdates(_chunks));

            var chunk = _chunkUpdateBufferService.PopNext();
            while (chunk != null)
            {
                switch (chunk.EventType)
                {
                    case ChunkUpdate.Type.CREATE:
                        CreateChunk(chunk.Location);
                        break;
                    case ChunkUpdate.Type.DELETE:
                        DeleteChunk(chunk.Location);
                        break;
                    default:
                        throw new Exception("Something has gone horribly wrong.");
                }
                chunk = _chunkUpdateBufferService.PopNext();
            }
        }

        private void CreateChunk(Int2 location)
        {
            Bounds<Vector3> chunkBounds = ChunkHelper.GetChunkBounds(location.X, location.Y,
                _configurationService.GetInt(ConfigurationKeyInt.CHUNK_SIZE_METERS));
            Coordinates minCoordinates = _coordinatePositionService.CoordinatesFromPosition(chunkBounds.MinPoint);
            Coordinates maxCoordinates = _coordinatePositionService.CoordinatesFromPosition(chunkBounds.MaxPoint);
            Bounds<Coordinates> areaBounds = Bounds<Coordinates>.of(minCoordinates, maxCoordinates);

            var heightmap = _heightmapService.GetHeightmapMatrix(areaBounds);
            var osmData = _osmDataService.GetDataForArea(areaBounds);
            var parsedData = _osmParserService.Parse(osmData);
            var structureVertexData = StructureVertexHelper.GetStructuresWithVertices(parsedData);
            var wayVertexData = WayVertexHelper.GetWaysWithVertices(parsedData, chunkBounds);
            
            var terrain = TerrainInstantiator.InstantiateTerrain(heightmap, location, chunkBounds,
                terrainMaterial, _terrainParentObject, _chunks);
            var heightService = new TerrainHeightService(terrain);
            WorldObjects worldObjects = WorldObjectsInstantiator.InstantiateWorldObjects(structureVertexData,
                wayVertexData, location, structurePrefab, roadPrefab, heightService, _mapDataParentObject);

            var chunk = new Chunk(location, terrain, worldObjects);
            _chunks.Add(chunk);
        }

        private void DeleteChunk(Int2 location)
        {
            var oldChunk = _chunks
                .First(chunk => chunk.Location.X == location.X && chunk.Location.Y == location.Y);
            
            Destroy(oldChunk.Terrain.gameObject);
            if (oldChunk.WorldObjects.MapObjects.Count > 0)
            {
                Destroy(oldChunk.WorldObjects.MapObjects[0].transform.parent.gameObject);
            }

            _chunks.Remove(oldChunk);
        }
    }
}