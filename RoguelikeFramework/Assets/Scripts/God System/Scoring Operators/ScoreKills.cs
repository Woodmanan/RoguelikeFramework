using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Dynamic")]
public class ScoreKills : GodScoringOperator
{
    Faction factionToKill;
    public override void OnKillMonster(Monster killed)
    {
        if ((killed.faction & factionToKill) > 0)
        {
            AddScoreToCurrent(weight);
        }
    }
}
