using UnityEngine;
using UnityEditor;
using System.Collections;

public class ClickTest : EditorWindow
{
    [MenuItem("ZoonTools/Test", false, 301)]
    static void Init()
    {
        ClickTest window = (ClickTest)EditorWindow.GetWindow(typeof(ClickTest));
        window.Show();
    }

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView scene)
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        Handles.color = Color.red;
        Handles.CubeCap(controlID, Vector3.zero, Quaternion.identity, 1);

        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.Layout:
                HandleUtility.AddControl(controlID, HandleUtility.DistanceToCircle(Vector3.zero, 1));
                break;

            case EventType.MouseDown:
                if (HandleUtility.nearestControl == controlID)
                {
                    Debug.Log("I AM ALIVE!");
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
                break;
        }


        Handles.BeginGUI();

        Handles.EndGUI();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}
