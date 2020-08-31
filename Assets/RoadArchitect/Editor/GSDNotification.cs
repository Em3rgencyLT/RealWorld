using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Used for notifications in other areas of RA.
/// </summary>
public class GSDNotification : EditorWindow
{
    private string notification = "This is a Notification";

    private static void Initialize()
    {
        var window = GetWindow<GSDNotification>();
        window.Show();
    }

    private void OnGUI()
    {
        notification = EditorGUILayout.TextField(notification);
        if (GUILayout.Button("Show Notification")) ShowNotification(new GUIContent(notification));
        if (GUILayout.Button("Remove Notification")) RemoveNotification();
    }
}