using UnityEngine;
using Utility;

namespace Domain {
    [System.Serializable]
    public class CoordinatesWithPosition {
        [SerializeField]
        private Coordinates _coordinates;
        [SerializeField]
        private Vector3 _position;

        public Coordinates Coordinates => _coordinates;
        public Vector3 Position => _position;

        public CoordinatesWithPosition(Coordinates coordinates, double height) {
            _coordinates = coordinates;
            Vector3 lateralPosition = CoordinateMath.CoordinatesToWorldPosition(coordinates.Latitude, coordinates.Longitude);
            _position = new Vector3(lateralPosition.x, (float)height, lateralPosition.z);
        }

        public override string ToString()
        {
            return
                $"Latitude: {_coordinates.Latitude} Longitude: {_coordinates.Longitude}, X: {_position.x} Y: {_position.y} Z: {_position.z}";
        }
    }
}
