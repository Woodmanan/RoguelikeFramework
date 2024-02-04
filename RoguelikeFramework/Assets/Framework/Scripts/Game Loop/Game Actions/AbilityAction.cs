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
            Debug.LogError($"Monster {caller[0].friendlyName} tried to cast a null ability.", caller[0].unity);
            caller[0].energy -= 1;
            yield break;
        }

        caller[0].AddConnection(toCast.connections);

        if (toCast.currentCooldown > 0)
        {
            RogueLog.singleton.Log($"You cannot cast {toCast.GetName()}, it still has {toCast.currentCooldown} turns of cooldown.");
            caller[0].RemoveConnection(toCast.connections);
            successful = false;
            yield break;
        }

        if (!toCast.castable)
        {
            RogueLog.singleton.Log($"You cannot cast {toCast.GetName()}.");
            caller[0].RemoveConnection(toCast.connections);
            successful = false;
            yield break;
        }

        AbilityAction action = this;

        { //Caller override check
            
            bool keepCasting = true;
            
            caller[0].connections.OnCastAbility.BlendInvoke(toCast.connections.OnCastAbility, ref action, ref keepCasting);

            if (!keepCasting)
            {
                caller[0].RemoveConnection(toCast.connections);
                successful = false;
                yield break;
            }
        }

        bool canFire = false;

        IEnumerator target = caller[0].controller.DetermineTarget(toCast.targeting, (b) => canFire = b, toCast.IsValidTarget);
        while (target.MoveNext())
        {
            yield return target.Current;
        }


        if (canFire)
        {
            //Ready to cast!
            caller[0].connections.OnTargetsSelected.BlendInvoke(toCast.connections.OnTargetsSelected, ref toCast.targeting, ref toCast);

            //Backwards, since they might remove themselves during this call.
            for (int i = toCast.targeting.affected.Count - 1; i >= 0; i--)
            {
                toCast.targeting.affected[i][0].connections.OnTargetedByAbility.Invoke(ref action);
            }

            //Take out the costs
            caller[0].LoseResources(toCast.costs);

            caller[0].connections.OnPreCast.BlendInvoke(toCast.connections.OnPreCast, ref toCast);

            if (!toCast.locName.IsEmpty)
            {
                string logString = LogFormatting.GetFormattedString("CastFullString", new { caster = caller[0].GetName(), singular = caller[0].singular, spell = toCast.GetName() });
                RogueLog.singleton.Log(logString, priority: LogPriority.IMPORTANT);
            }

            GenerateAnimations(toCast);
            
            
            IEnumerator castRoutine = toCast.Cast(caller);
            while (castRoutine.MoveNext())
            {
                yield return castRoutine.Current;
            }

            caller[0].connections.OnPostCast.BlendInvoke(toCast.connections.OnPostCast, ref toCast);

            for (int i = toCast.targeting.affected.Count - 1; i >= 0; i--)
            {
                toCast.targeting.affected[i][0].connections.OnHitByAbility.Invoke(ref action);
            }

            caller[0].energy -= toCast.EnergyCost;
        }
        else
        {
            successful = false;
        }

        caller[0].RemoveConnection(toCast.connections);
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {
        #if UNITY_EDITOR
        if (caller[0].abilities == null)
        {
            Debug.LogError($"A monster without abilites tried to activate ability {abilityIndex}", caller[0].unity);
            return;
        }
        #else
        if (caller[0].abilities == null) return;
        #endif
        if (toCast == null)
        {
            if (caller[0].abilities.HasAbility(abilityIndex))
            {
                toCast = caller[0].abilities[abilityIndex];
            }
        }
    }

    public override string GetDebugString()
    {
        return string.Format("Ability Action: Casting {0}", toCast.friendlyName);
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