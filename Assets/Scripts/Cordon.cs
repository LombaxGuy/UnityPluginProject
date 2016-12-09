using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class Cordon : MonoBehaviour {

    public Vector3[] points;

    public BoxCollider bounds;

    // Use this for initialization
    void Start()
    {
        RecalculatePoints();
    }

	public void Reset()
    {
        bounds = GetComponent<BoxCollider>();
        bounds.size = new Vector3(1, 1, 1);
        bounds.center = Vector3.zero;

        RecalculatePoints();
    }

    public void RecalculateBounds()
    {
        Vector3 boundsSize;
        Vector3 center = Vector3.zero;

        boundsSize.x = Vector3.Distance(transform.TransformPoint(points[4]), transform.TransformPoint(points[5]));
        boundsSize.y = Vector3.Distance(transform.TransformPoint(points[0]), transform.TransformPoint(points[1]));
        boundsSize.z = Vector3.Distance(transform.TransformPoint(points[2]), transform.TransformPoint(points[3]));

        bounds.size = transform.InverseTransformVector(boundsSize);

        center.x = (points[4].x + points[5].x) / 2;
        center.y = (points[0].y + points[1].y) / 2;
        center.z = (points[2].z + points[3].z) / 2;

        bounds.center = center;
    }

    public void RecalculatePoints()
    {
        bounds = GetComponent<BoxCollider>();

        points = new Vector3[6]
        {
            (bounds.center + new Vector3(0, bounds.size.y, 0) * 0.5f),
            (bounds.center + new Vector3(0, -bounds.size.y, 0) * 0.5f),
            (bounds.center + new Vector3(0, 0, bounds.size.z) * 0.5f),
            (bounds.center + new Vector3(0, 0, -bounds.size.z) * 0.5f),
            (bounds.center + new Vector3(bounds.size.x, 0, 0) * 0.5f),
            (bounds.center + new Vector3(-bounds.size.x, 0, 0) * 0.5f)
        };
    }
}
