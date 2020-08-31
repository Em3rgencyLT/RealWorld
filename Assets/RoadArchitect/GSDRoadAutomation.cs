using RoadArchitect;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

#endif
namespace GSD.Roads
{
#if UNITY_EDITOR
    // Proper automation flow:
    // 1. Make sure opt_bAllowRoadUpdates in the scene's GSDRoadSystem is set to FALSE.
    // 2. Create your roads programmatically via CreateRoad_Programmatically (pass it the road, and then the points in a list)
    //      a. Optionally you can do it via CreateNode_Programmatically and InsertNode_Programmatically
    // 3. Call CreateIntersections_ProgrammaticallyForRoad for each road to create intersections automatically at intersection points.
    // 4. Set opt_bAllowRoadUpdates in the scene's GSDRoadSystem is set to TRUE.
    // 5. Call GSDRoadSystem.UpdateAllRoads();
    // 6. Call GSDRoadSystem.UpdateAllRoads(); after step #5 completes.
    //
    // See "GSDUnitTests.cs" for an example on automation (ignore unit test #3).


    public static class GSDRoadAutomation
    {
        /// <summary>
        /// Use this to create nodes via coding while in editor mode. Make sure opt_bAllowRoadUpdates is set to false in RS.GSDRS.opt_bAllowRoadUpdates.
        /// </summary>
        /// <param name="RS">The road system to create nodes on.</param>
        /// <param name="NodeLocation">The location of the newly created node.</param>
        /// <returns></returns>
        public static GSDRoad CreateRoad_Programmatically(GSDRoadSystem GSDRS, ref List<Vector3> tLocs)
        {
            var tRoadObj = GSDRS.AddRoad(false);
            var tRoad = tRoadObj.GetComponent<GSDRoad>();

            var hCount = tLocs.Count;
            for (var i = 0; i < hCount; i++) CreateNode_Programmatically(tRoad, tLocs[i]);

            return tRoad;
        }


        /// <summary>
        /// Use this to create nodes via coding while in editor mode. Make sure opt_bAllowRoadUpdates is set to false in RS.GSDRS.opt_bAllowRoadUpdates.
        /// </summary>
        /// <param name="RS">The road system to create nodes on.</param>
        /// <param name="NodeLocation">The location of the newly created node.</param>
        /// <returns></returns>
        public static GSDSplineN CreateNode_Programmatically(GSDRoad tRoad, Vector3 NodeLocation)
        {
            var SplineChildCount = tRoad.GSDSpline.transform.childCount;
            var tNodeObj = new GameObject("Node" + (SplineChildCount + 1).ToString());
            var tNode = tNodeObj.AddComponent<GSDSplineN>(); //Add the node component.

            //Set node location:
            if (NodeLocation.y < 0.03f) NodeLocation.y = 0.03f;
            tNodeObj.transform.position = NodeLocation;

            //Set the node's parent:
            tNodeObj.transform.parent = tRoad.GSDSplineObj.transform;

            //Set the idOnSpline:
            tNode.idOnSpline = SplineChildCount + 1;
            tNode.GSDSpline = tRoad.GSDSpline;

            //Make sure opt_bAllowRoadUpdates is set to false in RS.GSDRS.opt_bAllowRoadUpdates
            tRoad.UpdateRoad();

            return tNode;
        }

        /// <summary>
        /// Use this to insert nodes via coding while in editor mode. Make sure opt_bAllowRoadUpdates is set to false in RS.GSDRS.opt_bAllowRoadUpdates.
        /// </summary>
        /// <param name="RS">The road system to insert nodes in.</param>
        /// <param name="NodeLocation">The location of the newly inserted node.</param>
        /// <returns></returns>
        public static GSDSplineN InsertNode_Programmatically(GSDRoad RS, Vector3 NodeLocation)
        {
            GameObject tNodeObj;
            var tWorldNodeCount = Object.FindObjectsOfType(typeof(GSDSplineN));
            tNodeObj = new GameObject("Node" + tWorldNodeCount.Length.ToString());

            //Set node location:
            if (NodeLocation.y < 0.03f) NodeLocation.y = 0.03f;
            tNodeObj.transform.position = NodeLocation;

            //Set the node's parent:
            tNodeObj.transform.parent = RS.GSDSplineObj.transform;

            var cCount = RS.GSDSpline.mNodes.Count;

            //Get the closet param on spline:
            var tParam = RS.GSDSpline.GetClosestParam(NodeLocation, false, true);

            var bEndInsert = false;
            var bZeroInsert = false;
            var iStart = 0;
            if (GSDRootUtil.IsApproximately(tParam, 0f, 0.0001f))
            {
                bZeroInsert = true;
                iStart = 0;
            }
            else if (GSDRootUtil.IsApproximately(tParam, 1f, 0.0001f))
            {
                //Inserted at end, switch to create node instead:
                Object.DestroyImmediate(tNodeObj);
                return CreateNode_Programmatically(RS, NodeLocation);
            }

            //Figure out where to insert the node:
            for (var i = 0; i < cCount; i++)
            {
                var xNode = RS.GSDSpline.mNodes[i];
                if (!bZeroInsert && !bEndInsert)
                    if (tParam > xNode.tTime)
                        iStart = xNode.idOnSpline + 1;
            }

            for (var i = iStart; i < cCount; i++) RS.GSDSpline.mNodes[i].idOnSpline += 1;

            var tNode = tNodeObj.AddComponent<GSDSplineN>();
            tNode.GSDSpline = RS.GSDSpline;
            tNode.idOnSpline = iStart;
            tNode.pos = NodeLocation;
            RS.GSDSpline.mNodes.Insert(iStart, tNode);

            //Make sure opt_bAllowRoadUpdates is set to false in RS.GSDRS.opt_bAllowRoadUpdates
            RS.UpdateRoad();

            return tNode;
        }


