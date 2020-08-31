using UnityEngine;
#if UNITY_EDITOR

#endif

namespace RoadArchitect
{
    public class GSDRoadSystem : MonoBehaviour
    {
#if UNITY_EDITOR

        public bool opt_bMultithreading = true;
        public bool opt_bSaveMeshes = false;
        public bool opt_bAllowRoadUpdates = true;

        public GameObject AddRoad(bool bForceSelect = false, string name = null)
        {
            var tObj = FindObjectsOfType(typeof(GSDRoad));
            var NewRoadNumber = tObj.Length + 1;
            if (name == null) name = "Road" + NewRoadNumber.ToString();

            //Road:
            var tRoadObj = new GameObject(name);
            UnityEditor.Undo.RegisterCreatedObjectUndo(tRoadObj, "Created road");
            tRoadObj.transform.parent = transform;
            var tRoad = tRoadObj.AddComponent<GSDRoad>();

            //Spline:
            var tSplineObj = new GameObject("Spline");
            tSplineObj.transform.parent = tRoad.transform;
            tRoad.GSDSpline = tSplineObj.AddComponent<GSDSplineC>();
            tRoad.GSDSpline.mSplineRoot = tSplineObj;
            tRoad.GSDSpline.tRoad = tRoad;
            tRoad.GSDSplineObj = tSplineObj;
            tRoad.GSDRS = this;
            tRoad.SetupUniqueIdentifier();

            tRoad.ConstructRoad_ResetTerrainHistory();

            if (bForceSelect) UnityEditor.Selection.activeGameObject = tRoadObj;

            return tRoadObj;
        }

        public Camera EditorPlayCamera = null;

        public void EditorCameraSetSingle()
        {
            if (EditorPlayCamera == null)
            {
                var EditorCams = (Camera[]) FindObjectsOfType(typeof(Camera));
                if (EditorCams != null && EditorCams.Length == 1) EditorPlayCamera = EditorCams[0];
            }
        }

        public void UpdateAllRoads()
        {
            var tRoadObjs = (GSDRoad[]) FindObjectsOfType(typeof(GSDRoad));
//		int i=0;

            var RoadCount = tRoadObjs.Length;

            GSDRoad tRoad = null;
            GSDSplineC[] tPiggys = null;
            if (RoadCount > 1)
            {
                tPiggys = new GSDSplineC[RoadCount];
                for (var h = 0; h < RoadCount; h++)
                {
                    tRoad = tRoadObjs[h];
                    tPiggys[h] = tRoad.GSDSpline;
                }
            }

            tRoad = tRoadObjs[0];
            if (tPiggys != null && tPiggys.Length > 0) tRoad.PiggyBacks = tPiggys;
            tRoad.UpdateRoad();
        }

        //Workaround for submission rules:
        public void UpdateAllRoads_MultiThreadOptions()
        {
            var tRoadObjs = (GSDRoad[]) FindObjectsOfType(typeof(GSDRoad));
            var RoadCount = tRoadObjs.Length;
            GSDRoad tRoad = null;
            for (var h = 0; h < RoadCount; h++)
            {
                tRoad = tRoadObjs[h];
                if (tRoad != null) tRoad.opt_bMultithreading = opt_bMultithreading;
            }
        }

        //Workaround for submission rules:
        public void UpdateAllRoads_SaveMeshesAsAssetsOptions()
        {
            var tRoadObjs = (GSDRoad[]) FindObjectsOfType(typeof(GSDRoad));
            var RoadCount = tRoadObjs.Length;
            GSDRoad tRoad = null;
            for (var h = 0; h < RoadCount; h++)
            {
                tRoad = tRoadObjs[h];
                if (tRoad != null) tRoad.opt_bSaveMeshes = opt_bSaveMeshes;
            }
        }
#endif
    }
}