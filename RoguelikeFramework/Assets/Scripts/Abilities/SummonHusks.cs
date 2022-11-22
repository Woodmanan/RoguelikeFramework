using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SummonHusks", menuName = "Abilities/SummonHusks", order = 1)]
public class SummonHusks : Ability
{
    [SerializeReference] public Effect summonEffect;
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
        if (targeting.affected.Count == 0)
        {
            Debug.LogError("Can't summon husks on no targets!");
        }

        foreach (Monster target in targeting.affected)
        {
            Effect toAdd = summonEffect.Instantiate();
            toAdd.credit = caster;
            target.AddEffect(toAdd);
        }


        Debug.Log($"Console: The {caster.name} screams! You hear a swarm incoming.");
    }
}
