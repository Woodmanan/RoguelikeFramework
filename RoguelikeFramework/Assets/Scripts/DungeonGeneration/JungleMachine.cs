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
    [Tooltip("The chance for a duplicate branch point")]
    public float chanceForDuplicateB1;

    [Range(0, 100)]
    [Tooltip("The chance for a duplicate control point 1")]
    public float chanceForDuplicateC1;

    [Range(0, 100)]
    [Tooltip("The chance for a duplicate control point 2")]
    public float chanceForDuplicateC2;

    public float bombSizeMean;

    public int statueIndex;
    
    Vector2Int[] points;

    Vector2Int[] statues;

    RogueBezier[] curves;

    Vector2Int branchPoint;

    Vector2Int branchEnd;

    RogueBezier branchCurve;

    const int numberOfStatues = 3;

    // Activate is called to start the machine
    public override IEnumerator Activate()
    {
        //Generate some connection points
        points = new Vector2Int[numberOfStatues];
        curves = new RogueBezier[numberOfStatues];
        statues = new Vector2Int[numberOfStatues];

        for (int i = 0; i < numberOfStatues; i++)
        {
            points[i] = GeneratePoint(i);
            yield return null;
        }

        //Generate the branch point (root of bezier tree)
        branchPoint = RogueRNG.Linear(new Vector2Int(5, 4), new Vector2Int(generator.bounds.x - 5, 8));

        int boundY = generator.bounds.y;
        branchEnd = RogueRNG.Linear(new Vector2Int(5, boundY - 8), new Vector2Int(generator.bounds.x - 5, boundY - 4));

        branchCurve = new RogueBezier(branchPoint, RogueRNG.Linear(generator.bounds), RogueRNG.Linear(generator.bounds), branchEnd);

        //Geneate (potentially overlapping) bezier points
        for (int i = 0; i < numberOfStatues; i++)
        {
            curves[i] = GenerateCurve(i);
            yield return null;
        }

        //Contour bomb main path
        {
            IEnumerator<Vector2Int> samples = branchCurve.SampleCurve();
            while (samples.MoveNext())
            {
                Vector2Int location = samples.Current;
                float size = Mathf.Max(1, RogueRNG.Exponential(bombSizeMean));
                yield return null;

                BombArea(location, size);
                yield return null;
            }
        }

        //Contour bomb side paths
        for (int i = 0; i < numberOfStatues; i++)
        {
            RogueBezier curve = curves[i];
            IEnumerator<Vector2Int> samples = curve.SampleCurve();
            while (samples.MoveNext())
            {
                Vector2Int location = samples.Current;
                float size = Mathf.Max(1, RogueRNG.Exponential(bombSizeMean));
                yield return null;

                BombArea(location, size);
                yield return null;
            }    
        }

        //Place stairs
        generator.desiredInStairs.Add(branchPoint + Vector2Int.left);
        generator.desiredInStairs.Add(branchPoint);
        generator.desiredInStairs.Add(branchPoint + Vector2Int.right);

        generator.desiredOutStairs.AddRange(points);

        //Place statues, check first if we should break one
        

        TotemType[] totems = World.current.BlackboardRead<TotemType[]>($"{generator.name} Totems");

        for (int i = 0; i < numberOfStatues; i++)
        {
            PlaceStair(i);
            yield return null;
        }

    }

    public Vector2Int GeneratePoint(int pointNumber)
    {
        bool valid = false;
        Vector2Int position = -Vector2Int.one;
        while (!valid)
        {
            valid = true;

            position = RogueRNG.LinearOnBorder(new Vector2Int(0, generator.bounds.y / 2), generator.bounds, 5);
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
        Vector2Int end = points[curveNumber];
        Vector2Int start, C1, C2;

        if (curveNumber != 0 && RogueRNG.Linear(0, 100f) < chanceForDuplicateB1)
        {
            start = curves[curveNumber - 1].a;
        }
        else
        {
            start = branchCurve.EvaluateClamped(RogueRNG.Linear(0.0f, 1.0f));
        }

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

    void PlaceStair(int index)
    {
        Vector2Int location = points[index];
        List<Vector2Int> offsets = new List<Vector2Int> { Vector2Int.left, Vector2Int.down, Vector2Int.right, Vector2Int.up };
        offsets.Sort((x, y) => RogueRNG.Linear(-1, 2));

        foreach (Vector2Int offset in offsets)
        {
            Vector2Int newLoc = location + offset;
            if (generator.map[newLoc.x, newLoc.y] == 0)
            {
                generator.map[newLoc.x, newLoc.y] = statueIndex;
                statues[index] = newLoc;
                return;
            }
        }

        //Surrounded by open spaces - pick any!
        location += offsets[0];
        generator.map[location.x, location.y] = statueIndex;
        statues[index] = location;
    }

    public override void PostActivation(Map m)
    {
        int brokenIndex = -1;
        if (RogueRNG.Linear(0, 100f) < chanceForBrokenStatue)
        {
            brokenIndex = RogueRNG.Linear(numberOfStatues);
        }

        TotemType[] totems = World.current.BlackboardRead<TotemType[]>($"{generator.name} Totems");

        for (int i = 0; i < numberOfStatues; i++)
        {
            TotemType totem = (brokenIndex == i) ? TotemType.Broken : totems[i];
            JungleStatueTile tile = m.GetTile(statues[i]) as JungleStatueTile;
            tile.SetSpriteForTotem(totem);
        }
    }
}
