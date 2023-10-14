using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GodSystem : DungeonSystem
{
    public GodDefinition[] gods;
    public List<Monster> candidates;

    public int numCandidates;

    public CandidateFactory candidateFactory;

    [SerializeReference]
    public Effect listenerEffect;

    int lowestVisitedLevel = -1;

    public override void OnSetup(World world, Branch branch = null, Map map = null)
    {
        Debug.Assert(world != null);

        gods = gods.Select(x => GodDefinition.Instantiate(x)).ToArray();

        //Instantiate premade profiles
        candidates = candidates.Select(x => x.Instantiate()).ToList();

        //Add the player profile
        candidates.Insert(0, Player.player);

        //Fill in the remaining number of random candidates
        for (int i = candidates.Count; i < numCandidates; i++)
        {
            candidates.Add(candidateFactory.CreateRandomCandidate());
        }

        //Attach listeners
        foreach (Monster monster in candidates)
        {
            monster.Setup();
            monster?.AddEffectInstantiate(listenerEffect);
        }

        //Setup
        foreach (GodDefinition definition in gods)
        {
            definition.Setup(this);
        }

        world.BlackboardWrite("GodSystem", this);

        //OnEnterLevel(Map.current);

        //PerformScoringPass();
    }

    public void PerformScoringPass()
    {
        foreach (GodDefinition definition in gods)
        {
            definition.EvaluateAllCandidates();
        }
    }

    public override void OnExitLevel(Map m)
    {
        for (int i = 1; i < numCandidates; i++)
        {
            m.monsters.Remove(candidates[i]);
        }
    }

    public override void OnEnterLevel(Map m)
    {
        //Move everyone up
        for (int i = 1; i < numCandidates; i++)
        {
            if (!candidates[i].IsDead())
            {
                Vector2Int location = m.GetRandomWalkableTile();
                candidates[i].transform.parent = m.monsterContainer;
                m.monsters.Add(candidates[i]);
                candidates[i].SetPositionSnap(location);
            }
        }

        if (m.index > lowestVisitedLevel)
        {
            lowestVisitedLevel = m.index;
            PerformScoringPass();
        }
    }

    public bool IsCandidate(Monster monster)
    {
        return GetIndexOf(monster) >= 0;
    }

    public int GetIndexOf(Monster monster)
    {
        return candidates.IndexOf(monster);
    }

    public Monster this[int index]
    {
        get { return candidates[index]; }
        set { candidates[index] = value; }
    }

}
