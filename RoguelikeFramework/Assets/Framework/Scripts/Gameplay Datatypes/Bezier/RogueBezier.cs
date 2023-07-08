using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RogueBezier
{
    public Vector2Int a;
    public Vector2Int b;
    public Vector2Int c;
    public Vector2Int d;

    public RogueBezier(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }

    public Vector2 Evaluate(float alpha)
    {
        return Vector2.Lerp(Vector2.Lerp(a, b, alpha), Vector2.Lerp(c, d, alpha), alpha);
    }

    public Vector2Int EvaluateClamped(float alpha)
    {
        return Vector2Int.RoundToInt(Evaluate(alpha));
    }

    public IEnumerator<Vector2Int> SampleCurve(float step = -1f)
    {
        if (step <= 0)
        {
            step = (Vector2Int.Distance(a, b) + Vector2Int.Distance(b, c) + Vector2Int.Distance(c, d));

            //IQ magic - if above m, round to never go below n
            float m = 12;
            float n = 8;

            if (step <= m)
            {
                float clampA = 2 * n - m;
                float clampB = 2 * m - 3 * n;
                float t = step / m;
                step = (clampA * t + clampB) * t * t + n;
            }

            //Invert to get step size
            step = 1f / step;
        }

        float total = 0.0f;
        while (total < 1.0f)
        {
            total = Mathf.Clamp(total + step, 0, 1);
            
            yield return EvaluateClamped(total);
        }
    }
}
