using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Special mixed/static dynamic - info is generated dynamically and used to update scores

[Group("Static")]
public class ScoreKilledCurrentCandidate : GodScoringOperator
{
    [HideInInspector]
    public List<Monster> killedWatching = new List<Monster>();
    [HideInInspector]
    public List<Monster> killedSponsored = new List<Monster>();

    public float sponsorMultiplier = 5f;

    public override void OnKilled(Monster killer)
    {
        if (connectedGod.IsSponsoring(connectedMonster))
        {
            killedSponsored.Add(killer);
        }
        else
        {
            killedWatching.Add(killer);
        }
    }

    public override float GetBaseScore(Monster monster)
    {
        if (killedSponsored.Contains(monster))
        {
            return weight * sponsorMultiplier;
        }
        else if (killedWatching.Contains(monster))
        {
            return weight;
        }

        return 0;
    }
}
