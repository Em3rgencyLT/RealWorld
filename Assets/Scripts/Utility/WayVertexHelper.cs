using System.Collections.Generic;
using Domain;
using Domain.Tuples;
using UnityEngine;

namespace Utility
{
    public class WayVertexHelper
    {
        public static List<WayWithVertices> GetWaysWithVertices(Dictionary<MapElement.ID, MapElement> mapElements, Bounds<Vector3> terrainBounds)
        {
            List<WayWithVertices> waysWithVertices = new List<WayWithVertices>();
            foreach (MapElement mapElement in mapElements.Values)
            {
                if (!mapElement.Data.ContainsKey(MapNodeKey.KeyType.Highway))
                {
                    continue;
                }

                List<CoordinatesWithPosition> waypoints = new List<CoordinatesWithPosition>();
                mapElement.References
                    .ForEach(reference => { waypoints.Add(mapElements[reference].CoordinatesWithPosition); });

                List<Vector3> leftVerticePositions = new List<Vector3>();
                List<Vector3> rightVerticePositions = new List<Vector3>();
                float width = Road.GuessRoadWidth(mapElement.Data[MapNodeKey.KeyType.Highway]);

                //TODO: pad waypoint list to have a points at fixed intervals, preventing long road segments
                waypoints
                    .ForEach(waypoint =>
                    {
                        int index = waypoints.IndexOf(waypoint);
                        Vector3 forward = Vector3.zero;

                        if (index < waypoints.Count - 1)
                        {
                            forward += waypoints[index + 1].Position - waypoint.Position;
                        }

                        if (index > 0)
                        {
                            forward += waypoint.Position - waypoints[index - 1].Position;
                        }

                        forward.Normalize();
                        Vector3 position = waypoint.Position;
                        if (index == 0)
                        {
                            position -= forward * width / 10;
                        }

                        if (index == waypoints.Count - 1)
                        {
                            position += forward * width / 10;
                        }

                        Vector3 leftDirection = new Vector3(-forward.z, 0f, forward.x);
                        Vector3 leftVertex = position + leftDirection * width;
                        Vector3 rightVertex = position - leftDirection * width;

                        if (PointIsWithinBounds(terrainBounds, leftVertex) &&
                            PointIsWithinBounds(terrainBounds, rightVertex))
                        {
                            leftVerticePositions.Add(leftVertex);
                            rightVerticePositions.Add(rightVertex);
                        }
                        
                    });
                var wayWithVertices = new WayWithVertices(mapElement, leftVerticePositions, rightVerticePositions);
                waysWithVertices.Add(wayWithVertices);
            }

            return waysWithVertices;
        }

        private static bool PointIsWithinBounds(Bounds<Vector3> bounds, Vector3 point)
        {
            return bounds.MinPoint.x < point.x && 
                   bounds.MinPoint.z < point.z && 
                   bounds.MaxPoint.x > point.x &&
                   bounds.MaxPoint.z > point.z;
        }
    }
}