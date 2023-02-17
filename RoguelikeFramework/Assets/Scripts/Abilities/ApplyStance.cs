using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This ability is the same type as the "Apply effect" section of abilities,
 * but enforces that the passive effect given from this ability can only come from 
 * one source at a time.
 */

[CreateAssetMenu(fileName = "New ApplyStance", menuName = "Abilities/ApplyStance", order = 1)]
public class ApplyStance : Ability
{
    //The dictionary that is used to track which monsters have which effects
    public static Dictionary<Monster, Effect> currentStances = new Dictionary<Monster, Effect>();

    [SerializeReference]
    public Effect stance;
    
    Effect lastGivenStance;

	//Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    public override bool OnCheckActivationSoft(Monster caster)
    {
        return true;
    }

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    public override bool OnCheckActivationHard(Monster caster)
    {
        Effect casterCurrentStance = null;
        if (currentStances.TryGetValue(caster, out casterCurrentStance))
        {
            if (casterCurrentStance == lastGivenStance)
            {
                return false;
            }
        }

        //If we got here, we definitely don't need lastGivenStance - monster has no stance, or that stance isn't ours. Drop it for GC.
        lastGivenStance = null;
        return true;
    }

    public override void OnCast(Monster caster)
    {
        Effect casterCurrentStance = null;
        if (currentStances.TryGetValue(caster, out casterCurrentStance))
        {
            casterCurrentStance.Disconnect();
            currentStances.Remove(caster);
        }

        lastGivenStance = stance.Instantiate();
        caster.AddEffect(lastGivenStance);
        currentStances.Add(caster, lastGivenStance);
    }
}
