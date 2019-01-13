using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;
using Domain;

public class MapData
{
    private const string MAP_DATA_API_URL = @"https://overpass-api.de/api/map?bbox=";

    public static Dictionary<MapElement.ID, MapElement> GetData(Coordinates bottomCoordinates, Coordinates topCoordinates) {
        Debug.Log("Requesting map data...");
        XElement rawMapData = MakeHTTPRequest(bottomCoordinates, topCoordinates);
        Dictionary<MapElement.ID, MapElement> mapData = new Dictionary<MapElement.ID, MapElement>();

        Debug.Log("Parsing values...");
        mapData = ParseNodes(ref rawMapData);
        mapData = mapData.Concat(ParseWays(ref rawMapData)).ToDictionary(x => x.Key, x=> x.Value);
        //TODO: Parse relations

        return mapData;
    }

    private static Dictionary<MapElement.ID, MapElement> ParseNodes(ref XElement xml) {
        Dictionary<MapElement.ID, MapElement> newData = new Dictionary<MapElement.ID, MapElement>();
        xml.Elements()
            .Where(element => element.Name.ToString() == "node")
            .ToList()
            .ForEach(node => {
                long rawId;
                double rawLat;
                double rawLong;
                Int64.TryParse(node.Attribute("id").Value, out rawId);
                Double.TryParse(node.Attribute("lat").Value, out rawLat);
                Double.TryParse(node.Attribute("lon").Value, out rawLong);
                MapElement.ID id = new MapElement.ID(rawId);
                MapElement newNode = new MapElement(id, Coordinates.of(rawLat, rawLong));

                Dictionary<MapNodeKey.KeyType, String> tags = ParseTags(node);
                newNode.AddDatum(tags);
                newData.Add(id, newNode);

                //TODO: for dev purposes only, remove later
                if(node.Elements().Count() == tags.Count) {
                    node.Remove();
                }
            });

        return newData;
    }

    private static Dictionary<MapElement.ID, MapElement> ParseWays(ref XElement xml) {
        Dictionary<MapElement.ID, MapElement> newData = new Dictionary<MapElement.ID, MapElement>();
        xml.Elements()
            .Where(element => element.Name.ToString() == "way")
            .ToList()
            .ForEach(way => {
                long rawId;
                Int64.TryParse(way.Attribute("id").Value, out rawId);
                MapElement.ID id = new MapElement.ID(rawId);
                MapElement newWay = new MapElement(id);
                
                Dictionary<MapNodeKey.KeyType, String> tags = ParseTags(way);
                List<MapElement.ID> nds = ParseNds(way);
                newWay.AddDatum(tags);
                newWay.AddReferences(nds);
                newData.Add(id, newWay);

                //TODO: for dev purposes only, remove later
                if(way.Elements().Count() == tags.Count + nds.Count) {
                    way.Remove();
                }
            });

        return newData;
    }
    
    private static Dictionary<MapNodeKey.KeyType, String> ParseTags(XElement tagParent) {
        Dictionary<MapNodeKey.KeyType, String> data = new Dictionary<MapNodeKey.KeyType, String>();

        tagParent.Elements()
            .Where(rawTag => rawTag.Name.ToString() == "tag")
            .ToList()
            .ForEach(tag => {
                MapNodeKey.KeyType key = MapNodeKey.GetTagType(tag.Attribute("k").Value);
                if(key != MapNodeKey.KeyType.None) {   
                    data.Add(key, tag.Attribute("v").Value);
                }
            });

        return data;
    }

    private static List<MapElement.ID> ParseNds(XElement ndParent) {
        List<MapElement.ID> data = new List<MapElement.ID>();

        ndParent.Elements()
            .Where(rawNd => rawNd.Name.ToString() == "nd")
            .ToList()
            .ForEach(nd => {
                long rawId;
                Int64.TryParse(nd.Attribute("ref").Value, out rawId);
                data.Add(new MapElement.ID(rawId));
            });

        return data;
    }

    private static XElement MakeHTTPRequest(Coordinates bottomCoordinates, Coordinates topCoordinates) {
        string xml = string.Empty;
        string url = MAP_DATA_API_URL + 
        bottomCoordinates.Longitude.ToString() + "," + 
        bottomCoordinates.Latitude.ToString() + "," +
        topCoordinates.Longitude.ToString() + "," +
        topCoordinates.Latitude.ToString();

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            xml = reader.ReadToEnd();
        }

        return XElement.Parse(xml);
    }
}
