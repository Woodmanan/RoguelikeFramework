using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fireball", menuName = "Abilities/Elemental/Fireball", order = 1)]
public class Fireball : Ability
{
    [SerializeField] Sprite[] sprites;

    public override bool OnCheckActivationSoft(Monster caster)
    {
        return (caster.location.x + caster.location.y) % 2 == 0;
    }

    public override void OnCast(Monster caster)
    {
        foreach (Monster m in targeting.affected)
        {
            m.Damage(caster, (int) stats.power, DamageType.CUTTING, DamageSource.ABILITY);
        }

        AnimationController.AddAnimation(new ExplosionAnimation(targeting.points[0], targeting.radius, null, sprites));
    }
}
