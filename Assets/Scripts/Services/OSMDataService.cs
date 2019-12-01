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
        
        public XElement GetDataForArea(AreaBounds<Coordinates> areaBounds) {
            Debug.Log(
                $"Requesting map object data from {areaBounds.BottomPoint} to {areaBounds.TopPoint}.");
            string url = Parameters.OSM_DATA_API_URL + 
                         areaBounds.BottomPoint.Longitude + "," + 
                         areaBounds.BottomPoint.Latitude + "," +
                         areaBounds.TopPoint.Longitude + "," +
                         areaBounds.TopPoint.Latitude;

            string xml = HttpRequest.Get(url);
            return XElement.Parse(xml);
        }
    }
}