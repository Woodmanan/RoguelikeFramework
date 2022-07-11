using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fireball", menuName = "Abilities/Elemental/Fireball", order = 1)]
public class Fireball : Ability
{
    [SerializeField] Sprite[] sprites;

    public override bool OnCheckActivationSoft(Monster caster)
    {
        return true;
    }

    public override void OnCast(Monster caster)
    {
        Debug.Log($"Fireball was able to hit {targeting.affected.Count} enemies");
        foreach (Monster m in targeting.affected)
        {
            m.Damage(caster, (int) stats.power, DamageType.CUTTING, DamageSource.ABILITY);
        }

        AnimationController.AddAnimation(new ProjectileBresenhamAnim(caster.location, targeting.points[0], 12, sprites));
        AnimationController.AddAnimation(new ExplosionAnimation(targeting.points[0], targeting.radius, targeting, sprites));
    }
}
