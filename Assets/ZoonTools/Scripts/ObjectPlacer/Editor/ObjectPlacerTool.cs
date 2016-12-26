using UnityEngine;
using UnityEditor;
using System.Collections;

public class ObjectPlacerTool : EditorWindow
{
    // Private enum used for choosing the snapping plane
    private enum SnapDirections { XY, XZ, YZ };

    private GameObject selectedObject = null;
    private static bool placementEnabled = false;

    private Vector3 mousePosition;

    private static Mesh previewMesh;
    private static Vector3 previewPosition;
    private static Quaternion previewRotation;
    private static float noMeshCircleRadius = 0.5f;

    private float distanceToObject = 10.0f;

    private bool keepNativeRotation = false;
    private bool randomizeLocalYRotation = false;
    private float currentRandomRotation = 0;
    private float localYRotation = 0;

    private bool snapToGrid = false;
    private SnapDirections snappingPlane = SnapDirections.XZ;

    /// <summary>
    /// Creates a menu point and opens the window when the menu point is clicked.
    /// </summary>
    [MenuItem("ZoonTools/Object Placer", false, 102)]
    static void Init()
    {
        ObjectPlacerTool window = (ObjectPlacerTool)GetWindow(typeof(ObjectPlacerTool));
        window.Show();
    }

    /// <summary>
    /// Called when the window is opened.
    /// </summary>
    private void OnEnable()
    {
        // Subscribe the method OnSceneGUI to the SceneView.onSceneGUIDelegate delegate.
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    /// <summary>
    /// Called when the window is closed.
    /// </summary>
    private void OnDisable()
    {
        // Unsubscribes the method OnSceneGUI from the SceneView.onSceneGUIDelegate delegate.
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        placementEnabled = false;
    }

    /// <summary>
    /// Used to handle events in the sceneview.
    /// </summary>
    private void OnSceneGUI(SceneView sceneView)
    {
        // The current event
        Event e = Event.current;

        // The mouse position on the screne to a world ray.
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        // If placement is enabled...
        if (placementEnabled)
        {
            //... and the left mouse button is pressed...
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                //... Place an object
                PlaceObject();

                // Create a new random rotation if randomize rotation is active.
                if (randomizeLocalYRotation)
                {
                    currentRandomRotation = Random.Range(0, 360);

                    // Updates the preview
                    UpdatePreview(ray);
                }
            }

            // If the mouse is moved the preview is updated
            if (e.type == EventType.MouseMove)
            {
                UpdatePreview(ray);
            }
        }
    }

