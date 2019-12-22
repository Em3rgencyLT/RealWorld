using UnityEngine;

namespace Domain
{
    public class Chunk
    {
        private Int2 _location;
        private Terrain _terrain;
        private WorldObjects _worldObjects;

        public Chunk(Int2 location, Terrain terrain, WorldObjects worldObjects)
        {
            _location = location;
            _terrain = terrain;
            _worldObjects = worldObjects;
        }

        public Int2 Location => _location;

        public Terrain Terrain => _terrain;

        public WorldObjects WorldObjects => _worldObjects;
    }
}