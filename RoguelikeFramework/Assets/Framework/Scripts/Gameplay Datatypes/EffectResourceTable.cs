using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
 * Resource table for effects
 * 
 * Should get bundled with the game and editor (belongs in Resource folder)
 * 
 * Main idea is to allow for a "Standardized Effect" which is
 * always the same thing - IE, an attack can apply "Light Stun" rather
 * than applying an entirely new effect from the stun class.
 * 
 */

[System.Serializable]
public struct ResourceEffect
{
    public string name;
    [SerializeReference]
    public Effect effect;
}

[System.Serializable]
public struct ResourceEffectKey
{
    public string name;
}

[CreateAssetMenu(fileName = "New Effect Resource Table", menuName = "ScriptableObjects/Effect Resource Table", order = 3)]
public class EffectResourceTable : ScriptableObject
{
    public List<ResourceEffect> effects;

    List<string> names;

    public Effect GetEffect(ResourceEffectKey key)
    {
        if (names == null || names.Count != effects.Count)
        {
            effects.Sort((x, y) => string.Compare(x.name, y.name));
            names = effects.Select((x) => x.name).ToList();
        }
        int index = names.BinarySearch(key.name);
        if (index >= 0 && index < effects.Count)
        {
            return effects[index].effect;
        }
        else
        {
            return null;
        }
    }

    public List<string> GetNames()
    {
        if (names == null || names.Count != effects.Count)
        {
            effects.Sort((x, y) => string.Compare(x.name, y.name));
            names = effects.Select((x) => x.name).ToList();
        }
        return names;
    }
}
