using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GodScoringOperator
{
    [SerializeField] [HideInInspector]
    protected float weight;
    [HideInInspector]
    public GodDefinition connectedGod;
    [HideInInspector]
    public Monster connectedMonster;

    public virtual void Setup(GodDefinition parent)
    {
        connectedGod = parent;
    }

    public virtual float GetBaseScore(Monster monster)
    {
        return 0;
    }

    public virtual void OnKillMonster(Monster killed) { }

    public virtual void OnKilled(Monster killer) { }

    public virtual void OnDealDamage(float damage, DamageType type, DamageSource source) { }
    public virtual void OnChangeFloors(Map newFloor) { }
    public virtual void OnOfferSponsorship(GodDefinition sponsor) { }
    public virtual void OnBecomeSponsored(GodDefinition sponsor) { }
    public virtual void OnTurnEnd() { }

    public void AddScoreToCurrent(float amount)
    {
        connectedGod.AddScoreToCurrent(amount);
    }
}