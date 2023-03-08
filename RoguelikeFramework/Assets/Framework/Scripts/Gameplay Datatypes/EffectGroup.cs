using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Class for holding a large group of effects, and some convenience
//functions for accessing those effects. Mostly used as a cleanup tool.

[System.Serializable]
public struct EffectRarityPairing
{
    public ItemRarity rarity;
    [SerializeReference]
    public Effect effect;
}

[CreateAssetMenu(fileName = "New EffectGroup", menuName = "ScriptableObjects/Effect Group", order = 2)]
public class EffectGroup : ScriptableObject
{
    [SerializeField]
    List<EffectRarityPairing> effects;

    public int favorLastX;
    public float favorChance;

    public Effect GetRandomEffect()
    {
        if (effects.Count == 0)
        {
            Debug.LogError("No effects in this grouping!", this);
            return null;
        }
        return effects[RogueRNG.Linear(0, effects.Count)].effect;
    }

    public Effect GetRandomEffectWithRarity(ItemSpawnInfo rarities)
    {
        if (favorLastX > 0 && Random.value < (favorChance/ 100))
        {
            int start = effects.Count - favorLastX;
            return effects[RogueRNG.Linear(start, effects.Count)].effect;
        }

        ItemRarity toGet = rarities.GetRarity();
        List<EffectRarityPairing> options = effects.Where(x => x.rarity == toGet).ToList();
        while (options.Count == 0)
        {
            if (toGet == ItemRarity.COMMON)
            {
                Debug.LogError("Paired effects could not return even a common effect!");
                return GetRandomEffect();
            }
            toGet--;
            options = effects.Where(x => x.rarity == toGet).ToList();
        }

        return options[RogueRNG.Linear(0, options.Count)].effect;
    }

    public Effect GetRandomEffectInstantiated()
    {
        if (effects.Count == 0)
        {
            Debug.LogError("No effects in this grouping!", this);
            return null;
        }
        return effects[RogueRNG.Linear(0, effects.Count)].effect.Instantiate();
    }
}
