using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SwitchCastResource", menuName = "Abilities/SwitchCastResource", order = 1)]
public class SwitchCastResource : Ability
{
    public Ability below;
    public Ability above;

    public Resources resource;

    public float checkAmount;

    //Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    public override bool OnCheckActivationSoft(Monster caster)
    {
        if (caster.currentStats[resource] < checkAmount)
        {
            return below.OnCheckActivationSoft(caster);
        }
        else
        {
            return above.OnCheckActivationSoft(caster);
        }
    }

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    public override bool OnCheckActivationHard(Monster caster)
    {
        if (caster.currentStats[resource] < checkAmount)
        {
            return below.OnCheckActivationHard(caster);
        }
        else
        {
            return above.OnCheckActivationHard(caster);
        }
    }

    public override void OnRegenerateStats(Monster caster)
    {
        if (caster.currentStats[resource] < checkAmount)
        {
            below.RegenerateStats(caster);
            //Below!
            targeting = below.targeting;
            baseStats = below.baseStats;
            currentStats = below.currentStats;
        }
        else
        {
            above.RegenerateStats(caster);
            targeting = above.targeting;
            baseStats = above.baseStats;
            currentStats = above.currentStats;
        }
    }

    public override void OnSetup()
    {
        below = below.Instantiate();
        above = above.Instantiate();
    }

    public override void OnCast(Monster caster)
    {
        if (caster.currentStats[resource] < checkAmount)
        {
            //Below!
            below.targeting = targeting;
            below.Cast(caster);
            currentCooldown = below.currentCooldown;
            below.currentCooldown = 0;
        }
        else
        {
            above.targeting = targeting;
            above.Cast(caster);
            currentCooldown = above.currentCooldown;
            above.currentCooldown = 0;
        }
    }

}
