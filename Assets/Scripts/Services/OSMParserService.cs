using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using Domain;
using Domain.Tuples;
using JetBrains.Annotations;
using Services;
using Utility;

public class OSMParserService
{
    private SRTMDataService _srtmDataService;
    private CoordinatePositionService _coordinatePositionService;
    
    public OSMParserService(SRTMDataService srtmDataService, CoordinatePositionService coordinatePositionService)
    {
        _srtmDataService = srtmDataService;
        _coordinatePositionService = coordinatePositionService;
    }

    public Dictionary<MapElement.ID, MapElement> Parse(XElement osmData)
    {
        var nodes = ReadNodeData(osmData);
        var ways = ReadWayData(osmData);
        nodes.AddRange(ways);
        return nodes.ToDictionary(node => node.Id, node => node);
        //TODO: Parse relations
    }

    private List<MapElement> ReadNodeData(XElement rawXml)
    {
        var collector = new XmlCollector<MapElement>();
        return collector.Collect(rawXml, "node", ParseNode);
    }

    private List<MapElement> ReadWayData(XElement rawXml)
    {
        var collector = new XmlCollector<MapElement>();
        return collector.Collect(rawXml, "way", ParseWay);
    }

    private Dictionary<MapNodeKey.KeyType, String> ReadTagData(XElement rawXml)
    {
        var collector = new XmlCollector<MapNodeTag>();
        return collector.Collect(rawXml, "tag", ParseTag).ToDictionary(tag => tag.KeyType, tag => tag.Value);
    }

    private List<MapElement.ID> ReadNdData(XElement rawXml)
    {
        var collector = new XmlCollector<MapElement.ID>();
        return collector.Collect(rawXml, "nd", ParseNd);
    }

    private MapElement ParseNode(XElement node)
    {
        Dictionary<MapNodeKey.KeyType, String> tags = ReadTagData(node);
        long.TryParse(node.Attribute("id").Value, out var rawId);
        double.TryParse(node.Attribute("lat").Value, out var rawLat);
        double.TryParse(node.Attribute("lon").Value, out var rawLong);
        MapElement.ID id = new MapElement.ID(rawId);
        Coordinates coordinates = Coordinates.of(rawLat, rawLong);
        double height = _srtmDataService.GetHeight(coordinates);
        CoordinatesWithPosition coordinatesWithPosition = _coordinatePositionService.GetCoordinatesWithPosition(coordinates, height);
        List<MapElement.ID> nds = ReadNdData(node);

        return new MapElement(id, coordinatesWithPosition, tags, nds);
    }

    private MapElement ParseWay(XElement way)
    {
        long.TryParse(way.Attribute("id").Value, out var rawId);
        MapElement.ID id = new MapElement.ID(rawId);
        Dictionary<MapNodeKey.KeyType, String> tags = ReadTagData(way);
        List<MapElement.ID> nds = ReadNdData(way);

        return new MapElement(id, null, tags, nds);
    }

    private static MapElement.ID ParseNd(XElement nd)
    {
        long.TryParse(nd.Attribute("ref").Value, out var rawId);
        return new MapElement.ID(rawId);
    }

    [CanBeNull]
    private static MapNodeTag ParseTag(XElement tag)
    {
        MapNodeKey.KeyType key = MapNodeKey.GetTagType(tag.Attribute("k").Value);

        if (key == MapNodeKey.KeyType.None)
        {
            return null;
        }

        string value = tag.Attribute("v").Value;
        return new MapNodeTag(key, value);
    }
}