using System.Globalization;
using UnityEngine;
using Utility;

namespace Domain {
    [System.Serializable]
    public class Coordinates {
        [SerializeField]
        private double latitude;
        [SerializeField]
        private double longitude;
        [SerializeField]
        private Vector3 position;

        public double Latitude => latitude;
        public double Longitude => longitude;
        public Vector3 Position => position;

        private Coordinates(double latitude, double longitude, Vector3 position) {
            this.latitude = latitude;
            this.longitude = longitude;
            this.position = position;
        }

        public static Coordinates of (double latitude, double longitude) {
            ValidateLatLong(latitude, longitude);
               
            Vector3 position = CoordinateMath.CoordinatesToWorldPosition(latitude, longitude);
            return new Coordinates(latitude, longitude, position);
        }

        public static Coordinates projectionOriginOf(double latitude, double longitude)
        {
            ValidateLatLong(latitude, longitude);
            return new Coordinates(latitude, longitude, Vector3.zero);
        }

        private static void ValidateLatLong(double latitude, double longitude) {
            if(latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180) {
                throw new System.ArgumentOutOfRangeException("Coordinates out of range");
            }
        }

        public override string ToString()
        {
            return "Lat: " + latitude + " Long: " + longitude;
        }
    }
}
