using UnityEngine;
using UnityEditor;

// Enum used for the direction dropdown menu
public enum DirectionEnum { Down, Up, Left, Right, Forward, Back }

public class GroundObjectsTool : EditorWindow
{
    // Arrays with different values. All arrays have their objects ordered in the same way eg. index 1 in groundedMeshes is the mesh of the object at index 1 in selectedObjects.
    private static GameObject[] selectedObjects;
    private static Vector3[] groundedPositions;
    private static Quaternion[] groundedRotations;
    private static Mesh[] groundedMeshes;
    private static bool[] foundGround;

    // The direction the tool tries to ground objects in.
    private Vector3 groundDir = Vector3.down;
    // Same as above. This is used only to control the dorpdown menu.
    private DirectionEnum groundInDirection = DirectionEnum.Down;

    // Show a preview of the objects before the button is pressed?
    private static bool showGroundedPreview = true;
    // Deselect all selected objects when the button is pressed?
    private bool deselectObjectsOnFinish = true;

    // Takes the two axis that are not towards ground and corects them to the grid.
    private bool corectPositionsToGrid = false;

    // The radius of the circle that is draw if the object has no mesh.
    private static float noMeshCircleRadius = 1.0f;

    // Creates a menu point.
    [MenuItem("ZoonTools/Ground Objects", false, 103)]
    private static void Init()
    {
        // Opens the window.
        GroundObjectsTool window = (GroundObjectsTool)GetWindow(typeof(GroundObjectsTool));
        window.Show();
    }

    /// <summary>
    /// Called when the window is opened
    /// </summary>
    private void OnEnable()
    {
        // Subscribes to onSceneGUIDelegate
        SceneView.onSceneGUIDelegate += OnSceneGUI;

        // Updates the selectedObjects array
        selectedObjects = Selection.gameObjects;

        // Updates the previews
        UpdatePreviews();
    }

    /// <summary>
    /// Called when the window is closed
    /// </summary>
    private void OnDisable()
    {
        // Unsubscribes from onSceneGUIDelegate
        SceneView.onSceneGUIDelegate -= OnSceneGUI;

        // Clears all the arrays
        ClearArrays();
    }

    /// <summary>
    /// Called when something in the editor window is updated.
    /// </summary>
    private void OnInspectorUpdate()
    {
        // Repaint the window
        Repaint();

        // Sets the groundDir based on groundInDirection
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

        // Updates the previews
        UpdatePreviews();
    }

    /// <summary>
    /// Used to draw GUI
    /// </summary>
    private void OnGUI()
    {
        // Description of the tool.
        EditorGUILayout.LabelField("This tool allows for easy placement of objects on surfaces.");

        // Title of following section.
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        // Begins a change check.
        EditorGUI.BeginChangeCheck();

        // Draws the Show preview toggle.
        showGroundedPreview = EditorGUILayout.Toggle(new GUIContent("Show Preview", "When this option is on a preview of the grounded objects objects will be shown in the Scene View."), showGroundedPreview);

        // Darws the Deselect on finish toggle.
        deselectObjectsOnFinish = EditorGUILayout.Toggle(new GUIContent("Deselect on finish", "When objects are grounded they are automatically deselected in the editor."), deselectObjectsOnFinish);

        // If the grid is enabled...
        if (GridTool.EnableGridTool)
        {
            //... the Align with Grid toggle is draw.
            corectPositionsToGrid = EditorGUILayout.Toggle(new GUIContent("Align with Grid", "If the grid is turned on the objects will find the nearest position that is on the grid."), corectPositionsToGrid);
        }
        // If the grid is not enabled...
        else
        {
            Color oldColor = GUI.color;
            GUI.color = Color.gray;

            // Draw some text
            EditorGUILayout.LabelField(new GUIContent("Align with Grid", "Activate the grid to enable this option."));
            corectPositionsToGrid = false;

            GUI.color = oldColor;
        }

        // Makes some empty space
        EditorGUILayout.Separator();

        // Popup-menu for groundInDirection 
        groundInDirection = (DirectionEnum)EditorGUILayout.EnumPopup(new GUIContent("Ground Direction", "The direction in world space the objects will be grounded."), groundInDirection, EditorStyles.popup);

        // If any of the above setting are changed...
        if(EditorGUI.EndChangeCheck())
        {
            //... the previews are updated.
            UpdatePreviews();
        }

        // Makes som empty space
        EditorGUILayout.Separator();

        // Creates the Ground Objects button. If the button is pressed the GroundObjects method is called.
        if (GUILayout.Button(new GUIContent("Ground Objects", "Grounds all selected objects that can be grounded in the specified direction.")))
        {
            GroundObjects();
        }
    }

    /// <summary>
    /// Used to handle various events in the sceneView
    /// </summary>
    /// <param name="sceneView">The sceneView</param>
    private void OnSceneGUI(SceneView sceneView)
    {
        // The current event
        Event e = Event.current;

        switch (e.type)
        {
            // If the current event is a mouse drag event and the selectedObjects array has any elements...
            case EventType.MouseDrag:

                if (selectedObjects.Length > 0)
                {
                    //... the previews are updated.
                    UpdatePreviews();
                }
                break;

                    default:
                break;
        }
    }

    /// <summary>
    /// Called when the selection in the editor changes.
    /// </summary>
    private void OnSelectionChange()
    {
        // Sets the selectedObjects array to the currently selected gameobjects.
        selectedObjects = Selection.gameObjects;

        // Updates the previews.
        UpdatePreviews();
    }

