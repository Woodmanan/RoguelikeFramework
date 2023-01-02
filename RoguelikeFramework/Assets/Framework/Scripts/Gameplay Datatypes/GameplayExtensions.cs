using UnityEngine;

public static class GameplayExtensions
{
    public static float GameDistance(this Vector2Int current, Vector2Int other)
    {
        switch (Map.space)
        {
            case MapSpace.Chebyshev:
                return ChebyshevDistance(current, other);
            case MapSpace.Manhattan:
                return ManhattanDistance(current, other);
            case MapSpace.Euclidean:
                return Vector2Int.Distance(current, other);
            default:
                return -1;
        }
    }

    //Normalize for chebyshev space - length is greater of x or y, so vector becomes 8-way direction vector;
    public static Vector2Int ChebyshevNormalize(this Vector2Int current)
    {
        current.x = Mathf.Clamp(current.x, -1, 1);
        current.y = Mathf.Clamp(current.y, -1, 1);
        return current;
    }

    public static int ChebyshevDistance(Vector2Int start, Vector2Int end)
    {
        int x = Mathf.Abs(start.x - end.x);
        int y = Mathf.Abs(start.y - end.y);
        return Mathf.Max(x, y);
    }

    public static int ManhattanDistance(Vector2Int start, Vector2Int end)
    {
        int x = Mathf.Abs(start.x - end.x);
        int y = Mathf.Abs(start.y - end.y);
        return x + y;
    }

    public static float Cross(this Vector2Int current, Vector2Int other)
    {
        return current.x * other.y - current.y * other.x;
    }

    public static (Rect, Rect) SplitOnX(this Rect inputRect, float splitWidth)
    {
        Rect left = new Rect(inputRect);
        left.width = splitWidth;

        Rect right = new Rect(inputRect);
        right.width = (right.width - splitWidth);
        right.x = inputRect.x + splitWidth;

        return (left, right);
    }

    public static (Rect, Rect) SplitOnY(this Rect inputRect, float splitHeight)
    {
        Rect bottom = new Rect(inputRect);
        bottom.height = splitHeight;

        Rect top = new Rect(inputRect);
        top.height = (top.height - splitHeight);
        top.y = inputRect.y + splitHeight;

        return (bottom, top);
    }


}