        /// <summary>
        /// Creates intersections where this road intersects with other roads.
        /// </summary>
        /// <param name="tRoad">The primary road to create intersections for.</param>
        /// <param name="iStopType">Stop signs, traffic lights #1 (US) or traffic lights #2 (Euro). Defaults to none.</param>
        /// <param name="rType">Intersection type: No turn lane, left turn lane or both turn lanes. Defaults to no turn lane.</param>
        public static void CreateIntersections_ProgrammaticallyForRoad(GSDRoad tRoad,
            GSDRoadIntersection.iStopTypeEnum iStopType = GSDRoadIntersection.iStopTypeEnum.None,
            GSDRoadIntersection.RoadTypeEnum rType = GSDRoadIntersection.RoadTypeEnum.NoTurnLane)
        {
            /*
            General logic:
             20m increments to gather collection of which roads intersect
                2m increments to find actual intersection point
                each 2m, primary road checks all intersecting array for an intersection.
             find intersection point
                if any intersections already within 75m or 100m, dont create intersection here
                check if nodes within 50m, if more than one just grab closest, and move  it to intersecting point
                if no node within 50m, add
             create intersection with above two nodes
            */

            Object[] GSDRoadObjs = Object.FindObjectsOfType<GSDRoad>();

            //20m increments to gather collection of which roads intersect
            var xRoads = new List<GSDRoad>();
            foreach (GSDRoad xRoad in GSDRoadObjs)
                if (tRoad != xRoad)
                {
                    var EarlyDistanceCheckMeters = 10f;
                    var EarlyDistanceCheckThreshold = 50f;
                    var EarlyDistanceFound = false;
                    var tRoadMod = EarlyDistanceCheckMeters / tRoad.GSDSpline.distance;
                    var xRoadMod = EarlyDistanceCheckMeters / xRoad.GSDSpline.distance;
                    var tVect1 = default(Vector3);
                    var tVect2 = default(Vector3);
                    for (var i = 0f; i < 1.0000001f; i += tRoadMod)
                    {
                        tVect1 = tRoad.GSDSpline.GetSplineValue(i);
                        for (var x = 0f; x < 1.000001f; x += xRoadMod)
                        {
                            tVect2 = xRoad.GSDSpline.GetSplineValue(x);
                            if (Vector3.Distance(tVect1, tVect2) < EarlyDistanceCheckThreshold)
                            {
                                if (!xRoads.Contains(xRoad)) xRoads.Add(xRoad);
                                EarlyDistanceFound = true;
                                break;
                            }
                        }

                        if (EarlyDistanceFound) break;
                    }
                }

            //See if any end point nodes are on top of each other already since T might not intersect all the time.:
            var tKVP = new List<KeyValuePair<GSDSplineN, GSDSplineN>>();
            foreach (var xRoad in xRoads)
            foreach (var IntersectionNode1 in tRoad.GSDSpline.mNodes)
            {
                if (IntersectionNode1.bIsIntersection || !IntersectionNode1.IsLegitimate()) continue;
                foreach (var IntersectionNode2 in xRoad.GSDSpline.mNodes)
                {
                    if (IntersectionNode2.bIsIntersection || !IntersectionNode2.IsLegitimate()) continue;
                    if (IntersectionNode1.transform.position == IntersectionNode2.transform.position
                    ) //Only do T intersections and let the next algorithm handle the +, since T might not intersect all the time.
                        if (IntersectionNode1.bIsEndPoint || IntersectionNode2.bIsEndPoint)
                            tKVP.Add(new KeyValuePair<GSDSplineN, GSDSplineN>(IntersectionNode1, IntersectionNode2));
                }
            }

            foreach (var KVP in tKVP)
            {
                //Now create the fucking intersection:
                var tInter = GSDIntersections.CreateIntersection(KVP.Key, KVP.Value);
                var GSDRI_JustCreated = tInter.GetComponent<GSDRoadIntersection>();
                GSDRI_JustCreated.iStopType = iStopType;
                GSDRI_JustCreated.rType = rType;
            }

            //Main algorithm: 2m increments to find actual intersection point:
            foreach (var xRoad in xRoads)
                if (tRoad != xRoad)
                {
                    //Debug.Log("Checking road: " + xRoad.transform.name);
                    var DistanceCheckMeters = 2f;
                    var EarlyDistanceFound = false;
                    var tRoadMod = DistanceCheckMeters / tRoad.GSDSpline.distance;
                    var xRoadMod = DistanceCheckMeters / xRoad.GSDSpline.distance;
                    var tVect = default(Vector3);
                    var iVect1 = default(Vector2);
                    var iVect2 = default(Vector2);
                    var xVect1 = default(Vector2);
                    var xVect2 = default(Vector2);
                    var IntersectPoint2D = default(Vector2);
                    var i2 = 0f;
                    for (var i = 0f; i < 1.0000001f; i += tRoadMod)
                    {
                        i2 = i + tRoadMod;
                        if (i2 > 1f) i2 = 1f;
                        tVect = tRoad.GSDSpline.GetSplineValue(i);
                        iVect1 = new Vector2(tVect.x, tVect.z);
                        tVect = tRoad.GSDSpline.GetSplineValue(i2);
                        iVect2 = new Vector2(tVect.x, tVect.z);

                        var x2 = 0f;
                        for (var x = 0f; x < 1.000001f; x += xRoadMod)
                        {
                            x2 = x + xRoadMod;
                            if (x2 > 1f) x2 = 1f;
                            tVect = xRoad.GSDSpline.GetSplineValue(x);
                            xVect1 = new Vector2(tVect.x, tVect.z);
                            tVect = xRoad.GSDSpline.GetSplineValue(x2);
                            xVect2 = new Vector2(tVect.x, tVect.z);

                            //Now see if these two lines intersect:
                            if (GSDRootUtil.Intersects2D(ref iVect1, ref iVect2, ref xVect1, ref xVect2,
                                out IntersectPoint2D))
                            {
                                //Get height of intersection on primary road:
                                var tHeight = 0f;
                                var hParam =
                                    tRoad.GSDSpline.GetClosestParam(new Vector3(IntersectPoint2D.x, 0f,
                                        IntersectPoint2D.y));
                                var hVect = tRoad.GSDSpline.GetSplineValue(hParam);
                                tHeight = hVect.y;

                                //if any intersections already within 75m or 100m, dont create intersection here
                                Object[] AllInterectionObjects = Object.FindObjectsOfType<GSDRoadIntersection>();
                                foreach (GSDRoadIntersection GSDRI in AllInterectionObjects)
                                    if (Vector2.Distance(
                                        new Vector2(GSDRI.transform.position.x, GSDRI.transform.position.z),
                                        IntersectPoint2D) < 100f)
                                        goto NoIntersectionCreation;

                                GSDSplineN IntersectionNode1 = null;
                                GSDSplineN IntersectionNode2 = null;
                                var IntersectionPoint3D = new Vector3(IntersectPoint2D.x, tHeight, IntersectPoint2D.y);
                                //Debug.Log("Instersect found road: " + xRoad.transform.name + " at point: " + IntersectionPoint3D.ToString());

                                //Check primary road if any nodes are nearby and usable for intersection
                                foreach (var tNode in tRoad.GSDSpline.mNodes)
                                    if (tNode.IsLegitimate())
                                        if (Vector2.Distance(
                                            new Vector2(tNode.transform.position.x, tNode.transform.position.z),
                                            IntersectPoint2D) < 30f)
                                        {
                                            IntersectionNode1 = tNode;
                                            IntersectionNode1.transform.position = IntersectionPoint3D;
                                            IntersectionNode1.pos = IntersectionPoint3D;
                                            break;
                                        }

                                //Check secondary road if any nodes are nearby and usable for intersection
                                foreach (var tNode in xRoad.GSDSpline.mNodes)
                                    if (tNode.IsLegitimate())
                                        if (Vector2.Distance(
                                            new Vector2(tNode.transform.position.x, tNode.transform.position.z),
                                            IntersectPoint2D) < 30f)
                                        {
                                            IntersectionNode2 = tNode;
                                            IntersectionNode2.transform.position = IntersectionPoint3D;
                                            IntersectionNode2.pos = IntersectionPoint3D;
                                            break;
                                        }

                                //Check if any of the nodes are null. If so, need to insert node. And maybe update it.
                                if (IntersectionNode1 == null)
                                    IntersectionNode1 = InsertNode_Programmatically(tRoad, IntersectionPoint3D);
                                if (IntersectionNode2 == null)
                                    IntersectionNode2 = InsertNode_Programmatically(xRoad, IntersectionPoint3D);

                                //Now create the fucking intersection:
                                var tInter = GSDIntersections.CreateIntersection(IntersectionNode1, IntersectionNode2);
                                var GSDRI_JustCreated = tInter.GetComponent<GSDRoadIntersection>();
                                GSDRI_JustCreated.iStopType = iStopType;
                                GSDRI_JustCreated.rType = rType;
                            }

                            NoIntersectionCreation:
                            //Gibberish to get rid of warnings:
                            var xxx = 1;
                            if (xxx == 1) xxx = 2;
                        }

                        if (EarlyDistanceFound) break;
                    }
                }
        }
    }
#endif
}