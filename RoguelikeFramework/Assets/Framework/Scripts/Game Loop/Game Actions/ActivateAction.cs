using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAction : GameAction
{
    public int index;
    //Constuctor for the action
    public ActivateAction(int index)
    {
        //Construct me! Don't need caller here, that will get assigned during Setup.
        this.index = index;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        ItemStack stack = caller.inventory[index];
        if (stack == null)
        {
            caller.energy -= 100;
            Debug.LogError("Someone an empty stack. Draining to prevent softlock.", caller);
            yield break;
        }

        ActivatableItem item = stack.held[stack.count - 1].activatable;
        if (item == null)
        {
            caller.energy -= 100;
            Debug.LogError("Someone activate an item with no activatable? Draining to prevent softlock.", caller);
            yield break;
        }

        bool keepActivating = true;
        Item itemRef = stack.held[stack.count - 1];
        caller.connections.OnActivateItem.Invoke(ref itemRef, ref keepActivating);

        if (!keepActivating)
        {
            RogueLog.singleton.Log("You cannot activate this item right now (prevented by effect).", priority: LogPriority.IMPORTANT);
            yield break;
        }

        if ((item.activateType & ActivateType.Ability) > 0)
        {
            Ability abilityOnActivation = item.abilityOnActivation;
            abilityOnActivation.RegenerateStats(caller);
            if (abilityOnActivation.CheckAvailable(caller))
            {
                yield return SubAction(new AbilityAction(abilityOnActivation));
            }
            else
            {
                RogueLog.singleton.Log("You cannot activate this item right now.", priority: LogPriority.IMPORTANT);
                this.successful = false;
            }
        }

        if ((item.activateType & ActivateType.Effect) > 0)
        {
            caller.AddEffectInstantiate(item.activationEffects.ToArray());
        }

        if (this.successful)
        {
            RogueLog.singleton.Log($"You use the {item.GetComponent<Item>().GetName()}!", priority: LogPriority.GENERIC);
            caller.energy -= 100;
            //Remove the item!
            if (item.ConsumedOnUse)
            {
                caller.inventory.RemoveLastItemFromStack(index);
            }
        }
    }

    public override string GetDebugString()
    {
        return $"ActivateAction on index {index}: {caller.inventory[index].held[0].friendlyName}";
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
