using UnityEngine;
using UnityEditor;
using System.Collections;

public enum DirectionEnum { Up, Down, Left, Right, Forward, Back }

public class GroundObjectsTool : EditorWindow
{
    static private GameObject[] objects;

    private DirectionEnum allignWithObjectVector = DirectionEnum.Up;

    static private bool showGroundedPreview = true;
    private bool checkAboveObject = false;
    private bool attempToPlaceOnGroundAbove = false;

    private bool corectPositionsToGrid = false;

    [MenuItem("ZoonTools/Ground Objects", false, 103)]
    private static void Init()
    {
        GroundObjectsTool window = (GroundObjectsTool)GetWindow(typeof(GroundObjectsTool));
        window.Show();
    }

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChange;

        objects = Selection.gameObjects;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChange;
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("This tool allows for easy placement of objects on surfaces.");
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        showGroundedPreview = EditorGUILayout.Toggle(new GUIContent("Show Preview", "When this option is on a preview of the grounded objects objects will be shown in the Scene View."), showGroundedPreview);

        checkAboveObject = EditorGUILayout.Toggle(new GUIContent("Check Above Object", "The tool always looks for ground under the object. This option will allow you to check above the object too."), checkAboveObject);

        if (checkAboveObject)
        {
            attempToPlaceOnGroundAbove = EditorGUILayout.Toggle(new GUIContent("Place On Ceiling", "Places the objects upside down on the ceiling if no ground could be found."), attempToPlaceOnGroundAbove);
        }
        else
        {
            Color oldColor = GUI.color;
            GUI.color = Color.gray;

            EditorGUILayout.LabelField(new GUIContent("Place On Ceiling", "Turn on 'Check Above Object' to enable this option."));
            attempToPlaceOnGroundAbove = false;

            GUI.color = oldColor;
        }


        if (GridTool.EnableGridTool)
        {
            corectPositionsToGrid = EditorGUILayout.Toggle(new GUIContent("Align with Grid", "If the grid is turned on the objects will find the nearest position that is on the grid."), corectPositionsToGrid);
        }
        else
        {
            Color oldColor = GUI.color;
            GUI.color = Color.gray;

            EditorGUILayout.LabelField(new GUIContent("Align with Grid", "Activate the grid to enable this option."));
            corectPositionsToGrid = false;

            GUI.color = oldColor;
        }

        allignWithObjectVector = (DirectionEnum)EditorGUILayout.EnumPopup(new GUIContent("Face Up", "The local direction vector of the object that will face away form the surface."), allignWithObjectVector, EditorStyles.popup);
    }

    private void OnSelectionChange()
    {
        objects = Selection.gameObjects;
    }

    private void GroundObjects()
    {

    }

    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    private static void DrawGizmos(Transform transform, GizmoType gizmoType)
    {
        if (showGroundedPreview)
        {
            

            for (int i = 0; i < objects.Length; i++)
            {
                RaycastHit hit;

                if (Physics.Raycast(new Ray(objects[i].transform.position, Vector3.down), out hit, Mathf.Infinity))
                {
                    if (hit.transform.gameObject != objects[i])
                    {
                        Mesh mesh = null;

                        try
                        {
                            Debug.Log("SDASDSAD");

                            mesh = objects[i].GetComponent<MeshFilter>().sharedMesh;

                            Gizmos.color = new Color(1, 0, 0, 0.5f);
                            Gizmos.DrawLine(objects[i].transform.position, hit.point);

                            Gizmos.color = new Color(0, 0, 0, 0.5f);
                            Gizmos.DrawMesh(mesh, hit.point, Quaternion.LookRotation(objects[i].transform.forward ,hit.normal));
                        }
                        catch
                        {
                            Debug.Log("SDASDSAD");

                            Gizmos.color = new Color(1, 0, 0, 0.5f);
                            Gizmos.DrawLine(objects[i].transform.position, hit.point);

                            Gizmos.color = new Color(0, 0, 0, 0.5f);
                            Gizmos.DrawSphere(hit.point, 1);
                        }
                    }
                }
                
            }
        }
    }
}
