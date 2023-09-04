using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageScoringOption
{
    Constant,
    Log,
    Linear,
    Squared
}

[Group("Dynamic")]
public class ScoreDamage : GodScoringOperator
{
    public float minThreshhold;
    public DamageType type;
    public DamageSource source;

    public DamageScoringOption scalingType;

    public override void OnDealDamage(float damage, DamageType type, DamageSource source)
    {
        if (damage > minThreshhold && (this.type & type) > 0 & (this.source & source) > 0)
        {
            switch (scalingType)
            {
                default:
                case DamageScoringOption.Constant:
                    AddScoreToCurrent(weight);
                    return;
                case DamageScoringOption.Log:
                    AddScoreToCurrent(weight * Mathf.Log(damage, 2));
                    return;
                case DamageScoringOption.Linear:
                    AddScoreToCurrent(weight * damage);
                    return;
                case DamageScoringOption.Squared:
                    AddScoreToCurrent(weight * damage * damage);
                    return;

            }
        }
    }
}
