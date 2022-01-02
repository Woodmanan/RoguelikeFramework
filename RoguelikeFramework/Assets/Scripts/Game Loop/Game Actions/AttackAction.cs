﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : GameAction
{
    public Monster target;
    List<MeleeWeapon> primaryWeapons = new List<MeleeWeapon>();
    List<MeleeWeapon> secondaryWeapons = new List<MeleeWeapon>();

    List<EquipmentSlot> unarmedSlots = new List<EquipmentSlot>();


    public AttackAction(Monster target)
    {
        //Construct me! Assigns caller by default in the base class
        this.target = target;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        //Correction: see if we have any weapons actively equipped (Doesn't need to be in your hands, theoretically
        List<EquipmentSlot> slots = caller.equipment.equipmentSlots.FindAll(x => x.active && x.equipped.held[0].type == ItemType.WEAPON);
        unarmedSlots = caller.equipment.equipmentSlots.FindAll(x => x.CanAttackUnarmed && !x.active);

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

            caller.connections.OnGenerateArmedAttacks.Invoke(ref primaryWeapons, ref secondaryWeapons);

            foreach (Weapon w in primaryWeapons)
            {
                w.PrimaryAttack(caller, target, this);
            }

            foreach (Weapon w in secondaryWeapons)
            {
                w.SecondaryAttack(caller, target, this);
            }

            caller.connections.OnGenerateUnarmedAttacks.Invoke(ref unarmedSlots);

            foreach (EquipmentSlot slot in unarmedSlots)
            {
                UnarmedAttack(caller, target, slot);
            }

            caller.energy -= 100;
        }
        else
        {
            Debug.Log("Console Log: You have nothing to attack with!");
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

        defender.connections.OnAfterUnarmedAttackTarget.Invoke(ref slot, ref action, ref result);

        attacker.connections.OnEndUnarmedAttack.Invoke(ref slot, ref action, ref result);


    }



    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}