    /// <summary>
    /// Used to update the previews and the arrays used to draw them.
    /// </summary>
    private void UpdatePreviews()
    {
        // Sets the length of the arrays.
        groundedPositions = new Vector3[selectedObjects.Length];
        groundedRotations = new Quaternion[selectedObjects.Length];
        groundedMeshes = new Mesh[selectedObjects.Length];
        foundGround = new bool[selectedObjects.Length];

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            // Default value. This remains false if no ground was found.
            foundGround[i] = false;

            RaycastHit hit;

            // Creates a raycast from the position of the selected object in the direction of groundDir. 
            if (Physics.Raycast(selectedObjects[i].transform.position, groundDir, out hit, Mathf.Infinity))
            {
                // If the ray hits something that is not the current gameobject
                if (hit.collider != null && hit.collider.gameObject != selectedObjects[i])
                {
                    // Ground has been found
                    foundGround[i] = true;

                    // Saves the old rotation of the object
                    Quaternion oldRot= selectedObjects[i].transform.rotation;

                    // Sets the up vector of the object to the normal of the raycast hit.
                    selectedObjects[i].transform.up = hit.normal;

                    // Sets the values in these two arrays.
                    groundedRotations[i] = selectedObjects[i].transform.rotation;
                    groundedPositions[i] = hit.point;

                    // If the corectPositionsToGrid is true and the grid is enabled
                    if (corectPositionsToGrid && GridTool.EnableGridTool)
                    {
                        // The "On Grid" position is calculated and saved.
                        Vector3 onGrid = GridTool.SnapToGrid(groundedPositions[i]);
                      
                        // Finds out if x, y or z should not be on grid
                        if (groundInDirection == DirectionEnum.Left || groundInDirection == DirectionEnum.Right)
                        {
                            onGrid.x = groundedPositions[i].x;
                        }
                        else if (groundInDirection == DirectionEnum.Up || groundInDirection == DirectionEnum.Down)
                        {
                            onGrid.y = groundedPositions[i].y;
                        }
                        else if (groundInDirection == DirectionEnum.Back || groundInDirection == DirectionEnum.Forward)
                        {
                            onGrid.z = groundedPositions[i].z;
                        }

                        // Sets the groundedPosition of the current object to the new onGrid vector.
                        groundedPositions[i] = onGrid;

                        // Creates a new raycast at the new position to find the rotation here. 
                        if (Physics.Raycast(groundedPositions[i] - groundDir, groundDir, out hit, Mathf.Infinity))
                        {
                            if (hit.collider != null && hit.collider.gameObject != selectedObjects[i])
                            {
                                selectedObjects[i].transform.up = hit.normal;
                                groundedRotations[i] = selectedObjects[i].transform.rotation;
                            }
                        }
                    }

                    // Rotation of the object is reset to the oldRot
                    selectedObjects[i].transform.rotation = oldRot;

                    try
                    {
                        // If the the selectedObject has a mesh filter the shared mesh is stored.
                        groundedMeshes[i] = selectedObjects[i].GetComponent<MeshFilter>().sharedMesh;
                    }
                    catch
                    {
                        // If it has no mesh filter a new mesh is created.
                        groundedMeshes[i] = new Mesh();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Grounds the selected objects.
    /// </summary>
    private void GroundObjects()
    {
        // If non of the arrays are null
        if (selectedObjects != null && groundedPositions != null && groundedRotations != null)
        {
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                // Records the object for an undo action.
                Undo.RecordObject(selectedObjects[i].transform, "ObjectGrounded" + i);

                // If ground has been found for the current object...
                if (foundGround[i])
                {
                    //... The object is grounded and it is marked as dirty for the Undo system.
                    selectedObjects[i].transform.position = groundedPositions[i];
                    EditorUtility.SetDirty(selectedObjects[i].transform);
                }

                // If ground has been found for the current object...
                if (foundGround[i])
                {
                    //... The object is grounded and it is marked as dirty for the Undo system.
                    selectedObjects[i].transform.rotation = groundedRotations[i];
                    EditorUtility.SetDirty(selectedObjects[i].transform);
                }
            }            
        }

        // If deselect on finish was marked the selection is set to none
        if (deselectObjectsOnFinish)
        {
            Selection.objects = new Object[0];
        }
    }

    /// <summary>
    /// Clears all the arrays.
    /// </summary>
    private void ClearArrays()
    {
        selectedObjects = null;
        groundedPositions = null;
        groundedRotations = null;
        groundedMeshes = null;
    }

    /// <summary>
    /// Used to dras previews.
    /// </summary>
    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    private static void DrawGizmos(Transform transform, GizmoType gizmoType)
    {
        // If at least 1 object is selected and the showGroundedPreview is true.
        if (selectedObjects != null && showGroundedPreview)
        {
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                if (groundedMeshes != null && groundedMeshes[i] != null)
                {
                    // Draws a red line form the original position to the new position
                    Gizmos.color = new Color(1, 0, 0, 0.3f);
                    Gizmos.DrawLine(selectedObjects[i].transform.position, groundedPositions[i]);

                    // If the object has no mesh...
                    if (groundedMeshes[i] == new Mesh())
                    {
                        //... a wire sphere and a normal sphere with a radius is draw at the groundedPosition.
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(groundedPositions[i], noMeshCircleRadius);

                        Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.3f);
                        Gizmos.DrawSphere(groundedPositions[i], noMeshCircleRadius);
                    }
                    // If the object has a mesh...
                    else
                    {
                        //... a preview of that specfic mesh is drawn.
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
