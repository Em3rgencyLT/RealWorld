using RoadArchitect;
using UnityEngine;
using UnityEditor;

public class GSDRoadSystemEditorMenu : ScriptableObject
{
    private const bool bRoadTestCubes = false;

    /// <summary>
    /// Creates the road system.
    /// </summary>
    [MenuItem("Window/Road Architect/Create road system")]
    public static void CreateRoadSystem()
    {
        var tObj = FindObjectsOfType(typeof(GSDRoadSystem));
        var i = tObj.Length + 1;
        tObj = null;

        var tRoadSystemObj = new GameObject("RoadArchitectSystem" + i.ToString());
        var tRoadSystem = tRoadSystemObj.AddComponent<GSDRoadSystem>(); //Add road system component.
        tRoadSystem.AddRoad(true); //Add road for new road system.

        var IntersectionsMasterObject = new GameObject("Intersections");
        IntersectionsMasterObject.transform.parent = tRoadSystemObj.transform;
    }

    /// <summary>
    /// Add road to gameobject. Not sure if this is necessary.
    /// </summary>
    [MenuItem("Window/Road Architect/Add road")]
    public static void AddRoad()
    {
        var tObjs = FindObjectsOfType(typeof(GSDRoadSystem));
        if (tObjs != null && tObjs.Length == 0)
        {
            CreateRoadSystem();
            return;
        }
        else
        {
            var GSDRS = (GSDRoadSystem) tObjs[0];
            Selection.activeGameObject = GSDRS.AddRoad();
        }
    }

    /// <summary>
    /// Updates all roads. Used when things get out of sync.
    /// </summary>
    [MenuItem("Window/Road Architect/Update All Roads")]
    public static void UpdateAllRoads()
    {
        var tRoadObjs = (GSDRoad[]) FindObjectsOfType(typeof(GSDRoad));

        var RoadCount = tRoadObjs.Length;

        GSDRoad tRoad = null;
        GSDSplineC[] tPiggys = null;
        if (RoadCount > 1) tPiggys = new GSDSplineC[RoadCount - 1];

        for (var h = 0; h < RoadCount; h++)
        {
            tRoad = tRoadObjs[h];
            if (h > 0) tPiggys[h - 1] = tRoad.GSDSpline;
        }

        tRoad = tRoadObjs[0];
        if (tPiggys != null && tPiggys.Length > 0) tRoad.PiggyBacks = tPiggys;
        tRoad.UpdateRoad();
    }

    /// <summary>
    /// Show the help screen.
    /// </summary>
    [MenuItem("Window/Road Architect/Help")]
    public static void GSDRoadsHelp()
    {
        var tHelp = EditorWindow.GetWindow<GSDHelpWindow>();
        tHelp.Initialize();
    }

    /// <summary>
    /// WARNING: Only call this on an empty scene that has some terrains on it. MicroGSD LLC is not responsbile for data loss if this function is called by user.
    /// </summary>
    [MenuItem("Window/Road Architect/Testing/Run all unit tests (caution)")]
    public static void TestProgram()
    {
        GSD.Roads.GSDUnitTests.RoadArchitectUnitTests();
    }

    /// <summary>
    /// WARNING: Only call this on an empty scene that has some terrains on it. MicroGSD LLC is not responsbile for data loss if this function is called by user.
    /// </summary>
    [MenuItem("Window/Road Architect/Testing/Clean up tests (caution)")]
    public static void TestCleanup()
    {
        GSD.Roads.GSDUnitTests.CleanupTests();
    }


    /// <summary>
    /// Get code line count for RA project.
    /// </summary>
    [MenuItem("Window/Road Architect/Testing/Get line count of RA")]
    public static void testCodeCount()
    {
        var mainDir = Application.dataPath + "/RoadArchitect/";
        var files = System.IO.Directory.GetFiles(mainDir, "*.cs", System.IO.SearchOption.AllDirectories);
        var lineCount = 0;
        foreach (var s in files) lineCount += System.IO.File.ReadAllLines(s).Length;
        Debug.Log(string.Format("{0:n0}", lineCount) + " lines of code in Road Architect.");
    }
}