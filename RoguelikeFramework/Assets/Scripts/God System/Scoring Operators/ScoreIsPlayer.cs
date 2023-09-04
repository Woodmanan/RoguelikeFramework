using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Static")]
public class ScoreIsPlayer : GodScoringOperator
{
    public override float GetBaseScore(Monster monster)
    {
        if (monster as Player)
        {
            return weight;
        }
        return 0;
    }
}
