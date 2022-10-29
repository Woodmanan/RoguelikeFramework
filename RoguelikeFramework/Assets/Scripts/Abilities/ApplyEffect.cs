using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ApplyEffect", menuName = "Abilities/ApplyEffect", order = 1)]
public class ApplyEffect : Ability
{
    [SerializeReference]
    public List<Effect> effectsToApply;
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

    public override void OnCast(Monster caster)
    {
        foreach (Effect e in effectsToApply)
        {
            caster.AddEffect(e);
        }
    }
}
