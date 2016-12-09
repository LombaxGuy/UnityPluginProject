using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class CordonToolWindow : EditorWindow {

    Cordon cordon;

    Vector3 mousePosition;

    List<GameObject> hiddenObjects = new List<GameObject>();

    int selectedIndex = -1;

    [MenuItem("ZoonTools/Cordon Tool", false, 101)]
    static void Init()
    {
        CordonToolWindow window = (CordonToolWindow)EditorWindow.GetWindow(typeof(CordonToolWindow));
        window.Show();
    }

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }

    void OnSceneGUI(SceneView scene)
    {
        if (cordon)
        {
            for (int i = 0; i < cordon.points.Length; i++)
            {
                ShowPoint(i);
            }

            Handles.color = Color.red;
            Handles.DrawWireCube(cordon.transform.TransformPoint(cordon.bounds.center), cordon.bounds.size);
        }

        HandleUtility.Repaint();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnGUI()
    {
        cordon = (Cordon)EditorGUILayout.ObjectField("Cordon: ", cordon, typeof(Cordon), true);

        if (GUILayout.Button("Hide objects outside Cordon"))
        {
            HideObjects();
        }

        if (GUILayout.Button("Unhide objects"))
        {
            UnHideObjects();
        }
    }

    void ShowPoint(int index)
    {
        Vector3 point = cordon.transform.TransformPoint(cordon.points[index]);

        Handles.color = Color.white;

        if(Handles.Button(point, Quaternion.identity, 0.04f, 0.06f, Handles.DotCap))
        {
            selectedIndex = index;
        }

        if(selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();

            point = Handles.DoPositionHandle(point, Quaternion.identity);

            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(cordon, "Move point.");
                EditorUtility.SetDirty(cordon);

                cordon.points[index] = cordon.transform.InverseTransformPoint(point);

                cordon.RecalculateBounds();
                cordon.RecalculatePoints();
            }
        }
    }

    void HideObjects()
    {
        Object[] objList = Resources.FindObjectsOfTypeAll(typeof(GameObject));

        GameObject tmpObj;

        foreach (Object obj in objList)
        {
            if (obj is GameObject)
            {
                tmpObj = (GameObject)obj;

                if (!cordon.bounds.bounds.Contains(tmpObj.transform.position) &&
                    tmpObj.hideFlags == HideFlags.None)
                {
                    if (tmpObj.GetComponent<Light>() == null && 
                        tmpObj.GetComponent<Camera>() == null)
                    {
                        hiddenObjects.Add((GameObject)obj);
                    }
                }
            }
        }

        if (hiddenObjects.Count > 0)
        {
            foreach (GameObject go in hiddenObjects)
            {
                go.SetActive(false);
                go.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        EditorApplication.RepaintHierarchyWindow();
    }

    void UnHideObjects()
    {
        if (hiddenObjects.Count > 0)
        {
            foreach (GameObject go in hiddenObjects)
            {
                go.SetActive(true);
                go.hideFlags = HideFlags.None;
            }
        }

        EditorApplication.RepaintHierarchyWindow();
    }
}
