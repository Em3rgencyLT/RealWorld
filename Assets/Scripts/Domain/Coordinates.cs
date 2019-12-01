using UnityEngine;

namespace Domain {
    [System.Serializable]
    public class Coordinates {
        [SerializeField]
        private double latitude;
        [SerializeField]
        private double longitude;

        public double Latitude => latitude;
        public double Longitude => longitude;

        private Coordinates(double latitude, double longitude) {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public static Coordinates of (double latitude, double longitude) {
            if(latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180) {
                throw new System.ArgumentOutOfRangeException("Coordinates out of range");
            }
            
            return new Coordinates(latitude, longitude);
        }

        public override string ToString()
        {
            return "Lat: " + latitude + " Long: " + longitude;
        }
    }
}
