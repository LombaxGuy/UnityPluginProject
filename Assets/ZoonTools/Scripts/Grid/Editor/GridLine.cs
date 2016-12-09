using UnityEngine;

public struct GridLine
{
    private Vector3 startPosition;
    private Vector3 endPosition;

    public Vector3 StartPosition
    {
        get { return startPosition; }
    }

    public Vector3 EndPosition
    {
        get { return endPosition; }
    }

    public GridLine(Vector3 startPosition, Vector3 endPosition)
    {
        this.startPosition = startPosition;
        this.endPosition = endPosition;
    }
}
