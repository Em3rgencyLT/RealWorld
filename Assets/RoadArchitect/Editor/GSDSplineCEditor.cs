#region "Imports"

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GSDSplineC))]

#endregion

public class GSDSplineCEditor : Editor
{
    protected GSDSplineC tSpline => (GSDSplineC) target;

    public override void OnInspectorGUI()
    {
    }
}