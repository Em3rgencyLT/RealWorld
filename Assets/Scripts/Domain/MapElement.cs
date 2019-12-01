using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Utility;

namespace Domain {
    public class MapElement {

        private ID id;
        [CanBeNull] private Coordinates coordinates;
        private Dictionary<MapNodeKey.KeyType, String> data = new Dictionary<MapNodeKey.KeyType, string>();
        private List<MapElement.ID> references = new List<MapElement.ID>();
        private bool isBuilt = false;

        public ID Id { get { return this.id; } }
        public Coordinates Coordinates { get { return this.coordinates; } }
        public Dictionary<MapNodeKey.KeyType, String> Data { get { return this.data; } }
        [CanBeNull] public List<MapElement.ID> References { get { return this.references; } }
        public bool IsBuilt { get { return this.isBuilt; } }

        public void AddData (MapNodeKey.KeyType type, String value) {
            this.data.Add(type, value);
        }

        public void AddDatum (Dictionary<MapNodeKey.KeyType, String> datum) {
            this.data = this.data.Concat(datum).ToDictionary(x => x.Key, x => x.Value);
        }

        public MapElement (
            ID id, 
            [CanBeNull] Coordinates coordinates, 
            Dictionary<MapNodeKey.KeyType, String> data, 
            List<ID> references) {
            this.id = id;
            this.coordinates = coordinates;
            this.data = data;
            this.references = references;
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

        public void MarkBuilt() {
            if(this.isBuilt) {
                throw new Exception(this.id.Id.ToString() + " tried to mark as built when already built!");
            }

            this.isBuilt = true;
        }

        public struct ID {
            private long id;

            public long Id { get {return this.id; } }

            public ID(long id) {
                this.id = id;
            }
        }
    }
}