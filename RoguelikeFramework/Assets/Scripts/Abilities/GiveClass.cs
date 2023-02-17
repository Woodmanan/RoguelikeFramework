using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GiveClass", menuName = "Abilities/GiveClass", order = 1)]
public class GiveClass : Ability
{
    public Class classToGive;

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

    public override void OnCast(Monster caster)
    {
        if (caster == Player.player)
        {
            UIController.singleton.OpenClassPanel(classToGive);
        }
        else
        {
            classToGive.Apply(caster, giveItems:false);
        }
    }
}
