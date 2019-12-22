using System.Collections.Generic;
using Domain;
using Domain.Tuples;
using Services;
using UnityEngine;

namespace Utility
{
    public static class TerrainInstantiator
    {
        public static Terrain InstantiateTerrain(float[,] heightmap, Int2 location, Bounds<Vector3> chunkBounds, Material terrainMaterial, GameObject _terrainParentObject, List<Chunk> chunks)
        {
            var terrain = new TerrainBuilder(terrainMaterial).Build($"Terrain X:{location.X} Y:{location.Y}" ,heightmap, chunkBounds.MinPoint);
            terrain.transform.parent = _terrainParentObject.transform;

            //TODO: maybe move this into the Chunk class on creation?
            var northNeighbour = chunks.Find(chunk => chunk.Location.X == location.X && chunk.Location.Y == location.Y + 1);
            var eastNeighbour = chunks.Find(chunk => chunk.Location.X == location.X + 1 && chunk.Location.Y == location.Y);
            var southNeighbour = chunks.Find(chunk => chunk.Location.X == location.X && chunk.Location.Y == location.Y - 1);
            var westNeighbour = chunks.Find(chunk => chunk.Location.X == location.X - 1 && chunk.Location.Y == location.Y);
            if (northNeighbour != null)
            {
                terrain = TerrainWeldingHelper.Weld(terrain,northNeighbour.Terrain, TerrainWeldingHelper.Direction.NORTH);
            }

            if (eastNeighbour != null)
            {
                terrain = TerrainWeldingHelper.Weld(terrain,eastNeighbour.Terrain, TerrainWeldingHelper.Direction.EAST);
            }
                    
            if (southNeighbour != null)
            {
                terrain = TerrainWeldingHelper.Weld(terrain,southNeighbour.Terrain, TerrainWeldingHelper.Direction.SOUTH);
            }
                    
            if (westNeighbour != null)
            {
                terrain = TerrainWeldingHelper.Weld(terrain,westNeighbour.Terrain, TerrainWeldingHelper.Direction.WEST);
            }

            return terrain;
        } 
    }
}