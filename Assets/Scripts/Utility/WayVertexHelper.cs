using System.Collections.Generic;
using System.Linq;
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
                List<WayNodes> segments = GetRoadSegments(waypoints, terrainBounds);

                segments.ForEach(waySegment =>
                {
                    var wayWithVertices = new WayWithVertices(mapElement, waySegment.Nodes);
                    waysWithVertices.Add(wayWithVertices);
                }); 
            }
            return waysWithVertices;
        }
        
        private static List<WayNodes> GetRoadSegments(List<CoordinatesWithPosition> waypoints,
            Bounds<Vector3> terrainBounds)
        {
            //TODO: pad waypoint list to have a points at fixed intervals, preventing long road segments
            List<WayNodes> wayNodes = new List<WayNodes>();
            List<Vector3> points = new List<Vector3>();
            bool currentlyAssemblingWay = false;

            for (int i = 0; i < waypoints.Count; i++)
            {
                bool isInside = PointIsWithinBounds(terrainBounds, waypoints[i].Position);
                
                //outside segment
                if (!isInside && !currentlyAssemblingWay)
                {
                    continue;
                }
                
                //first segment
                if (isInside && !currentlyAssemblingWay)
                {
                    currentlyAssemblingWay = true;
                    points = new List<Vector3>();
                    if (i > 0)
                    {
                        var cutoff = GetCutoffPoint(waypoints[i - 1].Position, waypoints[i].Position, terrainBounds);
                        points.Add(cutoff);
                    }
                    points.Add(waypoints[i].Position);
                    continue;
                }
                
                //middle segment
                if (isInside && currentlyAssemblingWay)
                {
                    points.Add(waypoints[i].Position);
                    continue;
                }
                
                //last segment
                if (!isInside && currentlyAssemblingWay)
                {
                    currentlyAssemblingWay = false;
                    var cutoff = GetCutoffPoint(waypoints[i].Position, waypoints[i-1].Position, terrainBounds);
                    points.Add(cutoff);
                    WayNodes nodes = new WayNodes(points);
                    wayNodes.Add(nodes);
                }
            }

            if (points.Count == 0)
            {
                return wayNodes;
            }

            WayNodes leftOverNodes = new WayNodes(points);
            wayNodes.Add(leftOverNodes);
            return wayNodes;
        }

        private static bool PointIsWithinBounds(Bounds<Vector3> bounds, Vector3 point)
        {
            return bounds.MinPoint.x < point.x && 
                   bounds.MinPoint.z < point.z && 
                   bounds.MaxPoint.x > point.x &&
                   bounds.MaxPoint.z > point.z;
        }

        private static Vector3 GetCutoffPoint(Vector3 outsidePoint, Vector3 previousPoint, Bounds<Vector3> bounds)
        {
            Vector2 roadPoint1 = new Vector2(previousPoint.x, previousPoint.z);
            Vector2 roadPoint2 = new Vector2(outsidePoint.x, outsidePoint.z);
            Vector2 chunkPoint1 = new Vector2(bounds.MinPoint.x, bounds.MinPoint.z);
            Vector2 chunkPoint2 = new Vector2(bounds.MinPoint.x, bounds.MaxPoint.z);
            Vector2 chunkPoint3 = new Vector2(bounds.MaxPoint.x, bounds.MaxPoint.z);
            Vector2 chunkPoint4 = new Vector2(bounds.MaxPoint.x, bounds.MinPoint.z);
            
            List<Vector2> intersections = new List<Vector2>();
            
            intersections.Add(LineHelper.FindIntersection(roadPoint1, roadPoint2, chunkPoint1, chunkPoint2));
            intersections.Add(LineHelper.FindIntersection(roadPoint1, roadPoint2, chunkPoint3, chunkPoint2));
            intersections.Add(LineHelper.FindIntersection(roadPoint1, roadPoint2, chunkPoint4, chunkPoint3));
            intersections.Add(LineHelper.FindIntersection(roadPoint1, roadPoint2, chunkPoint1, chunkPoint4));

            Vector2 intersection = intersections.OrderBy(point => Vector2.Distance(point, roadPoint2)).First();
            float lerpAmount = (intersection.x - roadPoint1.x) / (roadPoint2.x - roadPoint1.x);
            float y = Vector3.Lerp(previousPoint, outsidePoint, lerpAmount).y;
            return new Vector3(intersection.x, y, intersection.y);
        }
    }
}