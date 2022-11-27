using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Stats : ISerializationCallbackReceiver
{
    [HideInInspector][SerializeField]
    List<Resources> _keys = new List<Resources>();
    [HideInInspector][SerializeField]
    List<float> _vals = new List<float>();

    //Unity doesn't know how to serialize a Dictionary
    public Dictionary<Resources, float> dictionary = new Dictionary<Resources, float>();

    public float this[Resources r]
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
        dictionary = new Dictionary<Resources, float>();

        for (int i = 0; i != Mathf.Min(_keys.Count, _vals.Count); i++)
            dictionary.Add(_keys[i], _vals[i]);

        _keys.Clear();
        _vals.Clear();
    }

    public Stats Copy()
    {
        Stats copy = new Stats();
        foreach (Resources r in dictionary.Keys)
        {
            copy[r] = dictionary[r];
        }
        return copy;
    }

    public static Stats operator +(Stats first, Stats second)
    {
        foreach (Resources r in second.dictionary.Keys)
        {
            first[r] += second[r];
        }
        return first;
    }

    public static Stats operator -(Stats first, Stats second)
    {
        foreach (Resources r in second.dictionary.Keys)
        {
            first[r] -= second[r];
        }
        return first;
    }

    public static Stats operator /(Stats first, float value)
    {
        Stats result = new Stats();

        foreach (Resources r in first.dictionary.Keys)
        {
            result[r] = first[r] / value;
        }

        return result;
    }

    public static Stats operator *(Stats first, float value)
    {
        Stats result = new Stats();

        foreach (Resources r in first.dictionary.Keys)
        {
            result[r] = first[r] * value;
        }

        return result;
    }
}