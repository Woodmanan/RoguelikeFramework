using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public struct Boon
{
    public float favorNeeded;
    public bool requiresSponsor;
    [SerializeReference]
    public List<Effect> givenEffects;
    public List<Ability> givenAbilities;
}

[CreateAssetMenu(fileName = "New God Definition", menuName = "ScriptableObjects/Gods/New God", order = 0)]
public class GodDefinition : ScriptableObject
{
    public LocalizedString localName;
    [HideInInspector]
    public string title;

    public float maxFavor;
    public float favorToSponsor;
    public AnimationCurve favorToGold;

    [SerializeReference]
    public List<GodScoringOperator> scoring;

    public List<Boon> boons;

    float[] scores;

    GodSystem system;

    int currentlyWatching = -1;

    public void Setup(GodSystem system)
    {
        this.system = system;

        foreach (GodScoringOperator scoringOperator in scoring)
        {
            scoringOperator.Setup(this);
        }

        scores = new float[system.numCandidates];
    }

    public void AddScoreToCurrent(float amount)
    {

    }

    public void EvaluateAllCandidates()
    {
        int maxIndex = 0;
        for (int i = 0; i < system.numCandidates; i++)
        {
            if (currentlyWatching != i)
            {
                scores[i] = StaticScore(system[i]);
            }
            if (scores[i] > scores[maxIndex])
            {
                maxIndex = i;
            }
        }

        WatchCandidate(maxIndex);
    }

    public float StaticScore(Monster monster)
    {
        float sum = 0;
        foreach (GodScoringOperator scoringOperator in scoring)
        {
            sum += scoringOperator.GetBaseScore(monster);
        }
        return sum;
    }

    public void WatchCandidate(int index)
    {
        currentlyWatching = index;
    }
}
