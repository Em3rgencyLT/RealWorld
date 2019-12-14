using System.Collections.Generic;

namespace Domain
{
    public class WorldObjects
    {
        private List<MapObject> _mapObjects;

        public WorldObjects(List<MapObject> mapObjects)
        {
            _mapObjects = mapObjects;
        }

        public List<MapObject> MapObjects => _mapObjects;
    }
}