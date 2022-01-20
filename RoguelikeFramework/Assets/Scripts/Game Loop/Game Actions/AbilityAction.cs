using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityAction : GameAction
{
    public int abilityIndex;

    //Constuctor for the action; must include caller!
    public AbilityAction(int abilityIndex)
    {
        this.abilityIndex = abilityIndex;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        #if UNITY_EDITOR
        if (caller.abilities == null)
        {
            Debug.LogError($"A monster without abilites tried to activate ability {abilityIndex}", caller);
            yield break;
        }
        #else
        if (caller.abilites == null) yield break;
        #endif

        Ability toCast = caller.abilities[abilityIndex];

        caller.other = toCast.connections;
        bool keepCasting = true;
        AbilityAction action = this;
        caller.connections.OnCastAbility.BlendInvoke(toCast.connections.OnCastAbility, ref action, ref keepCasting);

        if (!keepCasting)
        {
            caller.other = null;
            yield break;
        }



        if (toCast.currentCooldown > 0)
        {
            Debug.Log($"Console: You cannot cast {toCast.displayName}, it still has {toCast.currentCooldown} turns left.");
            caller.other = null;
            yield break;
        }

        bool canFire = false;

        IEnumerator target = caller.controller.DetermineTarget(toCast.targeting, (b) => canFire = b);
        while (target.MoveNext())
        {
            yield return target.Current;
        }


        if (canFire)
        {
            //Ready to cast!
            caller.connections.OnTargetsSelected.BlendInvoke(toCast.connections.OnTargetsSelected, ref toCast.targeting, ref toCast);

            //Backwards, since they might remove themselves during this call.
            for (int i = toCast.targeting.affected.Count - 1; i >= 0; i--)
            {
                toCast.targeting.affected[i].connections.OnTargetedByAbility.Invoke(ref action);
            }

            //Take out the costs
            caller.LoseResources(toCast.stats.costs);

            caller.connections.OnPreCast.BlendInvoke(toCast.connections.OnPreCast, ref toCast);

            Debug.Log($"Console: {caller.GetFormattedName()} cast {toCast.displayName}!");
            toCast.Cast(caller);

            caller.connections.OnPostCast.BlendInvoke(toCast.connections.OnPostCast, ref toCast);

            for (int i = toCast.targeting.affected.Count - 1; i >= 0; i--)
            {
                toCast.targeting.affected[i].connections.OnHitByAbility.Invoke(ref action);
            }

            caller.energy -= 100;
        }

        caller.other = null;
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
