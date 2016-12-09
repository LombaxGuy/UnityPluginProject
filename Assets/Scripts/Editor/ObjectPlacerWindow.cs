using UnityEngine;
using UnityEditor;
using System.Collections;

public class ObjectPlacerWindow : EditorWindow {

    #region Exposed fields
    GameObject selectedObject = null;

    static Mesh mesh = null;

    Material ghostMaterial;

    Vector3 rotationOffset;

    Vector3 randomRotationMin;
    Vector3 randomRotationMax;
    #endregion

    GameObject ghost = null;

    bool placingObject = false;
    bool randomizeRotation = false;

    Vector3 mousePos;
    Quaternion rotation;

    [MenuItem("ZoonTools/Object Placer", false, 102)]
    static void Init()
    {
        ObjectPlacerWindow window = (ObjectPlacerWindow)EditorWindow.GetWindow(typeof(ObjectPlacerWindow));
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
        if (placingObject)
        {
            Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(r, out hit, Mathf.Infinity))
            {
                mousePos = hit.point;

                Handles.color = Color.red;
                Handles.DrawLine(hit.point, hit.point + hit.normal);

                if(ghost)
                {
                    if (GridTool.EnableGridTool && GridTool.SnapObjectToGrid)
                    {
                        Vector3 snapPos = GridTool.SnapToGrid(mousePos);

                        if (hit.normal == Vector3.up || hit.normal == Vector3.down ||
                            hit.normal == Vector3.right || hit.normal == Vector3.left ||
                            hit.normal == Vector3.forward || hit.normal == Vector3.back)
                        {
                            ghost.transform.position = snapPos;
                        }
                        else
                        {
                            if (hit.normal.x == 0)
                            {
                                ghost.transform.position = new Vector3(snapPos.x, mousePos.y, mousePos.z);
                            }

                            if (hit.normal.y == 0)
                            {
                                ghost.transform.position = new Vector3(mousePos.x, snapPos.y, mousePos.z);
                            }

                            if (hit.normal.z == 0)
                            {
                                ghost.transform.position = new Vector3(mousePos.x, mousePos.y, snapPos.z);
                            }
                        }
                    }
                    else
                    {
                        ghost.transform.position = mousePos;
                    }

                    ghost.transform.up = hit.normal;
                    rotation = ghost.transform.rotation;

                    //GhostGizmo( mousePos, Quaternion.LookRotation(hit.normal), GizmoType.NotInSelectionHierarchy, selectedObject.GetComponent<Mesh>());

                    if (randomRotationMax != Vector3.zero && randomizeRotation)
                    {
                        if (rotationOffset != Vector3.zero)
                        {
                            rotation = rotation * Quaternion.Euler(rotationOffset);
                        }

                        rotation = rotation * Quaternion.Euler(Random.Range(randomRotationMin.x, randomRotationMax.x),
                                                               Random.Range(randomRotationMin.y, randomRotationMax.y),
                                                               Random.Range(randomRotationMin.z, randomRotationMax.z));
                        ghost.transform.rotation = rotation;

                        randomizeRotation = false;
                    }

                    if (rotationOffset != Vector3.zero && randomRotationMax == Vector3.zero)
                    {
                        rotation = rotation * Quaternion.Euler(rotationOffset);
                        ghost.transform.rotation = rotation;
                    }
                }

                HandleUtility.Repaint();

                if(Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    GameObject obj = (GameObject)Instantiate(selectedObject, mousePos, Quaternion.identity);

                    obj.transform.rotation = rotation;

                    randomizeRotation = true;

                    Undo.RegisterCreatedObjectUndo(obj, "Undo placed object");
                }
            }
        }
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnGUI()
    {
        GUILayout.Label("Object", EditorStyles.boldLabel);


        EditorGUI.BeginChangeCheck();

        selectedObject = (GameObject)EditorGUILayout.ObjectField("Object: ", selectedObject, typeof(GameObject), false);

        if (EditorGUI.EndChangeCheck())
        {
            try
            {
                mesh = selectedObject.GetComponent<MeshFilter>().sharedMesh;
            }
            catch
            {
                Debug.Log("An error occured: Could not find mesh on selected object!");
            }
        }


        ghostMaterial = (Material)EditorGUILayout.ObjectField("Material", ghostMaterial, typeof(Material));


        GUILayout.Label("Transform", EditorStyles.boldLabel);

        rotationOffset = EditorGUILayout.Vector3Field("Rotation offset: ", rotationOffset);

        GUILayout.Label("Random rotation", EditorStyles.boldLabel);
        randomRotationMin = EditorGUILayout.Vector3Field("Minimum rotation: ", randomRotationMin);
        randomRotationMax = EditorGUILayout.Vector3Field("Maximum rotation: ", randomRotationMax);

        if (placingObject)
        {
            if (GUILayout.Button("Disable placement"))
            {
                placingObject = false;

                if(ghost != null)
                {
                    DestroyImmediate(ghost);
                }
            }
        }

        else
        {
            if(GUILayout.Button("Enable placement") && selectedObject)
            {
                placingObject = true;

                if (ghost == null)
                {
                    ghost = (GameObject)Instantiate(selectedObject, Vector3.zero, Quaternion.identity);
                    ghost.layer = 2;

                    Renderer rend = ghost.GetComponent<Renderer>();

                    if (rend)
                        rend.material = ghostMaterial;

                    Collider col = ghost.GetComponent<Collider>();

                    if (col)
                        DestroyImmediate(col);
                }
            }
        }
    }



    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    static void CustomGizmo(Transform objectTransform, GizmoType gizmoType)
    {
        

        //Debug.Log("Drawing");
        //Gizmos.color = Color.red;

        //Gizmos.DrawCube(Vector3.zero, Vector3.one);

        // Gizmos.DrawMesh(mesh, pos, rotation);
    }
}
