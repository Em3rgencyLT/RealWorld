using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;
using Domain;
using Domain.Tuples;
using Utility;
using SRTM;

public class MapData
{
    private const string MAP_DATA_API_URL = @"https://overpass-api.de/api/map?bbox=";

    public static Vector3[,] GetElevationData(CoordinateBounding coordinateBounding) {
        Debug.Log("Requesting elevation data...");
        Elevation[,] rawData = GetRawElevationData(coordinateBounding);
        Vector3[,] elevationData = ParseElevationData(rawData);

        return elevationData;
    }

    public static Dictionary<MapElement.ID, MapElement> GetObjectData(CoordinateBounding coordinateBounding) {
        Debug.Log("Requesting map object data...");
        XElement rawMapData = GetRawMapData(coordinateBounding);
        Dictionary<MapElement.ID, MapElement> mapData = new Dictionary<MapElement.ID, MapElement>();

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

    private static XElement GetRawMapData(CoordinateBounding coordinateBounding) {
        string url = MAP_DATA_API_URL + 
        coordinateBounding.BottomCoordinates.Longitude + "," + 
        coordinateBounding.BottomCoordinates.Latitude + "," +
        coordinateBounding.TopCoordinates.Longitude + "," +
        coordinateBounding.TopCoordinates.Latitude;

        string xml = HttpRequest.Get(url);
        return XElement.Parse(xml);
    }

    private static Elevation[,] GetRawElevationData(CoordinateBounding coordinateBounding, double step = 0.0005)
    {
        //FIXME: step is tied to world size, it must result in a power of 2 ratio
        var bottomCoordinates = coordinateBounding.BottomCoordinates;
        var topCoordinates = coordinateBounding.TopCoordinates;

        int stepsX = (int)Math.Ceiling((topCoordinates.Latitude - bottomCoordinates.Latitude) / step);
        int stepsY = (int)Math.Ceiling((topCoordinates.Longitude - bottomCoordinates.Longitude) / step);

        Elevation[,] data = new Elevation[stepsX, stepsY];
        
        string slashDirection = @"\";
        if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor)
        {
            slashDirection = "/";
        }
        var srtmData = new SRTMData(Application.dataPath + slashDirection + "SRTMData");

        for(int i = 0; i < stepsX; i++) {
            for(int j = 0; j < stepsY; j++) {
                double lat = bottomCoordinates.Latitude + step * i;
                double lon = bottomCoordinates.Longitude + step * j;
                double? elevation = srtmData.GetElevation(lat, lon);
                double height = -100;

                if(elevation.HasValue) {
                    height = elevation.Value;
                }

                data[i, j] = new Elevation(Coordinates.of(lat, lon), height);
            }
        }

        return data;
    }

    private static Vector3[,] ParseElevationData(Elevation[,] rawData)
    {
        int iSize = rawData.GetLength(0);
        int jSize = rawData.GetLength(1);
        Vector3[,] data = new Vector3[iSize, jSize];

        for(int i = 0; i < iSize; i++)
        {
            for(int j = 0; j < jSize; j++)
            {
                Elevation elevation = rawData[i, j];
                Vector3 basePosition = CoordinateMath.CoordinatesToWorldPosition(elevation.Coordinates.Latitude, elevation.Coordinates.Longitude);
                data[i, j] = new Vector3(basePosition.x, (float)elevation.Height, basePosition.z);
            }
        }

        return data;
    }
}