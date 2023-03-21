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
        foreach (Monster target in targeting.affected)
        {
            foreach (Effect e in effectsToApply)
            {
                Effect toAdd = e.Instantiate();
                toAdd.credit = caster;
                target.AddEffect(toAdd);
            }
        }

        foreach (Effect e in effectsToApplyToCaster)
        {
            Effect toAdd = e.Instantiate();
            toAdd.credit = caster;
            caster.AddEffect(toAdd);
        }
        yield break;
    }
}
