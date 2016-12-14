using UnityEngine;
using UnityEditor;

public enum DirectionEnum { Down, Up, Left, Right, Forward, Back }

public class GroundObjectsTool : EditorWindow
{
    private static GameObject[] selectedObjects;
    private static Vector3[] groundedPositions;
    private static Quaternion[] groundedRotations;
    private static Mesh[] groundedMeshes;
    private static bool[] foundGround;

    private Vector3 groundDir = Vector3.down;

    private DirectionEnum groundInDirection = DirectionEnum.Down;

    private static bool showGroundedPreview = true;
    private bool deselectObjectsOnFinish = true;

    private bool corectPositionsToGrid = false;

    private static float noMeshCircleRadius = 1.0f;

    [MenuItem("ZoonTools/Ground Objects", false, 103)]
    private static void Init()
    {
        GroundObjectsTool window = (GroundObjectsTool)GetWindow(typeof(GroundObjectsTool));
        window.Show();
    }

    private void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;

        selectedObjects = Selection.gameObjects;

        UpdatePreviews();
    }

    private void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;

        ClearArrays();
    }

    private void OnInspectorUpdate()
    {
        Repaint();

        switch (groundInDirection)
        {
            case DirectionEnum.Up:
                groundDir = Vector3.up;
                break;

            case DirectionEnum.Down:
                groundDir = Vector3.down;
                break;

            case DirectionEnum.Left:
                groundDir = Vector3.left;
                break;

            case DirectionEnum.Right:
                groundDir = Vector3.right;
                break;

            case DirectionEnum.Forward:
                groundDir = Vector3.forward;
                break;

            case DirectionEnum.Back:
                groundDir = Vector3.back;
                break;

            default:
                groundDir = Vector3.down;
                break;
        }

        UpdatePreviews();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("This tool allows for easy placement of objects on surfaces.");
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        showGroundedPreview = EditorGUILayout.Toggle(new GUIContent("Show Preview", "When this option is on a preview of the grounded objects objects will be shown in the Scene View."), showGroundedPreview);

        deselectObjectsOnFinish = EditorGUILayout.Toggle(new GUIContent("Deselect on finish", "When objects are grounded they are automatically deselected in the editor."), deselectObjectsOnFinish);

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

        EditorGUILayout.Separator();

        groundInDirection = (DirectionEnum)EditorGUILayout.EnumPopup(new GUIContent("Ground Direction", "The direction in world space the objects will be grounded."), groundInDirection, EditorStyles.popup);

        if(EditorGUI.EndChangeCheck())
        {
            UpdatePreviews();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button(new GUIContent("Ground Objects", "Grounds all selected objects that can be grounded in the specified direction.")))
        {
            GroundObjects();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        switch (e.type)
        {
            case EventType.MouseDrag:

                if (selectedObjects.Length > 0)
                {
                    UpdatePreviews();
                }
                break;

                    default:
                break;
        }
    }

    private void OnSelectionChange()
    {
        selectedObjects = Selection.gameObjects;

        UpdatePreviews();
    }

    private void UpdatePreviews()
    {
        groundedPositions = new Vector3[selectedObjects.Length];
        groundedRotations = new Quaternion[selectedObjects.Length];
        groundedMeshes = new Mesh[selectedObjects.Length];
        foundGround = new bool[selectedObjects.Length];

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            foundGround[i] = false;

            RaycastHit hit;

            if (Physics.Raycast(selectedObjects[i].transform.position, groundDir, out hit, Mathf.Infinity))
            {
                if (hit.collider != null && hit.collider.gameObject != selectedObjects[i])
                {
                    foundGround[i] = true;
                    Quaternion oldRot= selectedObjects[i].transform.rotation;
                    selectedObjects[i].transform.up = hit.normal;

                    groundedRotations[i] = selectedObjects[i].transform.rotation;
                    groundedPositions[i] = hit.point;

                    if (corectPositionsToGrid && GridTool.EnableGridTool)
                    {
                        Vector3 onGrid = GridTool.SnapToGrid(groundedPositions[i]);

                        onGrid.y = groundedPositions[i].y;
                        groundedPositions[i] = onGrid;

                        if (Physics.Raycast(groundedPositions[i] - groundDir, groundDir, out hit, 2))
                        {
                            if (hit.collider != null && hit.collider.gameObject != selectedObjects[i])
                            {
                                selectedObjects[i].transform.up = hit.normal;
                                groundedRotations[i] = selectedObjects[i].transform.rotation;
                            }
                        }
                    }

                    selectedObjects[i].transform.rotation = oldRot;

                    try
                    {
                        groundedMeshes[i] = selectedObjects[i].GetComponent<MeshFilter>().sharedMesh;
                    }
                    catch
                    {
                        groundedMeshes[i] = new Mesh();
                    }
                }
            }
        }
    }

    private void GroundObjects()
    {
        if (selectedObjects != null && groundedPositions != null && groundedRotations != null)
        {
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Undo.RecordObject(selectedObjects[i].transform, "ObjectGrounded" + i);

                if (foundGround[i])
                {
                    selectedObjects[i].transform.position = groundedPositions[i];
                    EditorUtility.SetDirty(selectedObjects[i].transform);
                }

                if (foundGround[i])
                {
                    selectedObjects[i].transform.rotation = groundedRotations[i];
                    EditorUtility.SetDirty(selectedObjects[i].transform);
                }
            }            
        }

        if (deselectObjectsOnFinish)
        {
            Selection.objects = new Object[0];
        }
    }

    private void ClearArrays()
    {
        selectedObjects = null;
        groundedPositions = null;
        groundedRotations = null;
        groundedMeshes = null;
    }

    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    private static void DrawGizmos(Transform transform, GizmoType gizmoType)
    {
        if (selectedObjects != null && showGroundedPreview)
        {
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                if (groundedMeshes != null && groundedMeshes[i] != null)
                {
                    Gizmos.color = new Color(1, 0, 0, 0.3f);
                    Gizmos.DrawLine(selectedObjects[i].transform.position, groundedPositions[i]);

                    if (groundedMeshes[i] == new Mesh())
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(groundedPositions[i], noMeshCircleRadius);

                        Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.3f);
                        Gizmos.DrawSphere(groundedPositions[i], noMeshCircleRadius);
                    }
                    else
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireMesh(groundedMeshes[i], groundedPositions[i], groundedRotations[i]);

                        Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.3f);
                        Gizmos.DrawMesh(groundedMeshes[i], groundedPositions[i], groundedRotations[i]);
                    }
                }
            }
        }
    }
}
