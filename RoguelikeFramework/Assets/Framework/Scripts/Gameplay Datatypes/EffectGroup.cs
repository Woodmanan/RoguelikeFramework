using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class for holding a large group of effects, and some convenience
//functions for accessing those effects. Mostly used as a cleanup tool.

[CreateAssetMenu(fileName = "New EffectGroup", menuName = "ScriptableObjects/Effect Group", order = 2)]
public class EffectGroup : ScriptableObject
{
    [SerializeReference]
    List<Effect> effects;

    public Effect GetRandomEffect()
    {
        if (effects.Count == 0)
        {
            Debug.LogError("No effects in this grouping!", this);
            return null;
        }
        return effects[RogueRNG.Linear(0, effects.Count)];
    }

    public Effect GetRandomEffectInstantiated()
    {
        if (effects.Count == 0)
        {
            Debug.LogError("No effects in this grouping!", this);
            return null;
        }
        return effects[RogueRNG.Linear(0, effects.Count)].Instantiate();
    }
}
