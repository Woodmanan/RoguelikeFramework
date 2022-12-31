using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : GameAction
{
    public Monster target;
    public List<Weapon> primaryWeapons = new List<Weapon>();
    public List<Weapon> secondaryWeapons = new List<Weapon>();

    public bool animates = true;

    public List<EquipmentSlot> unarmedSlots = new List<EquipmentSlot>();

    public AttackAction()
    {

    }

    public AttackAction(Monster target)
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
        List<EquipmentSlot> slots = caller.equipment.equipmentSlots.FindAll(x => x.active && x.equipped.held[0].type == ItemType.MELEE_WEAPON);
        unarmedSlots = caller.equipment.equipmentSlots.FindAll(x => x.CanAttackUnarmed);
        unarmedSlots = unarmedSlots.FindAll(x => !x.active || (!x.equipped.held[0].type.HasFlag(ItemType.MELEE_WEAPON) && !x.equipped.held[0].type.HasFlag(ItemType.RANGED_WEAPON)));

        //Do we have any weapons equipped?
        if (slots.Count > 0 || unarmedSlots.Count > 0)
        {
            //Begin attack
            foreach (EquipmentSlot s in slots)
            {
                if (!primaryWeapons.Contains(s.equipped.held[0].melee) && !secondaryWeapons.Contains(s.equipped.held[0].melee))
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

            caller.connections.OnGenerateUnarmedAttacks.Invoke(ref action, ref unarmedSlots);

            foreach (EquipmentSlot slot in unarmedSlots)
            {
                UnarmedAttack(caller, target, slot);
            }

            caller.energy -= 100;

            
        }
        else
        {
            Debug.Log("Your can't melee attack with a ranged weapon!");
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

    public void UnarmedAttack(Monster attacker, Monster defender, EquipmentSlot slot)
    {
        AttackAction action = this;
        attacker.connections.OnBeginUnarmedAttack.Invoke(ref slot, ref action);

        AttackResult result = Combat.DetermineHit(defender, slot.unarmedAttack);

        defender.connections.OnBeforeUnarmedAttackTarget.Invoke(ref slot, ref action, ref result);

        attacker.connections.OnUnarmedAttackResult.Invoke(ref slot, ref action, ref result);

        if (result == AttackResult.HIT)
        {
            Combat.Hit(attacker, defender, DamageSource.UNARMEDATTACK, slot.unarmedAttack);
        }
        else
        {
            Debug.Log($"Console: The {attacker.GetLocalizedName()} misses!");
        }

        defender.connections.OnAfterUnarmedAttackTarget.Invoke(ref slot, ref action, ref result);

        attacker.connections.OnEndUnarmedAttack.Invoke(ref slot, ref action, ref result);


    }



    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}