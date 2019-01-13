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

        private Coordinates(double latitude, double longitude) {
            this.latitude = latitude;
            this.longitude = longitude;
        }
        public static Coordinates of (double latitude, double longitude) {
            //TODO: make it rollover instead of throw exception
            if(latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180) {
                throw new System.ArgumentOutOfRangeException("Coordinates out of range");
            }

            Coordinates coordinates = new Coordinates(latitude, longitude);
            Coordinates origin = MapObjectPlacementManager.Instance.GetWorldOrigin();
            if(origin is null) {
                coordinates.position = Vector3.zero;
            } else {
                coordinates.position = CoordinateMath.CoordinatesToWorldPosition(coordinates);
            }

            return coordinates;
        }

        public override string ToString()
        {
            return "Lat: " + latitude + " Long: " + longitude;
        }
    }
}
