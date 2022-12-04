using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Resources;

public class Combat
{
    public static AttackResult DetermineHit(Monster defender, WeaponBlock stats)
    {
        if (UnityEngine.Random.Range(0, 99.99f) < stats.chanceToHit && !GetDodged(defender, stats))
        {
            //We didn't miss!
            //TODO: Determine blocking stats
            return AttackResult.HIT;
        }
        else
        {
            return AttackResult.MISSED;
        }
    }

    public static void Hit(Monster attacker, Monster defender, DamageSource source, WeaponBlock stats)
    {
        foreach (DamagePairing damage in stats.damage)
        {
            defender.Damage(attacker, GetArmorDamageShave(defender, stats) * damage.damage.evaluate(), damage.type, source);
        }
    }

    public static float GetArmorDamageShave(float inArmor)
    {
        return 1.0f / Mathf.Pow(2, inArmor / 10);
    }

    public static float GetEvasionDodgeBar(float inEvasion)
    {
        return 100f / Mathf.Pow(2, inEvasion / 10);
    }

    public static bool GetDodged(Monster monster, WeaponBlock attack)
    {
        return UnityEngine.Random.Range(0, 99.99f) > GetEvasionDodgeBar(monster.currentStats[EV] - attack.accuracy);
    }

    public static float GetArmorDamageShave(Monster monster, WeaponBlock attack)
    {
        return GetArmorDamageShave(monster.currentStats[AC] - attack.piercing);
    }
}

[Serializable]
public struct DamagePairing
{
    public Roll damage;
    public DamageType type;
}

[Serializable]
public struct ChanceEffect
{
    public string name;
    public float percentChance;
    [SerializeReference] public List<Effect> appliedEffects;

    public bool evaluate()
    {
        float amt = UnityEngine.Random.Range(0f, 99.999f); //99 so that 100% always succeeds
        return amt < percentChance; 
    }
}

[Serializable]
public class ChanceEffectList : ICollection
{
    public List<ChanceEffect> list;

    public ChanceEffect this[int index]
    {
        get { return list[index]; }
        set { list[index] = value; }
    }

    public void Add(ChanceEffect item)
    {
        list.Add(item);
    }

    public void Remove(ChanceEffect item)
    {
        list.Remove(item);
    }

    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
    }

    public int Count
    {
        get { return list.Count; }
    }

    void ICollection.CopyTo(Array array, int index)
    {
        int count = index;
        foreach (ChanceEffect stat in list)
        {
            array.SetValue(stat, count);
            count++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return list.GetEnumerator();
    }

    bool ICollection.IsSynchronized
    {
        get
        {
            return false;
        }
    }

    object ICollection.SyncRoot
    {
        get
        {
            return this;
        }
    }

    // The Count read-only property returns the number
    // of items in the collection. - Microsoft docs
    //https://i.kym-cdn.com/entries/icons/original/000/031/254/cover3.jpg
    int ICollection.Count
    {
        get
        {
            return list.Count;
        }
    }

    public ChanceEffect[] ToArray()
    {
        return list.ToArray();
    }

    public void Clear()
    {
        list.Clear();
    }
}


