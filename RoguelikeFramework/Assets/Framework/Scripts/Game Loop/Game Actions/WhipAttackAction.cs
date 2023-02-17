using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhipAttackAction : AttackAction
{

    public WhipAttackAction(Monster target)
    {
        //Construct me! Assigns caller by default in the base class
        this.target = target;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        //See if we have any weapons actively equipped, or unarmed slots that can attack
        if (caller.equipment == null)
        {
            Debug.LogError("Armless things can't attack! Wasting its turn");
            caller.energy -= 100;
            yield break;
        }

        int distance = GameplayExtensions.ChebyshevDistance(caller.location, target.location);
        List<EquipmentSlot> slots = caller.equipment.equipmentSlots.FindAll(x => x.active && x.equipped.held[0].type == ItemType.MELEE_WEAPON);

        if (distance <= 1)
        {
            Debug.LogError($"{caller.friendlyName} performed a whip attack at range {distance}? This seems incorrect, but I'll let it slide.", caller);
        }

        //Do we have any whips equipped?
        if (slots.Count > 0)
        {
            //Begin attack
            foreach (EquipmentSlot s in slots)
            {
                if (!primaryWeapons.Contains(s.equipped.held[0].melee) && !secondaryWeapons.Contains(s.equipped.held[0].melee))
                {
                    WhipPassive passive = IsAWhip(s.equipped.held[0]);
                    if (passive != null && passive.range >= distance)
                    {
                        if (s.type.Contains(EquipSlotType.PRIMARY_HAND))
                        {
                            primaryWeapons.Add(s.equipped.held[0].melee);
                        }
                        else
                        {
                            secondaryWeapons.Add(s.equipped.held[0].melee);
                        }
                    }
                }
            }

            AttackAction action = this;

            caller.connections.OnGenerateArmedAttacks.Invoke(ref action, ref primaryWeapons, ref secondaryWeapons);

            if (animates)
            {
                AnimationController.AddAnimation(new AttackAnimation(caller, target));
            }

            foreach (Weapon w in primaryWeapons)
            {
                w.PrimaryAttack(caller, target, this);
            }

            foreach (Weapon w in secondaryWeapons)
            {
                w.SecondaryAttack(caller, target, this);
            }

            caller.energy -= 100;
        }
        else
        {
            Debug.Log("Your can't whip attack with a ranged weapon!");
            if (caller == Player.player)
            {
                yield break;
            }

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("Assuming a monster did this. Taking its turn as retribution! (PLS Fix)");
            #endif
            caller.energy -= 100;

            yield break;
        }
    }

    WhipPassive IsAWhip(Item item)
    {
        if (item.equipable)
        {
            foreach (Effect e in item.equipable.addedEffects)
            {
                WhipPassive passive = e as WhipPassive;
                if (passive != null)
                {
                    return passive;
                }
            }
        }
        return null;
    }


    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}