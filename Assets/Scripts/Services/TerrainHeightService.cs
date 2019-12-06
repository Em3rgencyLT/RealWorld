using UnityEngine;

namespace Services
{
    public class TerrainHeightService
    {
        private Terrain _terrain;

        public TerrainHeightService(Terrain terrain)
        {
            _terrain = terrain;
        }

        public float GetHeightForPoint(Vector3 point)
        {
            return _terrain.SampleHeight(point);
        }
    }
}