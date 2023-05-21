using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TagMatch
{
    Exact,
    Familial,
    Parental
}

[System.Serializable]
public struct RogueTag : ISerializationCallbackReceiver, System.IEquatable<RogueTag>
{
    public static List<Dictionary<string, int>> nameContainers = new List<Dictionary<string, int>>();
    private static Dictionary<string, RogueTag> createdTags = new Dictionary<string, RogueTag>(256);
    public static ulong[] masks = new ulong[] {0x0000000000000000L,
                                               0x000000000000FFFFL,
                                               0x00000000FFFFFFFFL,
                                               0x0000FFFFFFFFFFFFL,
                                               0xFFFFFFFFFFFFFFFFL };

    //Max value is 1/4th of a ulong
    public const int maxValue = (1 << 16) - 1;
    const int maxDegree = 4;

    ulong value;
    int degree;
    [SerializeField]
    string name;

    public RogueTag(string name)
    {    
        if (createdTags.TryGetValue(name, out this))
        {
            return;
        }

        this.name = name;
        value = 0;
        degree = 0;
        UpdateValueFromName();
        createdTags.Add(name, this);
    }

    void UpdateValueFromName()
    {
        string[] names = name.Split('.');
        value = 0;
        if (names.Length > maxDegree)
        {
            Debug.LogError($"RogueTag will not fully use {name}, because it contains more than >{maxDegree} categories ({names.Length} in this name). Names larger than 'X.Y.Z.W' will lose their later categories.");
        }

        degree = Mathf.Min(names.Length, maxDegree);

        for (int i = 0; i < degree; i++)
        {
            ulong subValue = (ulong)DetermineValue(names[i], i);
            value |= subValue << (i * 16);
        }
    }

    public int DetermineValue(string name, int degree)
    {
        EnsureContainerForDegree(degree);

        Dictionary<string, int> container = nameContainers[degree];

        int outValue = 0;
        if (container.TryGetValue(name, out outValue))
        {
            return outValue;
        }
        else
        {
            if (container.Count == maxValue)
            {
                throw new System.Exception($"Tag container at index {degree} has reached the maximum allowable tags!");
            }
            outValue = container.Count;
            container.Add(name, outValue);
            return outValue;
        }
    }

    public ulong GetInternalValue()
    {
        return value;
    }

    public int GetInternalDegree()
    {
        return degree;
    }

    public bool IsMatch(RogueTag other, TagMatch matchType)
    {
        switch(matchType)
        {
            case TagMatch.Exact:
                return IsExactMatch(other);
            case TagMatch.Familial:
                return IsFamilialMatch(other);
            case TagMatch.Parental:
                return IsParentalMatch(other);
            default:
                return false;
        }
    }

    public bool IsExactMatch(RogueTag other)
    {
        return value == other.value;
    }

    //Are these a familial match? Returns true if equal, or one is a parent to the other.
    public bool IsFamilialMatch(RogueTag other)
    {
        int degreeToCheck = Mathf.Min(degree, other.degree);
        ulong mask = masks[degreeToCheck];
        return (other.value & mask) == (value & mask);
    }

    //Return true if this tag is equal or a parent to the child
    public bool IsParentalMatch(RogueTag child)
    {
        ulong mask = masks[degree];
        return (child.value & mask) == (value & mask);
    }

    public static void EnsureContainerForDegree(int degree)
    {
        if (nameContainers.Count <= degree)
        {
            for (int i = 0; i < ((degree + 1) - nameContainers.Count); i++)
            {
                Dictionary<string, int> newDict = new Dictionary<string, int>(128);
                newDict.Add("INTERNAL_ZERO", 0); //Reserve the zero value so that empties are never matches
                nameContainers.Add(newDict);
            }
        }
    }

    public static void FlushTagCache()
    {
        nameContainers.Clear();
        createdTags.Clear();
    }
    
    //Warning - extremely expensive! Gets the human readable name of this string.
    public string GetHumanName()
    {
        return name;
    }

    //Names should always be good - but if it's not, try to rebuild it from value.
    public void OnBeforeSerialize()
    {
        if (name == null)
        {
            string[] names = new string[degree];
            EnsureContainerForDegree(4);
            for (int i = 0; i < degree; i++)
            {
                int valueToFind = (int)(value >> (16 * i) & masks[1]);
                names[i] = nameContainers[i].FirstOrDefault(x => x.Value == valueToFind).Key;
            }
            name = string.Join(".", names);
        }
    }

    //Convert our names back into our value - try to cache as we're streaming them out
    public void OnAfterDeserialize()
    {
        string holdName = name;
        if (createdTags.TryGetValue(name, out this))
        {
            return;
        }
        this.name = holdName;
        UpdateValueFromName();
        createdTags.Add(name, this);
    }

    public bool Equals(RogueTag t) => value == t.GetInternalValue() && degree == t.GetInternalDegree();

    public override int GetHashCode() => (value, degree).GetHashCode();

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public static bool operator ==(RogueTag lhs, RogueTag rhs) => lhs.Equals(rhs);

    public static bool operator !=(RogueTag lhs, RogueTag rhs) => !(lhs == rhs);
}
