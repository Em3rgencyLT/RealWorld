using UnityEngine;
using Utility;

namespace Domain {
    [System.Serializable]
    //FIXME: Coordinates should not contain a Vector3 position. This tuple should contain Coordinates and a Vector3 position.
    public class Elevation {
        [SerializeField]
        private Coordinates coordinates;
        [SerializeField]
        private double height;

        public Coordinates Coordinates { get {return coordinates;} }
        public double Height{ get {return height;} }

        public Elevation(Coordinates coordinates, double height) {
            this.coordinates = coordinates;
            this.height = height;
        }

        public override string ToString()
        {
            return "Lat: " + coordinates.Latitude + " Long: " + coordinates.Longitude + " Elevation: " + height;
        }
    }
}
