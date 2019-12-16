using System;
using Domain;
using Domain.Tuples;
using UnityEngine;
using Utility;

namespace Services
{
    public class TerrainBuilder
    {
        //FIXME: should have a fancy multi-texture terrain thingy
        private Material _material;
        private int _baseMapResolution = 1024;
        private int _detailResolution = 1024;
        private int _resolutionPerPatch = 32;

        private ConfigurationService _config;

        public TerrainBuilder(Material material)
        {
            _material = material;
            _config = new ConfigurationService(FolderPaths.ConfigFile);
        }

        public TerrainBuilder BaseMapResolution(int baseMapResolution)
        {
            _baseMapResolution = baseMapResolution;
            return this;
        }

        public TerrainBuilder DetailResolution(int detailResolution)
        {
            _detailResolution = detailResolution;
            return this;
        }

        public TerrainBuilder ResolutionPerPatch(int resolutionPerPatch)
        {
            _resolutionPerPatch = resolutionPerPatch;
            return this;
        }

        public Terrain Build(string name, float[,] heightmap, Vector3 position)
        {
            var terrainObject = new GameObject(name);
            terrainObject.transform.position = position;
            terrainObject.layer = _config.GetInt(ConfigurationKeyInt.TERRAIN_LAYER);

            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = heightmap.GetLength(0);
            terrainData.baseMapResolution = _baseMapResolution;
            terrainData.SetDetailResolution(_detailResolution, _resolutionPerPatch);
            terrainData.size =
                new Vector3(_config.GetInt(ConfigurationKeyInt.CHUNK_SIZE_METERS), 8848f, _config.GetInt(ConfigurationKeyInt.CHUNK_SIZE_METERS));
            terrainData.SetHeights(0, 0, heightmap);

            TerrainCollider terrainCollider = terrainObject.AddComponent<TerrainCollider>();
            Terrain terrain = terrainObject.AddComponent<Terrain>();
            terrain.allowAutoConnect = true;
            terrain.materialTemplate = _material;

            terrainCollider.terrainData = terrainData;
            terrain.terrainData = terrainData;
            terrainObject.isStatic = true;

            return terrain;
        }
    }
}