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
    private Vector3 localRotation = Vector3.zero;

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

            // If the grid is enabled and snapToGrid is true
            if (GridTool.EnableGridTool && GridTool.SnapObjectToGrid && snapToGrid)
            {
                #region Grid Snapping
                // Finds the closest snap position.
                Vector3 snapPos = GridTool.SnapToGrid(mousePosition);
                // Creates a local variable and saves the mousePosition in it.
                Vector3 tempPos = mousePosition;
                Vector3 dir = Vector3.zero;

                // Switches on snappingPlane
                switch (snappingPlane)
                {
                    case SnapDirections.XY:
                        // Saves the correct coordinats in the tempPos variable.
                        tempPos.x = snapPos.x;
                        tempPos.y = snapPos.y;
                        // Sets the dir variable.
                        dir = Vector3.forward;
                        break;

                    case SnapDirections.XZ:
                        // Saves the correct coordinats in the tempPos variable.
                        tempPos.x = snapPos.x;
                        tempPos.z = snapPos.z;
                        // Sets the dir variable.
                        dir = Vector3.up;
                        break;

                    case SnapDirections.YZ:
                        // Saves the correct coordinats in the tempPos variable.
                        tempPos.y = snapPos.y;
                        tempPos.z = snapPos.z;
                        // Sets the dir variable.
                        dir = Vector3.right;
                        break;
                }

                // Casts a ray from the tempPos in the direction of dir and as long as GridTool.IncrementSize.
                if (Physics.Raycast(tempPos, dir, out hit, GridTool.IncrementSize))
                {
                    // If the ray hits something the calculate rotation method is called.
                    CalculateRotation(hit);

                    // Switch on snappingPlane
                    switch (snappingPlane)
                    {
                        case SnapDirections.XY:
                            // Sets the correct value for the z coordinate.
                            tempPos.z = hit.point.z;
                            break;

                        case SnapDirections.XZ:
                            // Sets the correct value for the y coordinate.
                            tempPos.y = hit.point.y;
                            break;

                        case SnapDirections.YZ:
                            // Sets the correct value for the x coordinate.
                            tempPos.x = hit.point.x;
                            break;
                    }
                }
                // If the above ray doesn't hit anything the ray is cast in the opposite direction.
                else if (Physics.Raycast(tempPos, -dir, out hit, GridTool.IncrementSize))
                {
                    // The same as above happen.
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

                // Sets the position of the preview to the tempPos.
                previewPosition = tempPos;
                #endregion
            }
            // If no snapping is required.
            else
            {
                // The position of the preview is set to mousePosition.
                previewPosition = mousePosition;

                // The rotation is calculated for the preview.
                CalculateRotation(hit);
            }
        }
        // If no surface is hit with the mouse.
        else
        {
            // The preview position is set to the mousePosition with the distance distanceToObject from the camera.
            previewPosition = ray.origin + (ray.direction * distanceToObject);

            // If the variable keepNativeRotation is true the preview will always have the same rotation.
            if (keepNativeRotation)
            {
                // Sets the preview rotatation to the native rotation of the object.
                previewRotation = selectedObject.transform.rotation;
            }
            // If the variable randomizeLocalYRotation is true the object will get a random rotation every time it is placed.
            else if (randomizeLocalYRotation)
            {
                previewRotation = selectedObject.transform.rotation * Quaternion.Euler(0, currentRandomRotation, 0);
            }
            // If none of the above is true the object will get the specified local y rotation. This is by default set to 0.
            else
            {
                previewRotation = selectedObject.transform.rotation * Quaternion.Euler(localRotation.x, localRotation.y, localRotation.z);
            }
        }
    }

    /// <summary>
    /// Calculates the rotation of the preview.
    /// </summary>
    /// <param name="hit">The raycast hit under the preview.</param>
    private void CalculateRotation(RaycastHit hit)
    {
        // Saves the original rotation for the selectedObject
        Quaternion originalRotation = selectedObject.transform.rotation;

        // If keepNativeRotation is true no rotation is calculated.
        if (keepNativeRotation)
        {
            previewRotation = selectedObject.transform.rotation;
        }
        // If randomizeLocalYRotation is true the rotation is calculated with a random local y value.
        else if (randomizeLocalYRotation)
        {
            // Rotate the selected object to match the preview rotation.
            selectedObject.transform.rotation = previewRotation;
            // The up vector of the object is alligned with the hit normal.
            selectedObject.transform.up = hit.normal;
            // The preview rotation is set to the selected objects rotation multiplied by the random rotation on the y axis.
            previewRotation = selectedObject.transform.rotation * Quaternion.Euler(0, currentRandomRotation, 0);
        }
        // If none of the above is true the rotation is calculated with a fixed local y value.
        else
        {
            // Same as with randomizedLocalYRotation but with a fixed value instead of a random value.
            selectedObject.transform.rotation = previewRotation;
            selectedObject.transform.up = hit.normal;
            previewRotation = selectedObject.transform.rotation * Quaternion.Euler(localRotation.x, localRotation.y, localRotation.z);
        }

        // The selected objects rotation is reset back to the original rotation.
        selectedObject.transform.rotation = originalRotation;
    }

    /// <summary>
    /// Main method. Executes when the left mouse button is clicked in the sceneview and enablePlacement is true.
    /// </summary>
    private void PlaceObject()
    {
        // Creates an instance of an object at a specified position and casts it to a GameObject.
        GameObject obj = (GameObject)Instantiate(selectedObject, previewPosition, Quaternion.identity);

        // Sets the name and rotation of the GameObject
        obj.name = selectedObject.name;
        obj.transform.rotation = previewRotation;

        // Registers the created object for an undo event.
        Undo.RegisterCreatedObjectUndo(obj, "Undo placed object.");
    }

    /// <summary>
    /// Updates the window.
    /// </summary>
    private void OnInspectorUpdate()
    {
        // Repaints the window.
        Repaint();
    }

    /// <summary>
    /// Draws the GUI in the editor window. Buttons, textfields, dorpdown menues etc. 
    /// </summary>
    private void OnGUI()
    {
        #region Object
        // The lable displayed at the top called "Object".
        GUILayout.Label("Object", EditorStyles.boldLabel);

        // Begins a change check.
        EditorGUI.BeginChangeCheck();

        // Creates the object field where the selected object is chosen.
        selectedObject = (GameObject)EditorGUILayout.ObjectField("Object", selectedObject, typeof(GameObject), false);

        // If an new object has been chosen...
        if (EditorGUI.EndChangeCheck())
        {
            //... and the selected object is not null.
            if (selectedObject != null)
            {
                // Update the preview rotation.
                previewRotation = selectedObject.transform.rotation;

                // Try to get the the MeshFilter component of the selected object. 
                try
                {
                    // Sets the preview mesh to the sharedMesh of the MeshFilter.
                    previewMesh = selectedObject.GetComponent<MeshFilter>().sharedMesh;
                }
                // If no MeshFilter component could be found the preview mesh is set to null and a warning is displayed in the Debug Log.
                catch
                {
                    previewMesh = null;
                    Debug.LogWarning("ObjectPlacerTool.cs - Warning: Selected object has no MeshFilter component.");
                }
            }
        }
        #endregion

        #region Rotation
        // The lable displayed as "Rotation"
        GUILayout.Label("Rotation", EditorStyles.boldLabel);

        // Creates a toggle for the keepNativeRotation variable.
        keepNativeRotation = EditorGUILayout.Toggle(new GUIContent("Keep native rotation", "If this option is active any objects placed will keep their native rotation."), keepNativeRotation);

        if (keepNativeRotation)
        {
            // randomizeLocalYRotation and localYRotation is reset to false and 0 respectivly.
            randomizeLocalYRotation = false;
            localRotation = Vector3.zero;
        }

        // Stats a disabled group with keepNativeRotation as control. 
        EditorGUI.BeginDisabledGroup(keepNativeRotation);

        // Creates a toggle for the randomizeLocalYRotation variable.
        randomizeLocalYRotation = EditorGUILayout.Toggle(new GUIContent("Randomize local Y-angle", "If this option is active any objects placed will get a random rotation on their local y-axis."), randomizeLocalYRotation);

        if (randomizeLocalYRotation)
        {
            // localYRotation is reset to 0.
            localRotation = Vector3.zero;
        }

        // Ends the disabled group.
        EditorGUI.EndDisabledGroup();

        // Stats a new disabled group with keepNativeRotation an randomizedLocalYRotation as control.
        EditorGUI.BeginDisabledGroup(keepNativeRotation || randomizeLocalYRotation);

        // Creates a float field for the localYRotation variable.
        localRotation = EditorGUILayout.Vector3Field("Local rotation: ", localRotation);

        // Ends the disabled group.
        EditorGUI.EndDisabledGroup();
        #endregion

        #region Grid
        // The lable displayed as Grid.
        GUILayout.Label("Grid", EditorStyles.boldLabel);

        // Begins a disabled group with GridTool.EnableGridTool as control.
        EditorGUI.BeginDisabledGroup(!GridTool.EnableGridTool);

        // Creates a toggle for the snapToGrid variable. 
        snapToGrid = EditorGUILayout.Toggle(new GUIContent("Snap to Grid", "The grid must be active for this option to work. If this option is active any objects placed will attemp to snap to the grid."), snapToGrid);

        if (!GridTool.EnableGridTool)
        {
            // If GridTool.EnableGridTool is false the snapToGrid is also set to false.
            snapToGrid = false;
        }

        // Ends the disabled group.
        EditorGUI.EndDisabledGroup();

        // Begins a new disabled group with snapToGrid as control.
        EditorGUI.BeginDisabledGroup(!snapToGrid);

        // Creates a dropdown menu for the snappingPlane variable.
        snappingPlane = (SnapDirections)EditorGUILayout.EnumPopup(new GUIContent("Snapping Plane", "The plane the objects will snap to."), snappingPlane, EditorStyles.popup);

        // Ends the disabled group.
        EditorGUI.EndDisabledGroup();
        #endregion

        // Creates a bit of space between GUI elements.
        EditorGUILayout.Separator();

        #region Enable/Disable button
        // Begins a new disabled group with selectedObject as control.
        EditorGUI.BeginDisabledGroup(selectedObject == null);

        if (placementEnabled)
        {
            // Creates a button. If placementEnabled is true the button displays "Disable placement".
            if (GUILayout.Button("Disable placement"))
            {
                // If the button is pressed placementEnabled is set to false and the SceneView is repainted.
                placementEnabled = false;
                SceneView.RepaintAll();
            }
        }
        else
        {
            // Creates a button. If placementEnabled is false the button displays "Disable placement".
            if (GUILayout.Button("Enable placement"))
            {
                // If the button is pressed placementEnabled is set to true.
                placementEnabled = true;
            }
        }

        // Ends the disabled group.
        EditorGUI.EndDisabledGroup();
        #endregion
    }

    /// <summary>
    /// Draws the previews as Gizmos.
    /// </summary>
    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    private static void CustomGizmo(Transform objectTransform, GizmoType gizmoType)
    {
        // Only draw if placementEnabled is true.
        if (placementEnabled)
        {
            // If the preview mesh is not null the mesh is draw.
            if (previewMesh != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireMesh(previewMesh, previewPosition, previewRotation);

                Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.3f);
                Gizmos.DrawMesh(previewMesh, previewPosition, previewRotation);
            }
            // If the preview mesh is null a sphere is drawn instead.
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