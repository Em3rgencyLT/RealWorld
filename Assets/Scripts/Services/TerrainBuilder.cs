using System;
using Domain;
using Domain.Tuples;
using UnityEngine;

namespace Services
{
    public class TerrainBuilder
    {
        private Vector3 _terrainAreaTopPoint;
        private float[,] _heightmap;

        //FIXME: should have a fancy multi-texture terrain thingy
        private Material _material;
        private int _baseMapResolution = 1024;
        private int _detailResolution = 1024;
        private int _resolutionPerPatch = 32;

        public TerrainBuilder(float[,] heightmap, Vector3 terrainAreaTopPoint, Material material)
        {
            _heightmap = heightmap;
            _terrainAreaTopPoint = terrainAreaTopPoint;
            _material = material;
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

        public Terrain Build()
        {
            var terrainObject = new GameObject("Terrain");
            terrainObject.layer = Parameters.TERRAIN_LAYER;

            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = _heightmap.GetLength(0);
            terrainData.baseMapResolution = _baseMapResolution;
            terrainData.SetDetailResolution(_detailResolution, _resolutionPerPatch);
            terrainData.size =
                new Vector3(_terrainAreaTopPoint.x, 8848f, _terrainAreaTopPoint.z);
            terrainData.SetHeights(0, 0, _heightmap);

            TerrainCollider terrainCollider = terrainObject.AddComponent<TerrainCollider>();
            Terrain terrain = terrainObject.AddComponent<Terrain>();
            terrain.materialTemplate = _material;

            terrainCollider.terrainData = terrainData;
            terrain.terrainData = terrainData;
            terrainObject.isStatic = true;

            return terrain;
        }
    }
}