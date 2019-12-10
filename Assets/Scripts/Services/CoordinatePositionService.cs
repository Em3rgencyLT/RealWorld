﻿using System;
using Domain;
using UnityEngine;

namespace Services {
    public class CoordinatePositionService
    {
        private const double EARTH_RADIUS_KILOMETERS = 6378.137;

        private Coordinates _projectionOrigin;

        public Coordinates ProjectionOrigin => _projectionOrigin;

        public CoordinatePositionService(Coordinates projectionOrigin)
        {
            _projectionOrigin = projectionOrigin;
        }

        public CoordinatesWithPosition GetCoordinatesWithPosition(Coordinates coordinates, double height)
        {
            return new CoordinatesWithPosition(this, coordinates, height);
        }

        public Vector3 PositionFromCoordinates(Coordinates coordinates) {
            double x = (coordinates.Longitude - _projectionOrigin.Longitude) * (ToRadian(EARTH_RADIUS_KILOMETERS) * Math.Cos(ToRadian(coordinates.Latitude))) * 1000;
            double y = (coordinates.Latitude - _projectionOrigin.Latitude) * ToRadian(EARTH_RADIUS_KILOMETERS) * 1000;

            return new Vector3((float)x, 0f, (float)y);
        }

        public Coordinates CoordinatesFromPosition(Vector3 position)
        {
            double latitude = position.z / (ToRadian(EARTH_RADIUS_KILOMETERS) * 1000) + _projectionOrigin.Latitude;
            double longitude = position.x / (ToRadian(EARTH_RADIUS_KILOMETERS) * Math.Cos(ToRadian(latitude)) * 1000) +
                               _projectionOrigin.Longitude;
            
            return Coordinates.of(latitude, longitude);
        }
        
        private static double ToRadian(double degrees)
        {
            return Math.PI / 180 * degrees;
        }
    }
}
