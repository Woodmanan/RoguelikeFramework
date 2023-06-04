using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resources;

[CreateAssetMenu(fileName = "New DeathMark", menuName = "Abilities/Classes/Necromancer/DeathMark", order = 1)]
public class DeathMarkAbility : Ability
{
    [SerializeField]
    RogueTagContainer exludeTags;

    public float percentHealthNeeded;

    [SerializeReference]
    public Effect effectToApply;

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
        return !exludeTags.MatchAnyTags(target.tags, TagMatch.Parental) &&
               (target.baseStats[HEALTH] / target.currentStats[MAX_HEALTH]) > (percentHealthNeeded / 100);
    }

    public override void OnRegenerateStats(Monster caster)
    {
        
    }

    public override IEnumerator OnCast(Monster caster)
    {
        foreach (Monster target in targeting.affected)
        {
            Effect effect = effectToApply.Instantiate();
            effect.credit = caster;
            target.AddEffect(effect);
        }
        yield break;
    }
}
