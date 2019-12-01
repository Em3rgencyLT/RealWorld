using System.Xml.Linq;
using Domain;
using Domain.Tuples;
using UnityEngine;
using Utility;

namespace Services
{
    public class OSMDataService
    {
        public OSMDataService() {}
        
        public XElement GetDataForArea(CoordinateBox coordinateBox) {
            Debug.Log(
                $"Requesting map object data from {coordinateBox.BottomCoordinates} to {coordinateBox.TopCoordinates}.");
            string url = Parameters.OSM_DATA_API_URL + 
                         coordinateBox.BottomCoordinates.Longitude + "," + 
                         coordinateBox.BottomCoordinates.Latitude + "," +
                         coordinateBox.TopCoordinates.Longitude + "," +
                         coordinateBox.TopCoordinates.Latitude;

            string xml = HttpRequest.Get(url);
            return XElement.Parse(xml);
        }
    }
}