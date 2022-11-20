using UnityEngine;

public static class GameplayExtensions
{
    public static float GameDistance(this Vector2Int current, Vector2Int other)
    {
        switch (Map.space)
        {
            case MapSpace.Chebyshev:
                int x = Mathf.Abs(current.x - other.x);
                int y = Mathf.Abs(current.y - other.y);
                return Mathf.Max(x, y);
            case MapSpace.Manhattan:
                x = Mathf.Abs(current.x - other.x);
                y = Mathf.Abs(current.x - other.x);
                return x + y;
            case MapSpace.Euclidean:
                return Vector2Int.Distance(current, other);
            default:
                return -1;
        }
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