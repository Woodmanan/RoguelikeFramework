using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resources;

[CreateAssetMenu(fileName = "Fireball", menuName = "Abilities/Elemental/Fireball", order = 1)]
public class Fireball : Ability
{
    [SerializeField] Sprite[] sprites;

    public override bool OnCheckActivationSoft(Monster caster)
    {
        return true;
    }

    public override IEnumerator OnCast(Monster caster)
    {
        AnimationController.AddAnimationSolo(new ProjectileBresenhamAnim(caster.location, targeting.points[0], 30, sprites));
        AnimationController.AddAnimationSolo(new ExplosionAnimation(targeting.points[0], targeting.radius, targeting, sprites));

        foreach (Monster m in targeting.affected)
        {
            m.Damage(caster, currentStats[POWER], DamageType.FIRE, DamageSource.ABILITY);
        }

        yield break;
    }
}
