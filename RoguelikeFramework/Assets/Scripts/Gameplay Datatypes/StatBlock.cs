using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class StatBlock
{
    public ResourceList resources;
    public int ac; //Armor Class
    public int ev; //Evasion

    public static StatBlock operator +(StatBlock a, StatBlock b)
    {
        StatBlock toReturn = new StatBlock();
        toReturn.resources = a.resources + b.resources;
        toReturn.ac = a.ac + b.ac;
        toReturn.ev = a.ev + b.ev;
        return toReturn;
    }

    public static StatBlock operator -(StatBlock a, StatBlock b)
    {
        StatBlock toReturn = new StatBlock();
        toReturn.resources = a.resources - b.resources;
        toReturn.ac = a.ac - b.ac;
        toReturn.ev = a.ev - b.ev;
        return toReturn;
    }
}
[Serializable]
public class AbilityBlock
{
    public ResourceList costs;
    public int cooldown;
    public float power;

    public static AbilityBlock operator +(AbilityBlock a, AbilityBlock b)
    {
        AbilityBlock toReturn = new AbilityBlock();
        toReturn.costs = a.costs + b.costs;
        toReturn.cooldown = a.cooldown + b.cooldown;
        toReturn.power = a.power + b.power;
        return toReturn;
    }

    public static AbilityBlock operator -(AbilityBlock a, AbilityBlock b)
    {
        AbilityBlock toReturn = new AbilityBlock();
        toReturn.costs = a.costs - b.costs;
        toReturn.cooldown = a.cooldown - b.cooldown;
        toReturn.power = a.power - b.power;
        return toReturn;
    }
}
