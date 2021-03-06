﻿using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class GridToolGUI
{
    private static GUISkin guiSkin;

    private static Texture toggleGridTexture;
    private static Texture toggleXYGridTexture;
    private static Texture toggleXZGridTexture;
    private static Texture toggleYZGridTexture;

    private static Texture toggleSnapObjectToGridTexture;
    private static Texture toggleSnapAxisOnlyTexture;
    private static Texture toggleShowStartPositionTexture;
    private static Texture toggleUseSelectionPositionTexture;
    private static Texture toggleIndividualSnappingTexture;

    /// <summary>
    /// Adds a menu item to the unity editor.
    /// </summary>
    [MenuItem("ZoonTools/Toggle Grid", false, 1)]
    static void ToggleGrid()
    {
        // Calls the OnToggle methods in both this class and the GridTool class.
        OnToggle();
        GridTool.OnToggle();

        // Repaints the scene view.
        SceneView.RepaintAll();
    }

    /// <summary>
    /// Subscribes/Unsubscribes form delegates.
    /// </summary>
    public static void OnToggle()
    {
        // If the variable enableGridTool is false...
        if (!GridTool.EnableGridTool)
        {
            //... subscribe the method OnSceneGUI to the SceneView.onSceneGUIDelegate
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }
        // If the variable enableGridTool is true...
        else
        {
            //... unsubscribe the method OnSceneGUI form the SceneView.onSceneGUIDelegate
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }
    }

    /// <summary>
    /// Static constructor for the GridToolGUI class.
    /// </summary>
    static GridToolGUI()
    {
        // Subscribes the method OnSceneGUI to the SceneView.onsceneGUIDelegate.
        //SceneView.onSceneGUIDelegate += OnSceneGUI;

        // Loads the GUI that should be used to draw the GUI.
        guiSkin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/ZoonTools/GUI/Grid/GridToolGUISkin.guiskin");

        // Loads the textures of all the buttons.
        toggleGridTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZoonTools/GUI/Grid/ToggleGrid.png");
        toggleXYGridTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZoonTools/GUI/Grid/ToggleGridBlue.png");
        toggleXZGridTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZoonTools/GUI/Grid/ToggleGridGreen.png");
        toggleYZGridTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZoonTools/GUI/Grid/ToggleGridRed.png");

        toggleSnapObjectToGridTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZoonTools/GUI/Grid/ToggleGridSnapping.png");
        toggleSnapAxisOnlyTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZoonTools/GUI/Grid/ToggleAxisSnap.png");
        toggleShowStartPositionTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZoonTools/GUI/Grid/ToggleStartPosition.png");
        toggleUseSelectionPositionTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZoonTools/GUI/Grid/ToggleRepositionGrid.png");
        toggleIndividualSnappingTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ZoonTools/GUI/Grid/ToggleIndividualSnapping.png");
    }

    /// <summary>
    /// Method that should be called every time something on the GUI changes.
    /// </summary>
    /// <param name="sceneView">The SceneView.</param>
    static void OnSceneGUI(SceneView sceneView)
    {
        // Catches the current event.
        Event e = Event.current;

        // If the current event is either a Repaint or a Layout event...
        if (e.type == EventType.Repaint || e.type == EventType.Layout)
        {
            //... call OnRepaint.
            OnRepaint(sceneView, e);
        }
    }

    /// <summary>
    /// Method that should be called on Repain events.
    /// </summary>
    /// <param name="sceneView">The SceneView.</param>
    /// <param name="e">The current event.</param>
    static void OnRepaint(SceneView sceneView, Event e)
    {
        // Creates a rectangle that defines the position and size of the toolbar.
        Rect rectangle = new Rect(0, sceneView.position.height - 40, sceneView.position.width, 40);

        // Creates a new GUI style to use for the toolbar.
        GUIStyle style = new GUIStyle(EditorStyles.toolbar);

        // Sets the height of the toolbar to 40 pixels.
        style.fixedHeight = 40;

        // Creates the toolbar as a window.
        GUILayout.Window(140064, rectangle, OnBottomToolbarGUI, "", style);
    }

    static void OnBottomToolbarGUI(int windowID)
    {
        // Begins a horizontal control group.
        GUILayout.BeginHorizontal();

        // Sets the GUI skin.
        GUI.skin = guiSkin;

        // Starts a change check.
        EditorGUI.BeginChangeCheck();

        // Creates toggle buttons for each of the grid settings.
        GridTool.ShowGrid = CreateGUIToggle(new Rect(5, 5, 30, 30), GridTool.ShowGrid, toggleGridTexture, "Draw grid.");

        GridTool.XYGrid = CreateGUIToggle(new Rect(55, 5, 30, 30), GridTool.XYGrid, toggleXYGridTexture, "Draw grid on the XY-plane.");
        GridTool.XZGrid = CreateGUIToggle(new Rect(90, 5, 30, 30), GridTool.XZGrid, toggleXZGridTexture, "Draw grid on the XZ-plane.");
        GridTool.YZGrid = CreateGUIToggle(new Rect(125, 5, 30, 30), GridTool.YZGrid, toggleYZGridTexture, "Draw grid on the YZ-plane.");

        // Creates a line between the grid settings and the snap settings.
        CreateGUILine(new Rect(164, 0, 2, 40));

        // Creates toggle buttons for each of the snap settings.
        GridTool.SnapObjectToGrid = CreateGUIToggle(new Rect(175, 5, 30, 30), GridTool.SnapObjectToGrid, toggleSnapObjectToGridTexture, "Snap to grid. When this is turned on objectes moved within the scene view will snap to the grid.");
        GridTool.SnapAxisOnly = CreateGUIToggle(new Rect(210, 5, 30, 30), GridTool.SnapAxisOnly, toggleSnapAxisOnlyTexture, "Axis snapping. When this is turned on objectes moved within the scene view will only snap to the axis the object is being moved on.");
        GridTool.AllignAllSelectedWithGrid = CreateGUIToggle(new Rect(245, 5, 30, 30), GridTool.AllignAllSelectedWithGrid, toggleIndividualSnappingTexture, "Individual snapping. If multiple objects are moved at the same time each individual object will snap to the nearest snapping point.");

        // Creates a line between the snap settings and the grid position settings.
        CreateGUILine(new Rect(354, 0, 2, 40));

        // Creates a toggle button for a grid position setting.
        GridTool.UseSelectionPosition = CreateGUIToggle(new Rect(365, 5, 30, 30), GridTool.UseSelectionPosition, toggleUseSelectionPositionTexture, "Dynamic grid. When this is turned on the grid will change position based on the selected object.");

        // Creates a line between the grid position settings and other settings.
        CreateGUILine(new Rect(614, 0, 2, 40));

        // Creates a toggle button for the show start position setting.
        GridTool.ShowStartPosition = CreateGUIToggle(new Rect(625, 5, 30, 30), GridTool.ShowStartPosition, toggleShowStartPositionTexture, "Show start positions. When this is turned on objects moved within the scene view will leave a 'ghost' at their initial position. The 'ghost' is removed when the mouse button is released.");

        // If any of the above setting has been changed...
        if (EditorGUI.EndChangeCheck())
        {
            //... update the lines in the grid and the selection arrays.
            GridTool.UpdateGridLines();
            GridTool.UpdateSelectionArrays();
        }

        // Defines the x position of the dorpdown menu
        int snapSizeX = 285;

        GUI.Label(new Rect(snapSizeX, 5, 60, 20), "Cell size", EditorStyles.label);
        EditorGUI.BeginChangeCheck();

        GridTool.IncrementSize = EditorGUI.FloatField(new Rect(285, 20, 60, 15), Mathf.Abs(GridTool.IncrementSize));

        if (EditorGUI.EndChangeCheck())
        {
            GridTool.UpdateGridLines();
        }

        // Starts a change check
        EditorGUI.BeginChangeCheck();

        // Creates a vector 3 field in which the grid offset can be set.
        GridTool.GridOffset = EditorGUI.Vector3Field(new Rect(405, 3, 200, 35), "Grid offset", GridTool.GridOffset);

        // If the grid offset is changed...
        if (EditorGUI.EndChangeCheck())
        {
            //... update the lines in the grid.
            GridTool.UpdateGridLines();
        }

        // Ends the horizontal control group.
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Creates a GUI toggle button.
    /// </summary>
    /// <param name="position">The Rect that defines the bounds and position of the toggle button.</param>
    /// <param name="toggleValue">The value that should be toggled.</param>
    /// <param name="toggleTexture">"The image on the button."</param>
    /// <param name="tooltipText">"The tooltip displayed when the button is hovered with the mouse."</param>
    /// <returns>The value of the toggle buttons current state.</returns>
    private static bool CreateGUIToggle(Rect position, bool toggleValue, Texture toggleTexture, string tooltipText)
    {
        // If the toggleValue is true...
        if (toggleValue)
        {
            //... set the GUI content color to gray.
            GUI.contentColor = Color.gray;
        }
        // Else if the toggleValue is false...
        else
        {
            //... set the GUI content color to white.
            GUI.contentColor = Color.white;
        }

        // Creates the GUI toggle button with the specified values.
        toggleValue = GUI.Toggle(position, toggleValue, new GUIContent(toggleTexture, tooltipText));

        // Returns the toggleValue
        return toggleValue;
    }

    /// <summary>
    /// Creates a gray box based on a given Rect
    /// </summary>
    /// <param name="position">The Rect that defines the box.</param>
    private static void CreateGUILine(Rect position)
    {
        // Sets the GUI color to gray
        GUI.backgroundColor = Color.gray;

        // Creates the box
        GUI.Box(position, "");

        // Sets the GUI color back to white
        GUI.backgroundColor = Color.white;
    }
}
