using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GiveClass", menuName = "Abilities/GiveClass", order = 1)]
public class GiveClass : Ability
{
    public Class classToGive;

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

    public override void OnRegenerateStats(RogueHandle<Monster> caster)
    {
        
    }

    public override IEnumerator OnCast(RogueHandle<Monster> caster)
    {
        if (caster == Player.player)
        {
            UIController.singleton.OpenClassPanel(classToGive);
        }
        else
        {
            classToGive.Apply(caster, giveItems:false);
        }
        yield break;
    }
}
