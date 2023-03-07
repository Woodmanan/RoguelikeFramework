using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DragonsBreath", menuName = "Abilities/Heat Mage/DragonsBreath", order = 5)]
public class DragonsBreath : Ability
{
    public RandomNumber damage;
    public float heatToAdd;
    public Sprite[] sprites;

    //Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    public override bool OnCheckActivationSoft(Monster caster)
    {
        return true;
    }

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    public override bool OnCheckActivationHard(Monster caster)
    {
        return true;
    }

    public override void OnRegenerateStats(Monster caster)
    {
        
    }

    public override IEnumerator OnCast(Monster caster)
    {
        AnimationController.AddAnimation(new ExplosionAnimation(caster.location, targeting.radius, targeting, sprites));
        caster.AddBaseStat(Resources.HEAT, heatToAdd);
        foreach (Monster m in targeting.affected)
        {
            m.Damage(caster, damage.Evaluate(), DamageType.FIRE, DamageSource.ABILITY);
        }
        yield break;
    }
}
