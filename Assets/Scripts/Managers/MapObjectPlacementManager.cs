using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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

        private List<PreparedChunk> _preparedChunks = new List<PreparedChunk>();
        private List<Chunk> _chunks = new List<Chunk>();
        
        private void Awake()
        {
            //Make sure doubles accept . for decimal instead of ,
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var worldCenter = Coordinates.of(locationLatitude, locationLongitude);

            _configurationService = new ConfigurationService(FolderPaths.ConfigFile);
            _coordinatePositionService = new CoordinatePositionService(worldCenter);
            string username = _configurationService.GetString(ConfigurationKeyString.NASA_SRTM_USERNAME);
            string password = _configurationService.GetString(ConfigurationKeyString.NASA_SRTM_PASSWORD);
            _srtmDataService = new SRTMDataService(username, password);
            string osmAPI = _configurationService.GetString(ConfigurationKeyString.OSM_DATA_API_URL);
            _osmDataService = new OSMDataService(osmAPI);
            int highestElevation = _configurationService.GetInt(ConfigurationKeyInt.HIGHEST_ELEVATION_ON_EARTH);
            _heightmapService = new HeightmapService(_srtmDataService, highestElevation);
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
                        PrepareChunk(chunk.Location);
                        break;
                    case ChunkUpdate.Type.DELETE:
                        DeleteChunk(chunk.Location);
                        break;
                    default:
                        throw new Exception("Something has gone horribly wrong.");
                }
                chunk = _chunkUpdateBufferService.PopNext();
            }

            if (_preparedChunks.Count > 0)
            {
                var preparedChunk = _preparedChunks[0];
                CreateChunk(preparedChunk);
                _preparedChunks.Remove(preparedChunk);
            }
        }

        private void PrepareChunk(Int2 location)
        {
            int chunkSizeMeters = _configurationService.GetInt(ConfigurationKeyInt.CHUNK_SIZE_METERS);
            Bounds<Vector3> chunkBounds = ChunkHelper.GetChunkBounds(location.X, location.Y, chunkSizeMeters);
            AsyncChunkRequest asyncChunkRequest = new AsyncChunkRequest(chunkBounds, location);
            ThreadPool.QueueUserWorkItem(AsyncCalculateChunkData, asyncChunkRequest);
        }

        private void AsyncCalculateChunkData(object state)
        {
            Bounds<Vector3> chunkBounds = ((AsyncChunkRequest) state).ChunkBounds;
            Int2 location = ((AsyncChunkRequest) state).Location;
            
            Coordinates minCoordinates = _coordinatePositionService.CoordinatesFromPosition(chunkBounds.MinPoint);
            Coordinates maxCoordinates = _coordinatePositionService.CoordinatesFromPosition(chunkBounds.MaxPoint);
            Bounds<Coordinates> areaBounds = Bounds<Coordinates>.of(minCoordinates, maxCoordinates);
            var heightmap = _heightmapService.GetHeightmapMatrix(areaBounds);
            var osmData = _osmDataService.GetDataForArea(areaBounds);
            var parsedData = _osmParserService.Parse(osmData);
            var structureVertexData = StructureVertexHelper.GetStructuresWithVertices(parsedData);
            var wayVertexData = WayVertexHelper.GetWaysWithVertices(parsedData, chunkBounds);

            var preparedChunk = new PreparedChunk(heightmap, chunkBounds, location, structureVertexData, wayVertexData);
            _preparedChunks.Add(preparedChunk);
        }

        private void CreateChunk(PreparedChunk preparedChunk)
        {
            var terrain = TerrainInstantiator.InstantiateTerrain(preparedChunk.Heightmap, preparedChunk.Location, preparedChunk.ChunkBounds,
                terrainMaterial, _terrainParentObject, _chunks);
            var heightService = new TerrainHeightService(terrain);
            WorldObjects worldObjects = WorldObjectsInstantiator.InstantiateWorldObjects(preparedChunk.StructureVertexData,
                preparedChunk.WayVertexData, preparedChunk.Location, structurePrefab, roadPrefab, heightService, _mapDataParentObject);
            var chunk = new Chunk(preparedChunk.Location, terrain, worldObjects);
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