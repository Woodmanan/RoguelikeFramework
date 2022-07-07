using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntExtensions
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
}