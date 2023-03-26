using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityAction : GameAction
{
    public Ability toCast;
    int abilityIndex;

    //Constuctor for the action; must include caller!
    public AbilityAction(int abilityIndex)
    {
        this.abilityIndex = abilityIndex;
    }

    public AbilityAction(Ability toCast)
    {
        this.toCast = toCast;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        if (toCast == null)
        {
            Debug.LogError($"Monster {caller.name} tried to cast a null ability.", caller);
            caller.energy -= 1;
            yield break;
        }

        caller.AddConnection(toCast.connections);

        if (toCast.currentCooldown > 0)
        {
            RogueLog.singleton.Log($"You cannot cast {toCast.GetName()}, it still has {toCast.currentCooldown} turns of cooldown.");
            caller.RemoveConnection(toCast.connections);
            successful = false;
            yield break;
        }

        if (!toCast.castable)
        {
            RogueLog.singleton.Log($"You cannot cast {toCast.GetName()}.");
            caller.RemoveConnection(toCast.connections);
            successful = false;
            yield break;
        }

        AbilityAction action = this;

        { //Caller override check
            
            bool keepCasting = true;
            
            caller.connections.OnCastAbility.BlendInvoke(toCast.connections.OnCastAbility, ref action, ref keepCasting);

            if (!keepCasting)
            {
                caller.RemoveConnection(toCast.connections);
                successful = false;
                yield break;
            }
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
            caller.LoseResources(toCast.costs);

            caller.connections.OnPreCast.BlendInvoke(toCast.connections.OnPreCast, ref toCast);

            if (!toCast.locName.IsEmpty)
            {
                RogueLog.singleton.Log($"The {caller.GetFormattedName()} casts {toCast.GetName()}!", priority: LogPriority.HIGH, display: LogDisplay.ABILITY);
            }

            GenerateAnimations(toCast);
            
            
            IEnumerator castRoutine = toCast.Cast(caller);
            while (castRoutine.MoveNext())
            {
                yield return castRoutine.Current;
            }

            caller.connections.OnPostCast.BlendInvoke(toCast.connections.OnPostCast, ref toCast);

            for (int i = toCast.targeting.affected.Count - 1; i >= 0; i--)
            {
                toCast.targeting.affected[i].connections.OnHitByAbility.Invoke(ref action);
            }

            caller.energy -= 100;
        }
        else
        {
            successful = false;
        }

        caller.RemoveConnection(toCast.connections);
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {
        #if UNITY_EDITOR
        if (caller.abilities == null)
        {
            Debug.LogError($"A monster without abilites tried to activate ability {abilityIndex}", caller);
            return;
        }
        #else
        if (caller.abilities == null) return;
        #endif
        if (toCast == null)
        {
            if (caller.abilities.HasAbility(abilityIndex))
            {
                toCast = caller.abilities[abilityIndex];
            }
        }
    }

    public void GenerateAnimations(Ability toCast)
    {
        if (toCast.animations.Count > 0)
        {
            foreach (TargetingAnimation anim in toCast.animations)
            {
                TargetingAnimation copy = anim.Instantiate();
                copy.GenerateFromTargeting(toCast.targeting, 0, caller);
                AnimationController.AddAnimationSolo(copy);
            }
        }
    }
}