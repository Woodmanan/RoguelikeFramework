﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SwitchCastResource", menuName = "Abilities/SwitchCastResource", order = 1)]
public class SwitchCastResource : Ability
{
    public Ability below;
    public Ability above;

    public Resources resource;

    public float checkAmount;

    bool isBelow = true;

    public override string GetName(bool shorten = false)
    {
        if (isBelow)
        {
            return below.GetName();
        }
        else
        {
            return above.GetName();
        }
    }

    public override string GetDescription()
    {
        if (isBelow)
        {
            return below.GetDescription();
        }
        else
        {
            return above.GetDescription();
        }
    }

    public override Sprite GetImage()
    {
        if (isBelow)
        {
            return below.GetImage();
        }
        else
        {
            return above.GetImage();
        }
    }

    //Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    public override bool OnCheckActivationSoft(Monster caster)
    {
        if (isBelow)
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
        if (isBelow)
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
        bool previous = isBelow;
        if (caster && caster.currentStats[resource] < checkAmount)
        {
            below.RegenerateStats(caster);
            //Below!
            targeting = below.targeting;
            baseStats = below.baseStats;
            currentStats = below.currentStats;
            costs = below.costs;
            isBelow = true;
        }
        else
        {
            above.RegenerateStats(caster);
            targeting = above.targeting;
            baseStats = above.baseStats;
            currentStats = above.currentStats;
            costs = above.costs;
            isBelow = false;
        }

        if (previous != isBelow)
        {
            dirty = true;
        }
    }

    public override void OnSetup()
    {
        below = below.Instantiate();
        above = above.Instantiate();
    }

    public override IEnumerator OnCast(Monster caster)
    {
        IEnumerator subroutine;
        if (isBelow)
        {
            //Below!
            below.targeting = targeting;
            currentCooldown = below.currentCooldown;
            below.currentCooldown = 0;
            subroutine = below.Cast(caster);
            while (subroutine.MoveNext())
            {
                yield return subroutine.Current;
            }
        }
        else
        {
            above.targeting = targeting;
            currentCooldown = above.currentCooldown;
            above.currentCooldown = 0;
            subroutine = above.Cast(caster);
            while (subroutine.MoveNext())
            {
                yield return subroutine.Current;
            }
        }
    }

}
