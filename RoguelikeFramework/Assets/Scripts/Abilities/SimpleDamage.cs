using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SimpleDamage", menuName = "Abilities/SimpleDamage", order = 1)]
public class SimpleDamage : Ability
{
    public DamagePairing damage;

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

    public override IEnumerator OnCast(Monster caster)
    {
        //Anim before damage so death animations line up
        foreach (Monster target in targeting.affected)
        {
            target.Damage(caster, damage.damage.evaluate() + currentStats[AbilityResources.POWER], damage.type, DamageSource.ABILITY);
        }
        yield break;
    }
}
