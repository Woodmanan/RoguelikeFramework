using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Candidate Factory", menuName = "ScriptableObjects/Gods/New Candidate Factory", order = 1)]
public class CandidateFactory : ScriptableObject
{
    public Monster template;

    public Monster CreateRandomCandidate()
    {
        Monster baseMonster = template.Instantiate();
        baseMonster.friendlyName = $"Candidate #{RogueRNG.Linear(0, 100)}";
        baseMonster.faction = Faction.NONE; //No faction for these guys
        return baseMonster;
    }
}
