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
    [Header("Naming info")]
    public LocalizedString localName;
    public LocalizedString format;
    public LocalizedString first;
    public LocalizedString last;
    public LocalizedString prefix;
    public LocalizedString suffix;

    [HideInInspector]
    public string title;


    public float maxFavor;
    public float favorToSponsor;
    public float scoringVariance; //How fickle a god is
    public AnimationCurve favorToGold;

    [SerializeReference]
    public List<GodScoringOperator> scoring;

    public List<Boon> boons;

    float[] scores;

    GodSystem system;

    int currentlyWatching = -1;
    int currentlySponsoring = -1;

    RogueTag listenerTag;

    public void Setup(GodSystem system)
    {
        listenerTag = new RogueTag("GodSystem.Listener");
        this.system = system;

        foreach (GodScoringOperator scoringOperator in scoring)
        {
            scoringOperator.Setup(this);
        }

        scores = new float[system.numCandidates];

        title = GenerateName();

        Debug.Log($"{name} has chosen the title {}");
    }

    public void AddScoreToCurrent(float amount)
    {
        Debug.Log($"Added {amount} score!");
        scores[currentlyWatching] += amount;
    }

    public void EvaluateAllCandidates()
    {
        int maxIndex = Mathf.Max(0, currentlyWatching);
        for (int i = 0; i < system.numCandidates; i++)
        {
            if (system[i].IsDead())
            {
                scores[i] = float.NegativeInfinity;
            }
            else if (currentlyWatching != i)
            {
                scores[i] = RogueRNG.Normal(StaticScore(system[i]), scoringVariance);
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
        Debug.Log($"God {name} is now watching {index}:{system[index].friendlyName} with score {scores[index]}");

        if (currentlyWatching == index) return;

        UnattachListener(currentlyWatching);        
        currentlyWatching = index;
        AttachListener(currentlyWatching);
    }

    public void UnattachListener(int index)
    {
        if (index >= 0 && index < system.numCandidates)
        {
            //Do the thing!
            GodSystemListener listener = system[index].GetEffectByTag(listenerTag) as GodSystemListener;
            listener?.DetachGod(this);
        }
    }

    public void AttachListener(int index)
    {
        if (index >= 0 && index < system.numCandidates)
        {
            GodSystemListener listener = system[index].GetEffectByTag(listenerTag) as GodSystemListener;
            listener?.AttachGod(this);
        }
    }

    public Monster GetCurrentlySponsoring()
    {
        if (currentlySponsoring == -1) return null;
        return system[currentlySponsoring];
    }

    public bool IsSponsoring(Monster monster)
    {
        return monster == GetCurrentlySponsoring();
    }

    public string GenerateName()
    {
        return format.GetLocalizedString(new { first = first.IsEmpty ? "" : first.GetLocalizedString(), 
                                                last = last.IsEmpty  ? "" : last.GetLocalizedString(), 
                                                prefix = prefix.IsEmpty ? "" : prefix.GetLocalizedString(), 
                                                suffix = suffix.IsEmpty ? "" : suffix.GetLocalizedString()
        });
    }
}

