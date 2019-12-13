using System.Collections.Generic;
using Domain;
using Domain.Tuples;
using UnityEngine;

namespace Utility
{
    public class StructureVertexHelper
    {
        public static List<StructureWithVertices> GetStructuresWithVertices(Dictionary<MapElement.ID, MapElement> mapElements)
        {
            List<StructureWithVertices> stucturesWithVertices = new List<StructureWithVertices>();
            foreach (MapElement mapElement in mapElements.Values)
            {
                if (!mapElement.Data.ContainsKey(MapNodeKey.KeyType.Building))
                {
                    continue;
                }

                List<Vector3> verticePositions = new List<Vector3>();
                
                mapElement.References.ForEach(reference =>
                {
                    verticePositions.Add(mapElements[reference].CoordinatesWithPosition.Position);
                });
                
                if (verticePositions.Count < 3)
                {
                    continue;
                }
                verticePositions.RemoveAt(verticePositions.Count - 1);
                var structureWithVertices = new StructureWithVertices(mapElement, verticePositions);
                stucturesWithVertices.Add(structureWithVertices);
            }

            return stucturesWithVertices;
        }
    }
}