using System.Collections.Generic;
using Domain;
using Domain.Tuples;
using RoadArchitect;
using Services;
using UnityEngine;

namespace Utility
{
    public static class WorldObjectsInstantiator
    {
        public static WorldObjects InstantiateWorldObjects(List<StructureWithVertices> structureVertexData, List<WayWithVertices> wayVertexData, Int2 location, GameObject structurePrefab, GameObject roadPrefab, TerrainHeightService heightService, GameObject mapDataParentObject, GSDRoadSystem gsdRoadSystem)
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
                if (wayWithVertices.SplineVertices.Count <= 1) continue;

                //TODO: ways can also be rivers and fuck knows what else. Need support for all of that too.
                MapElement mapElement = wayWithVertices.MapElement;
                string name = string.IsNullOrWhiteSpace(mapElement.GetRoadName())
                    ? "Road"
                    : mapElement.GetRoadName();
                GameObject roadObject = gsdRoadSystem.AddRoad(false, name);
                roadObject.transform.position = wayWithVertices.SplineVertices[0];
                Road roadScript = roadObject.AddComponent<Road>();
                
                roadScript.Build(heightService, mapElement, wayWithVertices.SplineVertices);
                mapObjects.Add(roadScript);
            }
            
            return new WorldObjects(mapObjects);
        } 
    }
}