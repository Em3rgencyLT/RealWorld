using System;
using UnityEngine;

namespace Domain {
    public class Coordinates {
        public double Latitude { get; }

        public double Longitude { get; }

        private Coordinates(double latitude, double longitude) {
            Latitude = latitude;
            Longitude = longitude;
        }

        public static Coordinates of (double latitude, double longitude) {
            if(latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180) {
                throw new System.ArgumentOutOfRangeException("Coordinates out of range");
            }
            
            return new Coordinates(latitude, longitude);
        }

        public static Coordinates lerp(Coordinates origin, Coordinates target, float size)
        {
            if (size > 1 || size < 0)
            {
                throw new ArgumentException("Parameter 'size' must be between 0 and 1 in Coordinates#lerp!");
            }
            
            double deltaLat = target.Latitude - origin.Latitude;
            double deltaLong = target.Longitude - origin.Longitude;

            double newLat = origin.Latitude + deltaLat * size;
            double newLong = origin.Longitude + deltaLong * size;

            return of(newLat, newLong);
        }

        public override string ToString()
        {
            return "Lat: " + Latitude + " Long: " + Longitude;
        }
    }
}
