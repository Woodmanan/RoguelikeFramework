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
    [HideInInspector]
    public bool given;
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
    public float giveGoldEvery;

    float favorSinceLastPayout;

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

        Debug.Log($"{name} has chosen the title {title}");
    }

    public void AddScoreToCurrent(float amount)
    {
        Debug.Log($"Added {amount} score!");
        favorSinceLastPayout += amount;
        scores[currentlyWatching] = Mathf.Clamp(scores[currentlyWatching] + amount, 0, maxFavor);

        if (WantsToSponsor())
        {
            OfferSponsorship();
        }

        EvaluateGold();

        EvaluateBoons();
    }

    public void EvaluateAllCandidates()
    {
        if (currentlySponsoring != -1 && system[currentlySponsoring].IsDead()) currentlySponsoring = -1;

        int maxIndex = Mathf.Max(0, currentlyWatching);
        for (int i = 0; i < system.numCandidates; i++)
        {
            if (system[i].IsDead())
            {
                scores[i] = float.NegativeInfinity;
            }
            else if (currentlyWatching != i)
            {
                scores[i] = Mathf.Clamp(RogueRNG.Normal(StaticScore(system[i]), scoringVariance), 0, maxFavor);
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
        if (currentlyWatching == index) return;

        if (system[index] == Player.player)
        {
            RogueLog.singleton.Log($"{title} is now watching you!", null, LogPriority.HIGH, LogDisplay.STANDARD);
        }

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

    public void SponsorCurrent()
    {
        if (currentlyWatching == -1) return;

        currentlySponsoring = currentlyWatching;

        EvaluateBoons();
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

    private bool WantsToSponsor()
    {
        if (currentlyWatching == -1 || currentlySponsoring != -1) return false;
        return scores[currentlyWatching] > favorToSponsor;
    }



    private void OfferSponsorship()
    {
        if (system[currentlyWatching] == Player.player)
        {
            RogueLog.singleton.Log($"{title} wants to sponsor you!", null, LogPriority.HIGH, LogDisplay.STANDARD);
        }
        currentlySponsoring = currentlyWatching;
    }

    private void EvaluateBoons()
    {
        bool isSponsoring = (currentlyWatching == currentlySponsoring);
        for (int i = boons.Count - 1; i >= 0; i--)
        {
            Boon boon = boons[i];
            if (scores[currentlyWatching] >= boon.favorNeeded && (!boon.requiresSponsor || isSponsoring) && !boon.given)
            {
                if (system[currentlyWatching] == Player.player)
                {
                    RogueLog.singleton.Log($"{title} gives you a boon!", null, LogPriority.HIGH, LogDisplay.STANDARD);
                }

                foreach (Ability ability in boon.givenAbilities)
                {
                    system.candidates[i]?.abilities?.AddAbility(ability.Instantiate());
                }

                foreach (Effect effect in boon.givenEffects)
                {
                    system.candidates[i]?.AddEffectInstantiate(effect);
                }
                boons.RemoveAt(i);
            }
        }
    }

    private void EvaluateGold()
    {
        if (favorSinceLastPayout >= giveGoldEvery)
        {
            favorSinceLastPayout -= giveGoldEvery;
            GiveGoldToCandidate(favorToGold.Evaluate(scores[currentlyWatching]));
            
        }
    }

    private void GiveGoldToCandidate(float gold)
    {
        if (system[currentlyWatching] == Player.player)
        {
            RogueLog.singleton.Log($"{title} is pleased with your peformance! They grant you {gold} coins!", null, LogPriority.HIGH, LogDisplay.STANDARD);
        }
    }
}

