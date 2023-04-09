using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DumpInstability", menuName = "Abilities/DumpInstability", order = 1)]
public class DumpInstability : Ability
{
    Instability cachedInstability;
    public int numStacksNeededForCast;
    public int cooldownToSet;
    public RandomNumber damage;

	//Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    public override bool OnCheckActivationSoft(Monster caster)
    {
        cachedInstability = caster.GetEffect<Instability>();
        if (cachedInstability != null && cachedInstability.numStacks >= numStacksNeededForCast)
        {
            return true;
        }
        return false;
    }

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    public override bool OnCheckActivationHard(Monster caster)
    {
        return true;
    }

    public override IEnumerator OnCast(Monster caster)
    {
        foreach (Monster target in targeting.affected)
        {
            target.Damage(caster, damage.Evaluate(), DamageType.MAGICAL, DamageSource.ABILITY);
            Clear(target);
        }
        Clear(caster);
        if (cachedInstability == null) cachedInstability = caster.GetEffect<Instability>();
        cachedInstability?.ClearStacks();

        yield break;
    }

    public void Clear(Monster monster)
    {
        monster.baseStats[Resources.MANA] = 0;
        foreach (Ability ability in monster.abilities.GetAbilitiesAsEnumerable())
        {
            ability.currentCooldown = cooldownToSet;
        }
    }

}
