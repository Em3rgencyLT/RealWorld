using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Utility;

namespace Domain {
    public class MapElement {

        private ID _id;
        [CanBeNull] private CoordinatesWithPosition _coordinatesWithPosition;
        private Dictionary<MapNodeKey.KeyType, String> _data;
        private List<ID> _references;
        private bool _isBuilt;

        public ID Id => _id;
        public CoordinatesWithPosition CoordinatesWithPosition => _coordinatesWithPosition;
        public Dictionary<MapNodeKey.KeyType, String> Data => _data;
        [CanBeNull] public List<ID> References => _references;

        public MapElement (
            ID id, 
            [CanBeNull] CoordinatesWithPosition coordinatesWithPosition, 
            Dictionary<MapNodeKey.KeyType, String> data, 
            List<ID> references) {
            _id = id;
            _coordinatesWithPosition = coordinatesWithPosition;
            _data = data;
            _references = references;
        }

        public string GetAddress() {
            string street = Data.ContainsKey(MapNodeKey.KeyType.Addr_Street) ? Data[MapNodeKey.KeyType.Addr_Street] : "";
            string houseNumber = Data.ContainsKey(MapNodeKey.KeyType.Addr_Housenumber) ? Data[MapNodeKey.KeyType.Addr_Housenumber] : "";

            return street + " " + houseNumber;
        }

        public string GetRoadName() {
            string type = Data.ContainsKey(MapNodeKey.KeyType.Highway) ? Data[MapNodeKey.KeyType.Highway] : "";
            string name = Data.ContainsKey(MapNodeKey.KeyType.Name) ? Data[MapNodeKey.KeyType.Name] : "";

            return type.Capitalise() + (name.Length > 0 ? " " + name : "");
        }

        public struct ID {
            private long _id;

            public long Id => _id;

            public ID(long id) {
                _id = id;
            }
        }
    }
}