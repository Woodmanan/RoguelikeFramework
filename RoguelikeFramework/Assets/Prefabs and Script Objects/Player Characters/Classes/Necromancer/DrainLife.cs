using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New DrainLife", menuName = "Abilities/Classes/Necromancer/DrainLife", order = 1)]
public class DrainLife : Ability
{
    public float percentHealthDamage;
    public RogueTagContainer excludeTags;

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

    public override bool IsValidTarget(Monster target)
    {
        return !excludeTags.MatchAnyTags(target.tags, TagMatch.Parental);
    }

    public override void OnRegenerateStats(Monster caster)
    {
        
    }

    public override IEnumerator OnCast(Monster caster)
    {
        Vector2Int mainTargetPoint = targeting.points[0];
        Monster mainTarget = Map.current.GetTile(mainTargetPoint).currentlyStanding;

        float health = mainTarget.baseStats[Resources.HEALTH] * percentHealthDamage / 100;
        mainTarget.Damage(caster, health, DamageType.TRUE, DamageSource.ABILITY);

        List<Monster> healTargets = targeting.affected;
        healTargets.Remove(mainTarget);
        healTargets.Where((x) => x.tags.MatchAnyTags("Monster.Undead", TagMatch.Parental) && !x.IsEnemy(caster)).ToList();

        foreach (Monster target in healTargets)
        {
            target.Heal(health / healTargets.Count, false);
        }

        yield break;
    }
}
