using UnityEngine;

namespace Domain {
    [System.Serializable]
    public class Coordinates : System.Object {
        [SerializeField]
        private double latitude;
        [SerializeField]
        private double longitude;
        [SerializeField]
        private Vector3 position;

        public double Latitude{ get {return latitude;} }
        public double Longitude{ get {return longitude;} }
        public Vector3 Position{ get {return position;} }

        private Coordinates(double latitude, double longitude, Vector3 position) {
            this.latitude = latitude;
            this.longitude = longitude;
            this.position = position;
        }

        public static Coordinates of (double latitude, double longitude) {
            ValidateLatLong(latitude, longitude);

            //Make sure we know what lat/long is at position (0,0,0) 
            MapObjectPlacementManager mopm = MapObjectPlacementManager.Instance;
            if(mopm.ProjectionOrigin is null) {
                mopm.ProjectionOrigin = new Coordinates(mopm.OriginLatitude, mopm.OriginLongitude, Vector3.zero);
            }

            Coordinates coordinates = new Coordinates(latitude, longitude, Vector3.zero);
            Vector3 position = CoordinateMath.CoordinatesToWorldPosition(coordinates);
            coordinates.position = position;

            return coordinates;
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
