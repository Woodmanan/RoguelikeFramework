using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AbilityStats : ISerializationCallbackReceiver
{
    [HideInInspector][SerializeField]
    List<AbilityResources> _keys = new List<AbilityResources>();
    [HideInInspector][SerializeField]
    List<float> _vals = new List<float>();

    //Unity doesn't know how to serialize a Dictionary
    public Dictionary<AbilityResources, float> dictionary = new Dictionary<AbilityResources, float>();

    public float this[AbilityResources r]
    {
        get
        {
            float val;
            if (dictionary.TryGetValue(r, out val))
            {
                return val;
            }
            return 0;
        }
        set
        {
            dictionary[r] = value;
        }
    }

    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _vals.Clear();

        //Sort, to preserve good ordering in editor
        foreach (var kvp in dictionary)
        {
            _keys.Add(kvp.Key);
        }
        _keys.Sort();

        //Add vals based on sorted list
        foreach (var key in _keys)
        {
            _vals.Add(dictionary[key]);
        }
    }

    public void OnAfterDeserialize()
    {
        dictionary = new Dictionary<AbilityResources, float>();

        for (int i = 0; i != Mathf.Min(_keys.Count, _vals.Count); i++)
            dictionary.Add(_keys[i], _vals[i]);

        _keys.Clear();
        _vals.Clear();
    }

    public AbilityStats Copy()
    {
        AbilityStats copy = new AbilityStats();
        foreach (AbilityResources r in dictionary.Keys)
        {
            copy[r] = dictionary[r];
        }
        return copy;
    }

    public static AbilityStats operator +(AbilityStats first, AbilityStats second)
    {
        foreach (AbilityResources r in second.dictionary.Keys)
        {
            first[r] += second[r];
        }
        return first;
    }

    public static AbilityStats operator -(AbilityStats first, AbilityStats second)
    {
        foreach (AbilityResources r in second.dictionary.Keys)
        {
            first[r] -= second[r];
        }
        return first;
    }

    public static AbilityStats operator *(AbilityStats first, float value)
    {
        AbilityStats result = new AbilityStats();

        foreach (AbilityResources r in first.dictionary.Keys)
        {
            result[r] = first[r] * value;
        }

        return result;
    }

    public static AbilityStats operator *(float value, AbilityStats stats)
    {
        return stats * value;
    }

    public static AbilityStats Lerp(AbilityStats a, AbilityStats b, float t)
    {
        return (1f - t) * a + t * b;
    }
}