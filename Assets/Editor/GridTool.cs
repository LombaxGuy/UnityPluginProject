using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[InitializeOnLoad]
public class GridTool
{
    // Is the tool enabled or disabled.
    private static bool enableGridTool = true;

    // Configuarable setting shown in the toolbar.
    private static bool _showGrid = true;
    private static bool _xyGrid = true;
    private static bool _xzGrid = true;
    private static bool _yzGrid = true;
    private static bool _useSelectionPosition = true;
    private static bool _snapObjectToGrid = true;
    private static bool _snapAxisOnly = true;
    private static bool _showStartPosition = false;
    private static bool _allignAllSelectedWithGrid = false;
    private static float _incrementSize = 1;
    private static Vector3 _gridOffset = Vector3.zero;

    // Internal setting. Can't be edited for the editor.
    private static int numberOfLines = 100;
    private static float gridSize;

    private static Vector3 oldPosition;
    private static Vector3 startPosition;
    private static bool draggingMouse = false;

    // The position of the grid's origin point.
    private static Vector3 position = Vector3.zero;

    // Lists for storing the lines that are drawn in the grid.
    private static List<GridLine> xzLinesX = new List<GridLine>();
    private static List<GridLine> xzLinesZ = new List<GridLine>();

    private static List<GridLine> xyLinesX = new List<GridLine>();
    private static List<GridLine> xyLinesY = new List<GridLine>();

    private static List<GridLine> yzLinesY = new List<GridLine>();
    private static List<GridLine> yzLinesZ = new List<GridLine>();

    private static GameObject[] selectedGameObjects = new GameObject[0];
    private static Vector3[] selectedRelativePositions = new Vector3[0];

    #region Properties
    public static bool EnableGridTool
    {
        get { return enableGridTool; }
        set { enableGridTool = value; }
    }

    public static bool ShowGrid
    {
        get { return _showGrid; }
        set { _showGrid = value; }
    }

    public static bool XYGrid
    {
        get { return _xyGrid; }
        set { _xyGrid = value; }
    }

    public static bool XZGrid
    {
        get { return _xzGrid; }
        set { _xzGrid = value; }
    }
    public static bool YZGrid
    {
        get { return _yzGrid; }
        set { _yzGrid = value; }
    }

    public static bool UseSelectionPosition
    {
        get { return _useSelectionPosition; }
        set { _useSelectionPosition = value; }
    }

    public static bool SnapObjectToGrid
    {
        get { return _snapObjectToGrid; }
        set { _snapObjectToGrid = value; }
    }

    public static bool SnapAxisOnly
    {
        get { return _snapAxisOnly; }
        set { _snapAxisOnly = value; }
    }

    public static bool ShowStartPosition
    {
        get { return _showStartPosition; }
        set { _showStartPosition = value; }
    }

    public static bool AllignAllSelectedWithGrid
    {
        get { return _allignAllSelectedWithGrid; }
        set { _allignAllSelectedWithGrid = value; }
    }

    public static float GridSize
    {
        get { return gridSize; }
        set { gridSize = value; }
    }

    public static float IncrementSize
    {
        get { return _incrementSize; }
        set { _incrementSize = value; }
    }

    public static Vector3 GridOffset
    {
        get { return _gridOffset; }
        set { _gridOffset = value; }
    }
    #endregion

    /// <summary>
    /// Sets the enabledGridVariable and Subscribes/Unsubscribes delegates
    /// </summary>
    public static void OnToggle()
    {
        // If enableGridTool is true...
        if (enableGridTool)
        {
            //... subscribe to some delegates and update the grid's lines.
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            EditorApplication.update += Update;
            Selection.selectionChanged += UpdateGridLines;
            UpdateGridLines();
        }
        // If enableGridTool is false...
        else
        {
            //... unsubscribe form some delegates and clear the grid's lines.
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            EditorApplication.update -= Update;
            Selection.selectionChanged -= UpdateGridLines;
            ClearGridLines();
        }
    }

    /// <summary>
    /// Static constructor for the GridTool class.
    /// </summary>
    static GridTool()
    {
        // If enableGridTool is true...
        if (enableGridTool)
        {
            //... subscribe to some delegates.
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            EditorApplication.update += Update;
            Selection.selectionChanged += OnSelectionChange;
        }

        // Sets the value of  gridSize and gridSectionSize.
        gridSize = 100 * _incrementSize;

        // Updates the grid's lines.
        UpdateGridLines();
    }

