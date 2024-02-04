using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ApplyEffect", menuName = "Abilities/ApplyEffect", order = 1)]
public class ApplyEffect : Ability
{
    [SerializeReference]
    public List<Effect> effectsToApply;

    [SerializeReference]
    public List<Effect> effectsToApplyToCaster;

    [SerializeField]
    DamagePairing damage;

    [SerializeField]
    RogueTagContainer RequireTags;

	//Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    public override bool OnCheckActivationSoft(RogueHandle<Monster> caster)
    {
        return true;
    }

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    public override bool OnCheckActivationHard(RogueHandle<Monster> caster)
    {
        return true;
    }

    public override bool IsValidTarget(RogueHandle<Monster> target)
    {
        if (RequireTags.IsEmpty)
        {
            return true;
        }
        return RequireTags.MatchAnyTags(target[0].tags, TagMatch.Parental);
    }

    public override IEnumerator OnCast(RogueHandle<Monster> caster)
    {
        foreach (RogueHandle<Monster> target in targeting.affected)
        {
            Monster monster = target.value;
            foreach (Effect e in effectsToApply)
            {
                Effect toAdd = e.Instantiate();
                toAdd.credit = caster;
                monster.AddEffect(toAdd);
            }

            if (damage.damage.dice > 0 && damage.damage.rolls > 0)
            {
                monster.Damage(caster, damage.damage.evaluate(), damage.type, DamageSource.ABILITY);
            }
        }

        Monster casterMonster = caster.value;
        foreach (Effect e in effectsToApplyToCaster)
        {
            Effect toAdd = e.Instantiate();
            toAdd.credit = caster;
            casterMonster.AddEffect(toAdd);
        }
        yield break;
    }
}
