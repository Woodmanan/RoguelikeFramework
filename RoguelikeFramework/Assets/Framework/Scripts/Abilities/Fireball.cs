using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resources;

[CreateAssetMenu(fileName = "Fireball", menuName = "Abilities/Elemental/Fireball", order = 1)]
public class Fireball : Ability
{
    [SerializeField] Sprite[] sprites;

    public override bool OnCheckActivationSoft(RogueHandle<Monster> caster)
    {
        return true;
    }

    public override IEnumerator OnCast(RogueHandle<Monster> caster)
    {
        AnimationController.AddAnimationSolo(new ProjectileBresenhamAnim(caster[0].location, targeting.points[0], 30, sprites));
        AnimationController.AddAnimationSolo(new ExplosionAnimation(targeting.points[0], targeting.radius, targeting, sprites));

        foreach (RogueHandle<Monster> m in targeting.affected)
        {
            m[0].Damage(caster, currentStats[POWER], DamageType.FIRE, DamageSource.ABILITY);
        }

        yield break;
    }
}