    /// <summary>
    /// Updates the preview ghost.
    /// </summary>
    /// <param name="ray">Mouse position to world ray.</param>
    private void UpdatePreview(Ray ray)
    {
        // Repaints the sceneview.
        SceneView.RepaintAll();

        RaycastHit hit;

        // If the ray hits anything in the direction the mouse was pointing
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            mousePosition = hit.point;

            // If the grid is enabled and snapToGrid is active
            if (GridTool.EnableGridTool && GridTool.SnapObjectToGrid && snapToGrid)
            {
                #region Grid Snapping
                Vector3 snapPos = GridTool.SnapToGrid(mousePosition);
                Vector3 tempPos = mousePosition;
                Vector3 dir = Vector3.zero;

                switch (snappingPlane)
                {
                    case SnapDirections.XY:
                        tempPos.x = snapPos.x;
                        tempPos.y = snapPos.y;
                        dir = Vector3.forward;
                        break;

                    case SnapDirections.XZ:
                        tempPos.x = snapPos.x;
                        tempPos.z = snapPos.z;
                        dir = Vector3.up;
                        break;

                    case SnapDirections.YZ:
                        tempPos.y = snapPos.y;
                        tempPos.z = snapPos.z;
                        dir = Vector3.right;
                        break;
                }

                if (Physics.Raycast(tempPos, dir, out hit, GridTool.IncrementSize))
                {
                    CalculateRotation(hit);

                    switch (snappingPlane)
                    {
                        case SnapDirections.XY:
                            tempPos.z = hit.point.z;
                            break;

                        case SnapDirections.XZ:
                            tempPos.y = hit.point.y;
                            break;

                        case SnapDirections.YZ:
                            tempPos.x = hit.point.x;
                            break;
                    }
                }
                else if (Physics.Raycast(tempPos, -dir, out hit, GridTool.IncrementSize))
                {
                    CalculateRotation(hit);

                    switch (snappingPlane)
                    {
                        case SnapDirections.XY:
                            tempPos.z = hit.point.z;
                            break;

                        case SnapDirections.XZ:
                            tempPos.y = hit.point.y;
                            break;

                        case SnapDirections.YZ:
                            tempPos.x = hit.point.x;
                            break;
                    }
                }

                previewPosition = tempPos;
                #endregion
            }
            else
            {
                previewPosition = mousePosition;

                CalculateRotation(hit);
            }
        }
        else
        {
            previewPosition = ray.origin + (ray.direction * distanceToObject);

            if (keepNativeRotation)
            {
                previewRotation = selectedObject.transform.rotation;
            }
            else if (randomizeLocalYRotation)
            {
                previewRotation = selectedObject.transform.rotation * Quaternion.Euler(0, currentRandomRotation, 0);
            }
            else
            {
                previewRotation = selectedObject.transform.rotation * Quaternion.Euler(0, localYRotation, 0);
            }
        }
    }

    /// <summary>
    /// Calculates the rotation of the object.
    /// </summary>
    /// <param name="hit">The raycast hit under the object.</param>
    private void CalculateRotation(RaycastHit hit)
    {
        Quaternion originalRotation = selectedObject.transform.rotation;

        if (keepNativeRotation)
        {
            previewRotation = selectedObject.transform.rotation;
        }
        else if (randomizeLocalYRotation)
        {
            selectedObject.transform.rotation = previewRotation;
            selectedObject.transform.up = hit.normal;
            previewRotation = selectedObject.transform.rotation * Quaternion.Euler(0, currentRandomRotation, 0);
        }
        else
        {
            selectedObject.transform.rotation = previewRotation;
            selectedObject.transform.up = hit.normal;
            previewRotation = selectedObject.transform.rotation * Quaternion.Euler(0, localYRotation, 0);
        }

        selectedObject.transform.rotation = originalRotation;
    }

    /// <summary>
    /// Main method. Executes when the left mouse button is clicked in the sceneview and enablePlacement is true.
    /// </summary>
    private void PlaceObject()
    {
        GameObject obj = (GameObject)Instantiate(selectedObject, previewPosition, Quaternion.identity);

        obj.name = selectedObject.name;
        obj.transform.rotation = previewRotation;

        Undo.RegisterCreatedObjectUndo(obj, "Undo placed object.");
    }

    /// <summary>
    /// Updates the window.
    /// </summary>
    private void OnInspectorUpdate()
    {
        Repaint();
    }

    /// <summary>
    /// Draws the GUI in the editor window. Buttons, textfields, dorpdown menues etc. 
    /// </summary>
    private void OnGUI()
    {
        #region Object
        GUILayout.Label("Object", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        selectedObject = (GameObject)EditorGUILayout.ObjectField("Object", selectedObject, typeof(GameObject), false);

        if (EditorGUI.EndChangeCheck())
        {
            if (selectedObject != null)
            {
                previewRotation = selectedObject.transform.rotation;

                try
                {
                    previewMesh = selectedObject.GetComponent<MeshFilter>().sharedMesh;
                }
                catch
                {
                    previewMesh = null;
                    Debug.Log("ObjectPlacerTool.cs - Warning: Selected object has no MeshFilter component.");
                }
            }
        }
        #endregion

        #region Rotation
        GUILayout.Label("Rotation", EditorStyles.boldLabel);

        keepNativeRotation = EditorGUILayout.Toggle(new GUIContent("Keep native rotation", "If this option is active any objects placed will keep their native rotation."), keepNativeRotation);

        if (keepNativeRotation)
        {
            randomizeLocalYRotation = false;
            localYRotation = 0;
        }

        EditorGUI.BeginDisabledGroup(keepNativeRotation);

        randomizeLocalYRotation = EditorGUILayout.Toggle(new GUIContent("Randomize local Y-angle", "If this option is active any objects placed will get a random rotation on their local y-axis."), randomizeLocalYRotation);

        if (randomizeLocalYRotation)
        {
            localYRotation = 0;
        }

        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(keepNativeRotation || randomizeLocalYRotation);

        localYRotation = EditorGUILayout.FloatField("Local Y-angle: ", localYRotation);

        EditorGUI.EndDisabledGroup();
        #endregion

        #region Grid
        GUILayout.Label("Grid", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(!GridTool.EnableGridTool);

        snapToGrid = EditorGUILayout.Toggle(new GUIContent("Snap to Grid", "The grid must be active for this option to work. If this option is active any objects placed will attemp to snap to the grid."), snapToGrid);

        if (!GridTool.EnableGridTool)
        {
            snapToGrid = false;
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(!snapToGrid);

        snappingPlane = (SnapDirections)EditorGUILayout.EnumPopup(new GUIContent("Snapping Plane", "The plane the objects will snap to."), snappingPlane, EditorStyles.popup);

        EditorGUI.EndDisabledGroup();
        #endregion

        EditorGUILayout.Separator();

        #region Enable/Disable button
        EditorGUI.BeginDisabledGroup(selectedObject == null);

        if (placementEnabled)
        {
            if (GUILayout.Button("Disable placement"))
            {
                placementEnabled = false;
                SceneView.RepaintAll();
            }
        }
        else
        {
            if (GUILayout.Button("Enable placement"))
            {
                placementEnabled = true;
            }
        }

        EditorGUI.EndDisabledGroup();
        #endregion
    }

    /// <summary>
    /// Draws the previews as Gizmos.
    /// </summary>
    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    private static void CustomGizmo(Transform objectTransform, GizmoType gizmoType)
    {
        if (placementEnabled)
        {
            if (previewMesh != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireMesh(previewMesh, previewPosition, previewRotation);

                Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.3f);
                Gizmos.DrawMesh(previewMesh, previewPosition, previewRotation);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(previewPosition, noMeshCircleRadius);

                Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.3f);
                Gizmos.DrawSphere(previewPosition, noMeshCircleRadius);
            }
        }
    }
}
