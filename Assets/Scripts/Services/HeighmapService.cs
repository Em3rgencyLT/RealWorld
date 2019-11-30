using System;
using Domain;
using UnityEngine;

namespace Services
{
    public class HeighmapService
    {
        private int _worldSize;

        public HeighmapService(int worldSize)
        {
            _worldSize = worldSize;
        }

        public int GetHeightmapResolution()
        {
            //Unity Terrain constraint. Heightmap must be a power of 2, plus 1.
            return (int) Math.Pow(2, Parameters.HEIGHTMAP_RESOLUTION_BASE_POWER + _worldSize) + 1;
        }

        /**
         * Heightmaps are expected to be a 2D array of values from 0 to 1.
         */
        public float[,] GetHeightmapMatrix(Vector3[,] data)
        {
            var size = GetHeightmapResolution();
            var heightmap = new float[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    heightmap[i, j] = data[i, j].y / 8848f;
                }
            }

            return heightmap;
        }
    }
}