    /// <summary>
    /// Update loop.
    /// </summary>
    static void Update()
    {
        // If an object with a Transform component has been selected, the mouse is being dragged and _snapObjectToGrid is true...
        if (Selection.activeTransform && draggingMouse && _snapObjectToGrid)
        {
            // The active transform is saved in a local variable.
            Transform activeTrans = Selection.activeTransform;

            // If the oldPosition is not set it is set to the active tansforms position. 
            if (oldPosition == Vector3.zero)
            {
                oldPosition = activeTrans.position;
            }

            // If the oldPosition is not the same as the current position...
            if (oldPosition != activeTrans.position)
            {
                //... and _snapAxisOnly is true...
                if (_snapAxisOnly)
                {
                    // Runs through the array of selected gameobjects.
                    for (int i = 0; i < selectedGameObjects.Length; i++)
                    {
                        // If _allignAllSelectedWithGrid is true all objects in the list will snap to the axis.
                        if (_allignAllSelectedWithGrid)
                        {
                            selectedGameObjects[i].transform.position = SnapToAxis(selectedGameObjects[i].transform);
                        }
                        // If _allignAllSelectedWithGrid is false only the active selection will snap to the axis and all other objects will move relative to the active selection.
                        else
                        {
                            // If the current object is not the active selection we snap to the axis and add the relative position stored in selectedRelativePositions.
                            if (selectedGameObjects[i].transform != activeTrans)
                            {
                                selectedGameObjects[i].transform.position = SnapToAxis(activeTrans) + selectedRelativePositions[i];
                            }
                            // If the current object is the active selection we just snap to the axis.
                            else
                            {
                                activeTrans.position = SnapToAxis(activeTrans);
                            }
                        }
                    }
                }
                //... and _snapAxisOnly is false...
                else
                {
                    // Runs through the array of selected gameobjects.
                    for (int i = 0; i < selectedGameObjects.Length; i++)
                    {
                        // If _allignAllSelectedWithGrid is true all objects in the list will snap to the grid.
                        if (_allignAllSelectedWithGrid)
                        {
                            selectedGameObjects[i].transform.position = SnapToGrid(selectedGameObjects[i].transform);
                        }
                        // If _allignAllSelectedWithGrid is false only the active selection will snap to the grid and all other objects will move relative to the active selection.
                        else
                        {
                            // If the current object is not the active selection we snap to the grid and add the relative position stored in selectedRelativePositions.
                            if (selectedGameObjects[i].transform != activeTrans)
                            {
                                selectedGameObjects[i].transform.position = SnapToGrid(activeTrans) + selectedRelativePositions[i];
                            }
                            // If the current object is the active selection we just snap to the grid.
                            else
                            {
                                activeTrans.position = SnapToGrid(activeTrans);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Method that should be called every time something on the GUI changes.
    /// </summary>
    /// <param name="sceneView">The SceneView.</param>
    static void OnSceneGUI(SceneView sceneView)
    {
        // Catch the current event.
        Event e = Event.current;

        // Switch on the current event.
        switch (e.type)
        {
            // If the current event is a MouseUp event...
            case EventType.MouseUp:
                // Resets variables
                oldPosition = Vector3.zero;
                startPosition = Vector3.zero;
                draggingMouse = false;

                // Updates the grid's lines.
                UpdateGridLines();
                break;

            // If the current event is a MouseDown event...
            case EventType.MouseDown:
                // If an object with a transform component is selected...
                if (Selection.activeTransform)
                {
                    //... startPosition is set to the selected transform's position.
                    startPosition = Selection.activeTransform.position;
                }
                break;

            // If the current event is a MouseDrag event...
            case EventType.MouseDrag:
                // Set the draggingMouse variable to true.
                draggingMouse = true;
                break;

            // If the curren event is a ValidateCommand event...
            case EventType.ValidateCommand:
                // If the event's command name is "UndoRedoPerformed"...
                if (e.commandName == "UndoRedoPerformed")
                {
                    // Update the grid's lines.
                    UpdateGridLines();
                }
                break;
        }
    }

    /// <summary>
    /// Function used for drawing Gizmos.
    /// </summary>
    /// <param name="objectTransform"></param>
    /// <param name="gizmoType"></param>
    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    static void CustomGizmo(Transform objectTransform, GizmoType gizmoType)
    {
        // If _showGrid is true the grid is drawn.
        if (_showGrid)
        {
            #region Draw XZ-Grid
            if (_xzGrid)
            {
                // Draw grid
                if (xzLinesX.Count > 0 && xzLinesZ.Count > 0)
                {
                    // Draw lines along the X-axis
                    for (int i = 0; i < xzLinesX.Count; i++)
                    {
                        // If the line is not a section line...
                        if ((i % 10) != 0)
                        {
                            //... set the color to a very transparent green.
                            Gizmos.color = new Color(0, 1, 0, 0.1f);
                        }
                        // If the line is a section line...
                        else
                        {
                            //... set the color to a less transparent green.
                            Gizmos.color = new Color(0, 1, 0, 0.4f);
                        }

                        // When the color is determined draw the line.
                        Gizmos.DrawLine(xzLinesX[i].StartPosition, xzLinesX[i].EndPosition);
                    }

                    // Draw lines along the Z-axis
                    for (int i = 0; i < xzLinesZ.Count; i++)
                    {
                        // If the line is not a section line...
                        if ((i % 10) != 0)
                        {
                            //... set the color to a very transparent green.
                            Gizmos.color = new Color(0, 1, 0, 0.1f);
                        }
                        // If the line is a section line...
                        else
                        {
                            //... set the color to a less transparent green.
                            Gizmos.color = new Color(0, 1, 0, 0.4f);
                        }

                        // When the color is determined draw the line.
                        Gizmos.DrawLine(xzLinesZ[i].StartPosition, xzLinesZ[i].EndPosition);
                    }
                }
            }
            #endregion

            #region Draw XY-Grid
            if (_xyGrid)
            {
                // Draw grid
                if (xyLinesX.Count > 0 && xyLinesY.Count > 0)
                {
                    // Draw lines along the X-axis
                    for (int i = 0; i < xzLinesX.Count; i++)
                    {
                        // If the line is not a section line...
                        if ((i % 10) != 0)
                        {
                            //... set the color to a very transparent green.
                            Gizmos.color = new Color(0, 0, 1, 0.1f);
                        }
                        // If the line is a section line...
                        else
                        {
                            //... set the color to a less transparent green.
                            Gizmos.color = new Color(0, 0, 1, 0.4f);
                        }

                        // When the color is determined draw the line.
                        Gizmos.DrawLine(xyLinesX[i].StartPosition, xyLinesX[i].EndPosition);
                    }

                    // Draw lines along the Y-axis
                    for (int i = 0; i < xyLinesY.Count; i++)
                    {
                        // If the line is not a section line...
                        if ((i % 10) != 0)
                        {
                            //... set the color to a very transparent blue.
                            Gizmos.color = new Color(0, 0, 1, 0.1f);
                        }
                        // If the line is a section line...
                        else
                        {
                            //... set the color to a less transparent blue.
                            Gizmos.color = new Color(0, 0, 1, 0.4f);
                        }

                        // When the color is determined draw the line.
                        Gizmos.DrawLine(xyLinesY[i].StartPosition, xyLinesY[i].EndPosition);
                    }
                }
            }
            #endregion

            #region Draw YZ-Grid
            if (_yzGrid)
            {
                // Draw grid
                if (yzLinesY.Count > 0 && yzLinesZ.Count > 0)
                {
                    // Draw lines along the Y-axis
                    for (int i = 0; i < yzLinesY.Count; i++)
                    {
                        // If the line is not a section line...
                        if ((i % 10) != 0)
                        {
                            //... set the color to a very transparent red.
                            Gizmos.color = new Color(1, 0, 0, 0.1f);
                        }
                        // If the line is a section line...
                        else
                        {
                            //... set the color to a less transparent red.
                            Gizmos.color = new Color(1, 0, 0, 0.4f);
                        }

                        // When the color is determined draw the line.
                        Gizmos.DrawLine(yzLinesY[i].StartPosition, yzLinesY[i].EndPosition);
                    }

                    // Draw lines along the Z-axis
                    for (int i = 0; i < yzLinesZ.Count; i++)
                    {
                        // If the line is not a section line...
                        if ((i % 10) != 0)
                        {
                            //... set the color to a very transparent red.
                            Gizmos.color = new Color(1, 0, 0, 0.1f);
                        }
                        // If the line is a section line...
                        else
                        {
                            //... set the color to a less transparent red.
                            Gizmos.color = new Color(1, 0, 0, 0.4f);
                        }

                        // When the color is determined draw the line.
                        Gizmos.DrawLine(yzLinesZ[i].StartPosition, yzLinesZ[i].EndPosition);
                    }
                }
            }
            #endregion
        }

        //If _showStartPosition and draggingMouse is true the ghost object is shown.
        if (_showStartPosition && draggingMouse)
        {
            #region Draw Start Position
            // If an object with a transform component is selected and oldPositin is not the same as the current position...
            if (Selection.activeTransform && oldPosition != Selection.activeTransform.position)
            {
                // Get the mesh component of the object and save it as a local variable.
                Mesh mesh;

                try
                {
                    mesh = Selection.activeGameObject.GetComponent<MeshFilter>().sharedMesh;
                }
                catch
                {
                    mesh = null;
                }

                // If the local mesh variable is not null
                if (mesh != null)
                {
                    // Draw a 'ghost' of the object at the starting position.
                    Gizmos.color = new Color(1, 1, 0, 0.3f);
                    Gizmos.DrawMesh(mesh, startPosition);

                    Gizmos.color = new Color(1, 1, 0, 1f);
                    Gizmos.DrawWireMesh(mesh, startPosition);
                }
                // If the local mesh variable is null
                else
                {
                    // Draw a 'ghost' of a sphere at the starting position.
                    Gizmos.color = new Color(1, 1, 0, 0.3f);
                    Gizmos.DrawSphere(startPosition, 0.25f);

                    Gizmos.color = new Color(1, 1, 0, 1f);
                    Gizmos.DrawWireSphere(startPosition, 0.25f);
                }
            }
            #endregion
        }
    }

    /// <summary>
    /// Method that should be called when the selection changes.
    /// </summary>
    private static void OnSelectionChange()
    {
        // Updates the grid's lines.
        UpdateGridLines();

        // Update the selection arrays.
        UpdateSelectionArrays();
    }

    /// <summary>
    /// Updates the lines in the grid.
    /// </summary>
    public static void UpdateGridLines()
    {
        // If _showGrid is true the lines are updated.
        if (_showGrid)
        {
            // Sets the gridSize and the girdSectionSize
            gridSize = 100 * _incrementSize;

            // Clears all the lists with lines. 
            xzLinesX.Clear();
            xzLinesZ.Clear();

            xyLinesX.Clear();
            xyLinesY.Clear();

            yzLinesY.Clear();
            yzLinesZ.Clear();

            // If _useSelectionPosition is true and an object with a transform component is selected...
            if (_useSelectionPosition && Selection.activeTransform)
            {
                // The position of the grid is set and the _gridOffset is added.
                position = SnapToGrid(Selection.activeTransform) /*+ _gridOffset*/;
            }
            // If _useSelectionPosition is false or if no object with a transform component is selected...
            else
            {
                // The position of the grid is set to Vector3.zero + the offset.
                position = Vector3.zero /*+ _gridOffset*/;
            }

            // Each line is adde to the correct list.
            for (float i = 0; i <= numberOfLines; i++)
            {
                #region Update XZ-Grid
                // Adds lines that run along the X-axis
                xzLinesX.Add(new GridLine(new Vector3(position.x, position.y, position.z + (i * _incrementSize)), new Vector3(position.x + gridSize, position.y, position.z + (i * _incrementSize))));

                // Adds lines that run along the Z-axis
                xzLinesZ.Add(new GridLine(new Vector3(position.x + (i * _incrementSize), position.y, position.z), new Vector3(position.x + (i * _incrementSize), position.y, position.z + gridSize)));
                #endregion

                #region Update XY-Grid
                // Adds lines that run along the X-axis
                xyLinesX.Add(new GridLine(new Vector3(position.x, position.y + (i * _incrementSize), position.z), new Vector3(position.x + gridSize, position.y + (i * _incrementSize), position.z)));

                // Adds lines that run along the Y-axis
                xyLinesY.Add(new GridLine(new Vector3(position.x + (i * _incrementSize), position.y, position.z), new Vector3(position.x + (i * _incrementSize), position.y + gridSize, position.z)));
                #endregion

                #region Update YZ-Grid
                // Adds lines that run along the Y-axis
                yzLinesY.Add(new GridLine(new Vector3(position.x, position.y, position.z + (i * _incrementSize)), new Vector3(position.x, position.y + gridSize, position.z + (i * _incrementSize))));

                // Adds lines that run along the Z-axis
                yzLinesZ.Add(new GridLine(new Vector3(position.x, position.y + (i * _incrementSize), position.z), new Vector3(position.x, position.y + (i * _incrementSize), position.z + gridSize)));
                #endregion
            }
        }
    }

    /// <summary>
    /// Updates the selection arrays.
    /// </summary>
    public static void UpdateSelectionArrays()
    {
        // Sets the two arrays for used for snapping.
        selectedGameObjects = Selection.gameObjects;
        selectedRelativePositions = new Vector3[selectedGameObjects.Length];

        // Calculates the position of all the selected object relative to the active selection and saves it in an array.
        for (int i = 0; i < selectedRelativePositions.Length; i++)
        {
            selectedRelativePositions[i] = Selection.activeTransform.InverseTransformPoint(selectedGameObjects[i].transform.position);
        }
    }

    /// <summary>
    /// Returns a Vector3 that is "On grid" based on the activeTransform's position.
    /// </summary>
    /// <param name="activeTransform">The transform which position will be used.</param>
    /// <returns>Returns a Vector3 which x, y and z components are all multiples of _incrementSize.</returns>
    private static Vector3 SnapToGrid(Transform activeTransform)
    {
        return new Vector3
        (
        _incrementSize * Mathf.Round(activeTransform.position.x - _gridOffset.x / _incrementSize) + _gridOffset.x,
        _incrementSize * Mathf.Round(activeTransform.position.y - _gridOffset.y / _incrementSize) + _gridOffset.y,
        _incrementSize * Mathf.Round(activeTransform.position.z - _gridOffset.z / _incrementSize) + _gridOffset.z
        );
    }

    /// <summary>
    /// Returns a Vector3 that is "On grid" on one or more axis based on the activeTransform's position.
    /// </summary>
    /// <param name="activeTransform">The transform which position will be used.</param>
    /// <returns>Returns a Vector3 which x, y or z components are multiples of _incrementSize.</returns>
    private static Vector3 SnapToAxis(Transform activeTransform)
    {
        // Local variable used to return at the end.
        Vector3 snapPos = activeTransform.position;

        // If the x-position of the active transform is not the old x-Position... 
        if (oldPosition.x != activeTransform.position.x)
        {
            // Snap the x-component of the snapPos vector to the grid.
            snapPos.x = _incrementSize * Mathf.Round(activeTransform.position.x - _gridOffset.x / _incrementSize) + _gridOffset.x;
        }

        // If the y-position of the active transform is not the old y-Position... 
        if (oldPosition.y != activeTransform.position.y)
        {
            // Snap the y-component of the snapPos vector to the grid.
            snapPos.y = _incrementSize * Mathf.Round(activeTransform.position.y - _gridOffset.y / _incrementSize) + _gridOffset.y;
        }

        // If the z-position of the active transform is not the old z-Position... 
        if (oldPosition.z != activeTransform.position.z)
        {
            // Snap the z-component of the snapPos vector to the grid.
            snapPos.z = _incrementSize * Mathf.Round(activeTransform.position.z - _gridOffset.z / _incrementSize) + _gridOffset.z;
        }

        // Returns snapPos.
        return snapPos;
    }

    /// <summary>
    /// Clears all the lines in the lists.
    /// </summary>
    private static void ClearGridLines()
    {
        xzLinesX.Clear();
        xzLinesZ.Clear();

        xyLinesX.Clear();
        xyLinesY.Clear();

        yzLinesY.Clear();
        yzLinesZ.Clear();
    }
}
