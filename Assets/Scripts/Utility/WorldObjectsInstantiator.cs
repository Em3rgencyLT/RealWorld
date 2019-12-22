using System.Collections.Generic;
using Domain;
using Domain.Tuples;
using Services;
using UnityEngine;

namespace Utility
{
    public static class WorldObjectsInstantiator
    {
        public static WorldObjects InstantiateWorldObjects(List<StructureWithVertices> structureVertexData, List<WayWithVertices> wayVertexData, Int2 location, GameObject structurePrefab, GameObject roadPrefab, TerrainHeightService heightService, GameObject mapDataParentObject)
        {
            var mapObjects = new List<MapObject>();
            var chunkParent = new GameObject($"WorldObjects X:{location.X} Y:{location.Y}");
            chunkParent.transform.parent = mapDataParentObject.transform;
            
            //TODO: maybe generalize and combine structureVertexData and wayVertexData? The XML they read from is the same, and these foreach loops look awfully similar.
            foreach (StructureWithVertices structureWithVertices in structureVertexData)
            {
                MapElement mapElement = structureWithVertices.MapElement;
                GameObject structureObject = GameObject.Instantiate(structurePrefab, chunkParent.transform);
                structureObject.name = string.IsNullOrWhiteSpace(mapElement.GetAddress())
                    ? "Building"
                    : mapElement.GetAddress();
                Structure structureScript = structureObject.GetComponent<Structure>();
                structureScript.Build(heightService, mapElement, structureWithVertices.Vertices);
                mapObjects.Add(structureScript);
            }
            foreach (WayWithVertices wayWithVertices in wayVertexData)
            {
                if (wayWithVertices.LeftVertices.Count <= 1 || wayWithVertices.RightVertices.Count <= 1 ||
                    wayWithVertices.LeftVertices.Count != wayWithVertices.RightVertices.Count)
                {
                    continue;
                }

                //TODO: ways can also be rivers and fuck knows what else. Need support for all of that too.
                GameObject wayObject = GameObject.Instantiate(roadPrefab, chunkParent.transform);
                MapElement mapElement = wayWithVertices.MapElement;
                wayObject.name = string.IsNullOrWhiteSpace(mapElement.GetRoadName())
                        ? "Road"
                        : mapElement.GetRoadName();
                Road roadScript = wayObject.GetComponent<Road>();
                roadScript.Build(heightService, mapElement, wayWithVertices.LeftVertices, wayWithVertices.RightVertices);
                mapObjects.Add(roadScript);
            }
            
            return new WorldObjects(mapObjects);
        } 
    }
}