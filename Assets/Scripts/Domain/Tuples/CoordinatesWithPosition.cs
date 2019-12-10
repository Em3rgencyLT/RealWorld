using Services;
using UnityEngine;
using Utility;

namespace Domain {
    public class CoordinatesWithPosition
    {
        private CoordinatePositionService _coordinatePositionService;
        private Coordinates _coordinates;
        private Vector3 _position;

        public Coordinates Coordinates => _coordinates;
        public Vector3 Position => _position;

        public CoordinatesWithPosition(CoordinatePositionService coordinatePositionService, Coordinates coordinates, double height)
        {
            _coordinatePositionService = coordinatePositionService;
            _coordinates = coordinates;
            Vector3 lateralPosition = _coordinatePositionService.PositionFromCoordinates(coordinates);
            _position = new Vector3(lateralPosition.x, (float)height, lateralPosition.z);
        }

        public override string ToString()
        {
            return
                $"Latitude: {_coordinates.Latitude} Longitude: {_coordinates.Longitude}, X: {_position.x} Y: {_position.y} Z: {_position.z}";
        }
    }
}
