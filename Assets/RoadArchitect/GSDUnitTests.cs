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
    public static class GSDUnitTests
    {
        private static GSDRoadSystem RoadSystem;

        /// <summary>
        /// WARNING: Only call this on an empty scene that has some terrains on it. MicroGSD LLC is not responsbile for data loss if this function is called by user.
        /// </summary>
        public static void RoadArchitectUnitTests()
        {
            CleanupTests();

            //Create new road system and turn off updates:
            var tRoadSystemObj = new GameObject("RoadArchitectSystem1");
            RoadSystem = tRoadSystemObj.AddComponent<GSDRoadSystem>(); //Add road system component.
            RoadSystem.opt_bAllowRoadUpdates = false;

            var numTests = 5;
            double totalTestTime = 0f;
            for (var i = 1; i <= numTests; i++)
            {
                Debug.Log("Running test " + i);
                double testTime = RunTest(i);
                totalTestTime += testTime;
                Debug.Log("Test " + i + " complete.  Test time: " + testTime + "ms");
            }

            Debug.Log("All tests completed.  Total test time: " + totalTestTime + "ms");

            //Turn updates back on and update road:
            RoadSystem.opt_bAllowRoadUpdates = true;
            RoadSystem.UpdateAllRoads();
        }


        private static long RunTest(int test)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            switch (test)
            {
                case 1:
                    RoadArchitectUnitTest1(); //Bridges
                    break;
                case 2:
                    RoadArchitectUnitTest2(); //2L intersections
                    break;
                case 3:
                    RoadArchitectUnitTest3(); //4L intersections
                    break;
                case 4:
                    RoadArchitectUnitTest4(); //Large suspension bridge
                    break;
                case 5:
                    RoadArchitectUnitTest5(); //Long road:
                    break;
            }

            stopwatch.Stop();
            var testTime = stopwatch.ElapsedMilliseconds;
            return testTime;
        }


        public static void CleanupTests()
        {
            Debug.Log("Cleaning up tests");
            //Get the existing road system, if it exists:
            var GSDRS = (GameObject) GameObject.Find("RoadArchitectSystem1");
            DestroyTerrainHistory(GSDRS);
            Object.DestroyImmediate(GSDRS);
            FlattenTerrains();
        }


        private static void DestroyTerrainHistory(GameObject GSDRS)
        {
            //Destroy the terrain histories:
            if (GSDRS != null)
            {
                Object[] tRoads = GSDRS.GetComponents<GSDRoad>();
                foreach (GSDRoad xRoad in tRoads) GSDTerraforming.TerrainsReset(xRoad);
            }
        }


        private static void FlattenTerrains()
        {
            //Reset all terrains to 0,0
            Object[] zTerrains = Object.FindObjectsOfType<Terrain>();
            foreach (Terrain xTerrain in zTerrains) xTerrain.terrainData.SetHeights(0, 0, new float[513, 513]);
        }

        private static void RoadArchitectUnitTest1()
        {
            //Create node locations:
            var nodeLocations = new List<Vector3>();
            var MaxCount = 18;
            var tMod = 100f;
            var xVect = new Vector3(50f, 40f, 50f);
            for (var i = 0;
                i < MaxCount;
                i++) //tLocs.Add(xVect + new Vector3(tMod * Mathf.Pow((float)i / ((float)MaxCount * 0.15f), 2f), 1f*((float)i*1.25f), tMod * i));
                nodeLocations.Add(xVect + new Vector3(tMod * Mathf.Pow((float) i / ((float) 25 * 0.15f), 2f), 0f,
                    tMod * i));

            //Get road system create road:
            var tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);

            //Bridge0: (Arch)
            tRoad.GSDSpline.mNodes[4].bIsBridgeStart = true;
            tRoad.GSDSpline.mNodes[4].bIsBridgeMatched = true;
            tRoad.GSDSpline.mNodes[7].bIsBridgeEnd = true;
            tRoad.GSDSpline.mNodes[7].bIsBridgeMatched = true;
            tRoad.GSDSpline.mNodes[4].BridgeCounterpartNode = tRoad.GSDSpline.mNodes[7];
            tRoad.GSDSpline.mNodes[4].LoadWizardObjectsFromLibrary("Arch12m-2L", true, true);

            //Bridge1: (Federal causeway)
            tRoad.GSDSpline.mNodes[8].bIsBridgeStart = true;
            tRoad.GSDSpline.mNodes[8].bIsBridgeMatched = true;
            tRoad.GSDSpline.mNodes[8].BridgeCounterpartNode = tRoad.GSDSpline.mNodes[10];
            tRoad.GSDSpline.mNodes[8].LoadWizardObjectsFromLibrary("Causeway1-2L", true, true);
            tRoad.GSDSpline.mNodes[10].bIsBridgeEnd = true;
            tRoad.GSDSpline.mNodes[10].bIsBridgeMatched = true;

            //Bridge2: (Steel)
            tRoad.GSDSpline.mNodes[11].bIsBridgeStart = true;
            tRoad.GSDSpline.mNodes[11].bIsBridgeMatched = true;
            tRoad.GSDSpline.mNodes[11].BridgeCounterpartNode = tRoad.GSDSpline.mNodes[13];
            tRoad.GSDSpline.mNodes[11].LoadWizardObjectsFromLibrary("Steel-2L", true, true);
            tRoad.GSDSpline.mNodes[13].bIsBridgeEnd = true;
            tRoad.GSDSpline.mNodes[13].bIsBridgeMatched = true;

            //Bridge3: (Causeway)
            tRoad.GSDSpline.mNodes[14].bIsBridgeStart = true;
            tRoad.GSDSpline.mNodes[14].bIsBridgeMatched = true;
            tRoad.GSDSpline.mNodes[16].bIsBridgeEnd = true;
            tRoad.GSDSpline.mNodes[16].bIsBridgeMatched = true;
            tRoad.GSDSpline.mNodes[14].BridgeCounterpartNode = tRoad.GSDSpline.mNodes[16];
            tRoad.GSDSpline.mNodes[14].LoadWizardObjectsFromLibrary("Causeway4-2L", true, true);
        }

        /// <summary>
        /// Create 2L intersections:
        /// </summary>
        private static void RoadArchitectUnitTest2()
        {
            //Create node locations:
            var StartLocX = 800f;
            var StartLocY = 200f;
            var StartLocYSep = 200f;
            var tHeight = 20f;
            GSDRoad bRoad = null;
            if (bRoad == null)
            {
            } //Buffer

            GSDRoad tRoad = null;
            if (tRoad == null)
            {
            } //Buffer

            //Create base road:
            var nodeLocations = new List<Vector3>();
            for (var i = 0; i < 9; i++) nodeLocations.Add(new Vector3(StartLocX + i * 200f, tHeight, 600f));
            bRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);

            //Get road system, create road #1:
            nodeLocations.Clear();
            for (var i = 0; i < 5; i++)
                nodeLocations.Add(new Vector3(StartLocX, tHeight, StartLocY + i * StartLocYSep));
            tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            //UnitTest_IntersectionHelper(bRoad, tRoad, GSDRoadIntersection.iStopTypeEnum.TrafficLight1, GSDRoadIntersection.RoadTypeEnum.NoTurnLane);

            //Get road system, create road #2:
            nodeLocations.Clear();
            for (var i = 0; i < 5; i++)
                nodeLocations.Add(new Vector3(StartLocX + StartLocYSep * 2f, tHeight, StartLocY + i * StartLocYSep));
            tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            //UnitTest_IntersectionHelper(bRoad, tRoad, GSDRoadIntersection.iStopTypeEnum.TrafficLight1, GSDRoadIntersection.RoadTypeEnum.TurnLane);

            //Get road system, create road #3:
            nodeLocations.Clear();
            for (var i = 0; i < 5; i++)
                nodeLocations.Add(new Vector3(StartLocX + StartLocYSep * 4f, tHeight, StartLocY + i * StartLocYSep));
            tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            //UnitTest_IntersectionHelper(bRoad, tRoad, GSDRoadIntersection.iStopTypeEnum.TrafficLight1, GSDRoadIntersection.RoadTypeEnum.BothTurnLanes);

            //Get road system, create road #4:
            nodeLocations.Clear();
            for (var i = 0; i < 5; i++)
                nodeLocations.Add(new Vector3(StartLocX + StartLocYSep * 6f, tHeight, StartLocY + i * StartLocYSep));
            tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            //UnitTest_IntersectionHelper(bRoad, tRoad, GSDRoadIntersection.iStopTypeEnum.TrafficLight1, GSDRoadIntersection.RoadTypeEnum.TurnLane);

            //Get road system, create road #4:
            nodeLocations.Clear();
            for (var i = 0; i < 5; i++)
                nodeLocations.Add(new Vector3(StartLocX + StartLocYSep * 8f, tHeight, StartLocY + i * StartLocYSep));
            tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            //UnitTest_IntersectionHelper(bRoad, tRoad, GSDRoadIntersection.iStopTypeEnum.TrafficLight1, GSDRoadIntersection.RoadTypeEnum.TurnLane);

            GSDRoadAutomation.CreateIntersections_ProgrammaticallyForRoad(bRoad, GSDRoadIntersection.iStopTypeEnum.None,
                GSDRoadIntersection.RoadTypeEnum.NoTurnLane);

            //Now count road intersections, if not 5 throw error
            var iCount = 0;
            foreach (var tNode in bRoad.GSDSpline.mNodes)
                if (tNode.bIsIntersection)
                    iCount += 1;
            if (iCount != 5)
                Debug.LogError("Unit Test #2 failed: " + iCount.ToString() + " intersections instead of 5.");
        }

        /// <summary>
        /// This will create an intersection if two nodes overlap on the road. Only good if the roads only overlap once.
        /// </summary>
        /// <param name="bRoad"></param>
        /// <param name="tRoad"></param>
        private static void UnitTest_IntersectionHelper(GSDRoad bRoad, GSDRoad tRoad,
            GSDRoadIntersection.iStopTypeEnum iStopType, GSDRoadIntersection.RoadTypeEnum rType)
        {
            GSDSplineN tInter1 = null;
            GSDSplineN tInter2 = null;
            foreach (var tNode in bRoad.GSDSpline.mNodes)
            foreach (var xNode in tRoad.GSDSpline.mNodes)
                if (GSDRootUtil.IsApproximately(Vector3.Distance(tNode.transform.position, xNode.transform.position),
                    0f, 0.05f))
                {
                    tInter1 = tNode;
                    tInter2 = xNode;
                    break;
                }

            if (tInter1 != null && tInter2 != null)
            {
                var tInter = GSDIntersections.CreateIntersection(tInter1, tInter2);
                var GSDRI = tInter.GetComponent<GSDRoadIntersection>();
                GSDRI.iStopType = iStopType;
                GSDRI.rType = rType;
            }
        }

        /// <summary>
        /// Create 4L intersections:
        /// </summary>
        private static void RoadArchitectUnitTest3()
        {
            //Create node locations:
            var StartLocX = 200f;
            var StartLocY = 2500f;
            var StartLocYSep = 300f;
            var tHeight = 20f;
            GSDRoad bRoad; //Buffer
            GSDRoad tRoad; //Buffer

            //Create base road:
            var nodeLocations = new List<Vector3>();
            for (var i = 0; i < 9; i++)
                nodeLocations.Add(new Vector3(StartLocX + i * StartLocYSep, tHeight, StartLocY + StartLocYSep * 2f));
            bRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            bRoad.opt_Lanes = 4;


            //Get road system, create road #1:
            nodeLocations.Clear();
            for (var i = 0; i < 5; i++)
                nodeLocations.Add(new Vector3(StartLocX, tHeight, StartLocY + i * StartLocYSep));
            tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            tRoad.opt_Lanes = 4;
            UnitTest_IntersectionHelper(bRoad, tRoad, GSDRoadIntersection.iStopTypeEnum.TrafficLight1,
                GSDRoadIntersection.RoadTypeEnum.NoTurnLane);

            //Get road system, create road #2:
            nodeLocations.Clear();
            for (var i = 0; i < 5; i++)
                nodeLocations.Add(new Vector3(StartLocX + StartLocYSep * 2f, tHeight, StartLocY + i * StartLocYSep));
            tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            tRoad.opt_Lanes = 4;
            UnitTest_IntersectionHelper(bRoad, tRoad, GSDRoadIntersection.iStopTypeEnum.TrafficLight1,
                GSDRoadIntersection.RoadTypeEnum.NoTurnLane);

            //Get road system, create road #3:
            nodeLocations.Clear();
            for (var i = 0; i < 5; i++)
                nodeLocations.Add(new Vector3(StartLocX + StartLocYSep * 4f, tHeight, StartLocY + i * StartLocYSep));
            tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            tRoad.opt_Lanes = 4;
            UnitTest_IntersectionHelper(bRoad, tRoad, GSDRoadIntersection.iStopTypeEnum.TrafficLight1,
                GSDRoadIntersection.RoadTypeEnum.NoTurnLane);

            //Get road system, create road #4:
            nodeLocations.Clear();
            for (var i = 0; i < 5; i++)
                nodeLocations.Add(new Vector3(StartLocX + StartLocYSep * 6f, tHeight, StartLocY + i * StartLocYSep));
            tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            tRoad.opt_Lanes = 4;
            UnitTest_IntersectionHelper(bRoad, tRoad, GSDRoadIntersection.iStopTypeEnum.TrafficLight1,
                GSDRoadIntersection.RoadTypeEnum.NoTurnLane);

            //Get road system, create road #5:
            nodeLocations.Clear();
            for (var i = 0; i < 5; i++)
                nodeLocations.Add(new Vector3(StartLocX + StartLocYSep * 8f, tHeight, StartLocY + i * StartLocYSep));
            tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
            tRoad.opt_Lanes = 4;
            UnitTest_IntersectionHelper(bRoad, tRoad, GSDRoadIntersection.iStopTypeEnum.TrafficLight1,
                GSDRoadIntersection.RoadTypeEnum.NoTurnLane);

            //Now count road intersections, if not 5 throw error
            var iCount = 0;
            foreach (var tNode in bRoad.GSDSpline.mNodes)
                if (tNode.bIsIntersection)
                    iCount += 1;
            if (iCount != 5)
                Debug.LogError("Unit Test #3 failed: " + iCount.ToString() + " intersections instead of 5.");
        }

        //Large suspension bridge:
        private static void RoadArchitectUnitTest4()
        {
            //Create base road:
            var nodeLocations = new List<Vector3>();
            for (var i = 0; i < 5; i++) nodeLocations.Add(new Vector3(3500f, 90f, 200f + 800f * i));
            var tRoad = GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);

            //Suspension bridge:
            tRoad.GSDSpline.mNodes[1].bIsBridgeStart = true;
            tRoad.GSDSpline.mNodes[1].bIsBridgeMatched = true;
            tRoad.GSDSpline.mNodes[3].bIsBridgeEnd = true;
            tRoad.GSDSpline.mNodes[3].bIsBridgeMatched = true;
            tRoad.GSDSpline.mNodes[1].BridgeCounterpartNode = tRoad.GSDSpline.mNodes[3];
            tRoad.GSDSpline.mNodes[1].LoadWizardObjectsFromLibrary("SuspL-2L", true, true);
        }

        //Long road
        private static void RoadArchitectUnitTest5()
        {
            //Create base road:
            var nodeLocations = new List<Vector3>();
            for (var i = 0; i < 48; i++) nodeLocations.Add(new Vector3(3000f, 40f, 10f + 79f * i));
            for (var i = 0; i < 35; i++) nodeLocations.Add(new Vector3(2900f - 79f * i, 30f, 3960f));
            for (var i = 0; i < 40; i++) nodeLocations.Add(new Vector3(30, 30f, 3960f - 79f * i));
            GSDRoadAutomation.CreateRoad_Programmatically(RoadSystem, ref nodeLocations);
        }
    }
#endif
}