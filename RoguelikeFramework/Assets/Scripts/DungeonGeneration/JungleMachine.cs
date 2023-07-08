using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Branch/Jungle")]
public class JungleMachine : Machine
{
    public float minDistance;

    [Range(0, 100)]
    public float chanceForBrokenStatue;

    [Range(0, 100)]
    [Tooltip("The chance for a duplicate control point 1")]
    public float chanceForDuplicateC1;

    [Range(0, 100)]
    [Tooltip("The chance for a duplicate control point 2")]
    public float chanceForDuplicateC2;

    public RandomNumber bombSize;
    
    Vector2Int[] points;

    RogueBezier[] curves;

    Vector2Int branchPoint;

    const int numberOfStatues = 3;

    // Activate is called to start the machine
    public override IEnumerator Activate()
    {
        //Generate some connection points
        points = new Vector2Int[numberOfStatues];
        curves = new RogueBezier[numberOfStatues];

        for (int i = 0; i < numberOfStatues; i++)
        {
            points[i] = GeneratePoint(i);
            yield return null;
        }

        //Generate the branch point (root of bezier tree)
        branchPoint = RogueRNG.Linear(new Vector2Int(5, 4), new Vector2Int(generator.bounds.x - 5, 8));

        //Geneate (potentially overlapping) bezier points
        for (int i = 0; i < numberOfStatues; i++)
        {
            curves[i] = GenerateCurve(i);
            yield return null;
        }

        //Contour Bomb
        for (int i = 0; i < numberOfStatues; i++)
        {
            RogueBezier curve = curves[i];
            IEnumerator<Vector2Int> samples = curve.SampleCurve();
            while (samples.MoveNext())
            {
                Vector2Int location = samples.Current;
                float size = bombSize.EvaluateFloat();
                yield return null;

                BombArea(location, size);
                yield return null;
            }    
        }

        //Place stairs and statues

        //Check if we should break a statue

    }

    public Vector2Int GeneratePoint(int pointNumber)
    {
        bool valid = false;
        Vector2Int position = -Vector2Int.one;
        while (!valid)
        {
            valid = true;

            position = RogueRNG.Linear(new Vector2Int(0, generator.bounds.y / 2), generator.bounds);
            for (int i = 0; i < pointNumber; i++)
            {
                if (Vector2Int.Distance(points[i], position) < minDistance)
                {
                    valid = false;
                    break;
                }
            }
        }

        return position;
    }

    public RogueBezier GenerateCurve(int curveNumber)
    {
        Vector2Int start = branchPoint;
        Vector2Int end = points[curveNumber];

        Vector2Int C1;
        Vector2Int C2;

        if (curveNumber != 0 && RogueRNG.Linear(0, 100f) < chanceForDuplicateC1)
        {
            C1 = curves[curveNumber - 1].b;
        }
        else
        {
            C1 = RogueRNG.Linear(generator.bounds);
        }

        if (curveNumber != 0 && RogueRNG.Linear(0, 100f) < chanceForDuplicateC2)
        {
            C2 = curves[curveNumber - 1].c;
        }
        else
        {
            C2 = RogueRNG.Linear(generator.bounds);
        }

        return new RogueBezier(start, C1, C2, end);
    }

    public void BombArea(Vector2Int location, float size)
    {
        int iSize = Mathf.CeilToInt(size);
        Vector2Int lower = location - iSize * Vector2Int.one;
        Vector2Int upper = location + iSize * Vector2Int.one;

        lower.Clamp(Vector2Int.one, generator.bounds - 2 * Vector2Int.one);
        upper.Clamp(Vector2Int.one, generator.bounds - 2 * Vector2Int.one);

        Debug.Log($"Lower is {lower}, upper is {upper}");

        for (int i = lower.x; i <= upper.x; i++)
        {
            for (int j = lower.y; j <= upper.y; j++)
            {
                if (Vector2Int.Distance(new Vector2Int(i, j), location) < size)
                {
                    generator.map[i, j] = 1;
                }
            }
        }
        //generator.map[location.x, location.y] = 1;
    }

}
