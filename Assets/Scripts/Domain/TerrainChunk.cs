using UnityEngine;

namespace Domain
{
    public class TerrainChunk
    {
        private int _x;
        private int _y;
        private Terrain _terrain;

        public TerrainChunk(int x, int y, Terrain terrain)
        {
            _x = x;
            _y = y;
            _terrain = terrain;
        }

        public int X => _x;

        public int Y => _y;

        public Terrain Terrain => _terrain;
    }
}