using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RogueTagContainer : ISerializationCallbackReceiver
{
    [HideInInspector] [SerializeField]
    List<RogueTag> _keys = new List<RogueTag>();
    [HideInInspector] [SerializeField]
    List<int> _vals = new List<int>();

    Dictionary<RogueTag, int> counts = new Dictionary<RogueTag, int>();

    RogueTagContainer()
    {
        _keys = new List<RogueTag>();
        _vals = new List<int>();

        counts = new Dictionary<RogueTag, int>();
    }

    RogueTagContainer(params string[] tags)
    {
        foreach (string tag in tags)
        {
            AddTag(new RogueTag(tag));
        }
    }

    public RogueTagContainer(params RogueTag[] tags)
    {
        foreach (RogueTag tag in tags)
        {
            AddTag(tag);
        }
    }

    public void OnBeforeSerialize()
    {
        if (_keys == null) _keys = new List<RogueTag>();
        if (_vals == null) _vals = new List<int>();
        if (counts == null) counts = new Dictionary<RogueTag, int>();
        _keys.Clear();
        _vals.Clear();

        //Sort, to preserve good ordering in editor
        foreach (var kvp in counts)
        {
            if (kvp.Value != 0) //Trim out keys that we're not storing any info of!
            {
                _keys.Add(kvp.Key);
            }
        }
        //_keys.Sort();

        //Add vals based on sorted list
        foreach (var key in _keys)
        {
            _vals.Add(counts[key]);
        }
    }

    public void OnAfterDeserialize()
    {
        counts = new Dictionary<RogueTag, int>();

        for (int i = 0; i != Mathf.Min(_keys.Count, _vals.Count); i++)
            counts.Add(_keys[i], _vals[i]);

        _keys.Clear();
        _vals.Clear();
    }

    public void AddTag(string tag)
    {
        AddTag(new RogueTag(tag));
    }

    public void RemoveTag(string tag)
    {
        RemoveTag(new RogueTag(tag));
    }

    public bool HasTag(string tag)
    {
        return HasTag(new RogueTag(tag));
    }

    public void AddTag(RogueTag tag)
    {
        int outValue;
        if (counts.TryGetValue(tag, out outValue))
        {
            outValue += 1;
            if (outValue == 0)
            {
                counts.Remove(tag);
            }
            else
            {
                counts[tag] = outValue;
            }
        }
        else
        {
            counts.Add(tag, 1);
        }
    }

    public void RemoveTag(RogueTag tag)
    {
        int outValue;
        if (counts.TryGetValue(tag, out outValue))
        {
            outValue -= 1;
            if (outValue == 0)
            {
                counts.Remove(tag);
            }
            else
            {
                counts[tag] = outValue;
            }
        }
        else
        {
            counts.Add(tag, -1);
        }
    }

    public bool HasTag(RogueTag tag)
    {
        int outValue;
        return counts.TryGetValue(tag, out outValue) && outValue > 0;
    }

    public bool MatchAnyTags(string tag, TagMatch matchType)
    {
        return MatchAnyTags(new RogueTag(tag), matchType);
    }
    
    public bool MatchAnyTags(RogueTag tag, TagMatch matchType)
    {
        foreach (RogueTag key in counts.Keys)
        {
            if (tag.IsMatch(key, matchType) && HasTag(key)) return true;
        }
        return false;
    }

    public bool MatchAllTags(RogueTag tag, TagMatch matchType)
    {
        foreach (RogueTag key in counts.Keys)
        {
            if (!tag.IsMatch(key, matchType) || !HasTag(key)) return false;
        }
        return true;
    }

    public bool MatchAnyTags(RogueTagContainer other, TagMatch matchType)
    {
        foreach (RogueTag tag in counts.Keys)
        {
            if (other.MatchAnyTags(tag, matchType)) return true;
        }
        return false;
    }

    public bool MatchAllTags(RogueTagContainer other, TagMatch matchType)
    {
        foreach (RogueTag tag in counts.Keys)
        {
            if (!other.MatchAnyTags(tag, matchType)) return false;
        }
        return true;
    }

    public bool IsEmpty
    {
        get { return counts.Count == 0; }
    }

    public int Count
    {
        get { return counts.Count; }
    }
}
