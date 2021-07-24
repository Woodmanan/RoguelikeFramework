using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct DamagePairing
{
    public Roll damage;
    public DamageType type;
}


[Serializable]
public struct ChanceEffect
{
    public float percentChance;
    public List<StatusEffect> appliedEffects;

    public bool evaluate()
    {
        float amt = UnityEngine.Random.Range(0f, 100f);
        return amt < percentChance;
    }
}


