using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public struct WightRitualCost
{
    public float baseCost;
    public int numTurns;
    public float costPerTurn;
    public LocalizedString description;
}

[CreateAssetMenu(fileName = "New WightRitual", menuName = "Abilities/WightRitual", order = 1)]
public class WightRitual : Ability
{
    Wight wightEffect;

    public List<WightRitualCost> ritualCosts;

    [SerializeReference]
    public Effect wightEffectToAdd;

    int level = 0;

    public override string GetDescription()
    {
        return ritualCosts[level].description.GetLocalizedString(this, ritualCosts[level]);
    }

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
        if (wightEffect == null)
        {
            wightEffect = caster.GetEffect<Wight>();
        }

        if (wightEffect != null)
        {
            level = wightEffect.level;
        }

        costs[Resources.MANA] = GetBaseCost();
    }

    public override IEnumerator OnCast(Monster caster)
    {
        for (int i = 0; i < GetNumTurns(); i++)
        {
            caster.energy -= 100;
            if (caster.baseStats[Resources.MANA] >= GetCostPerTurn())
            {
                caster.baseStats[Resources.MANA] -= GetCostPerTurn();
            }
            else
            {
                caster.baseStats[Resources.MANA] = 0;
                RogueLog.singleton.Log("You fail to complete the ritual.");
                yield break;
            }
            
            yield return GameAction.StateCheckAllowExit;
        }

        RogueLog.singleton.Log("You finish the ritual!");
        caster.AddEffectInstantiate(wightEffectToAdd);

        if (level == ritualCosts.Count - 1)
        {
            caster.abilities.RemoveAbility(this);
        }
    }

    public float GetBaseCost()
    {
        if (level >= 0 && level < ritualCosts.Count)
        {
            return ritualCosts[level].baseCost;
        }

        return 0;
    }

    public float GetCostPerTurn()
    {
        if (level >= 0 && level < ritualCosts.Count)
        {
            return ritualCosts[level].costPerTurn;
        }

        return 0;
    }

    public int GetNumTurns()
    {
        if (level >= 0 && level < ritualCosts.Count)
        {
            return ritualCosts[level].numTurns;
        }

        return 1;
    }
}
