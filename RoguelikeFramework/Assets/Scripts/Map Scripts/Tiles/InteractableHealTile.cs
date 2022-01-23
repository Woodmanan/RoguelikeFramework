using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableHealTile : InteractableTile
{
    public int healAmount;
    /*public override float Inspect(Monster toInspect)
    {
        return base.Inspect(toInspect);
    }*/

    public override GameAction GetAction()
    {
        ActionPlan plan = new ActionPlan();
        plan.AddAction(new PathfindAction(location));
        return plan;
    }

    public override IEnumerator Interact(Monster caller)
    {
        Debug.Log($"{caller.name} is healed!");
        caller.Heal(healAmount);
        caller.energy -= 100;
        yield break;
    }
}
