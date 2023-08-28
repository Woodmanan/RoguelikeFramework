using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AbilityBlock
{
    public int cooldown;
    public float power;

    public static AbilityBlock operator +(AbilityBlock a, AbilityBlock b)
    {
        AbilityBlock toReturn = new AbilityBlock();
        toReturn.cooldown = a.cooldown + b.cooldown;
        toReturn.power = a.power + b.power;
        return toReturn;
    }

    public static AbilityBlock operator -(AbilityBlock a, AbilityBlock b)
    {
        AbilityBlock toReturn = new AbilityBlock();
        toReturn.cooldown = a.cooldown - b.cooldown;
        toReturn.power = a.power - b.power;
        return toReturn;
    }
}

//These things generally don't get edited, so I'm not worrying about figuring out how to add these things
[Serializable]
public class WeaponBlock
{
    //Primary Attributes
    public float chanceToHit;
    public int accuracy;
    public int piercing;
    public float energyCost = 100f;
    public List<DamagePairing> damage;
    public RogueTagContainer tags;
}
