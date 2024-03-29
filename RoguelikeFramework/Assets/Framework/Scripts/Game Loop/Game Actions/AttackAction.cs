﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : GameAction
{
    public RogueHandle<Monster> target;
    public List<Weapon> primaryWeapons = new List<Weapon>();
    public List<Weapon> secondaryWeapons = new List<Weapon>();

    public bool animates = true;

    public List<EquipmentSlot> unarmedSlots = new List<EquipmentSlot>();

    public AttackAction()
    {

    }

    public AttackAction(RogueHandle<Monster> target)
    {
        //Construct me! Assigns caller by default in the base class
        this.target = target;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        AttackAction reference = this;
        bool canContinue = true;
        caller[0].connections.OnStartAttack.Invoke(ref reference, ref canContinue);
        target[0].connections.OnStartAttackTarget.Invoke(ref reference, ref canContinue);
        if (canContinue == false)
        {
            yield break;
        }

        //See if we have any weapons actively equipped, or unarmed slots that can attack
        if (caller[0].equipment == null)
        {
            Debug.LogError("Armless things can't attack! Wasting its turn");
            caller[0].energy -= 100;
            yield break;
        }
        List<EquipmentSlot> slots = caller[0].equipment.equipmentSlots.FindAll(x => x.active && x.equipped.held[0].type == ItemType.MELEE_WEAPON);
        unarmedSlots = caller[0].equipment.equipmentSlots.FindAll(x => x.CanAttackUnarmed);
        unarmedSlots = unarmedSlots.FindAll(x => !x.active || (!x.equipped.held[0].equipable.blocksUnarmed));

        string logString = LogFormatting.GetFormattedString("AttackFullString", new { attacker = caller[0].GetName(), singular = caller[0].singular, defender = target[0].GetName() });
        RogueLog.singleton.Log(logString, priority: LogPriority.COMBAT);

        //Do we have any weapons equipped?
        if (slots.Count > 0 || unarmedSlots.Count > 0)
        {
            //Begin tracking energy cost
            float energyCost = 0;

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

            caller[0].connections.OnGenerateArmedAttacks.Invoke(ref action, ref primaryWeapons, ref secondaryWeapons);

            if (animates)
            {
                AnimationController.AddAnimationForMonster(new AttackAnimation(caller, target), caller);
            }

            foreach (Weapon w in primaryWeapons)
            {
                w.PrimaryAttack(caller, target, this);
                energyCost = Mathf.Max(energyCost, w.primary.energyCost);
            }

            foreach (Weapon w in secondaryWeapons)
            {
                w.SecondaryAttack(caller, target, this);
                energyCost = Mathf.Max(energyCost, w.secondary.energyCost);
            }

            caller[0].connections.OnGenerateUnarmedAttacks.Invoke(ref action, ref unarmedSlots);

            foreach (EquipmentSlot slot in unarmedSlots)
            {
                UnarmedAttack(caller, target, slot);
                energyCost = Mathf.Max(energyCost, slot.unarmedAttack.energyCost);
            }

            caller[0].energy -= energyCost;

            
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
            caller[0].energy -= 100;

            yield break;
        }
    }

    public void UnarmedAttack(RogueHandle<Monster> attacker, RogueHandle<Monster> defender, EquipmentSlot slot)
    {
        Monster vAttacker = attacker;
        Monster vDefender = defender;
        if (vAttacker.IsDead() || vDefender.IsDead()) return;

        AttackAction action = this;
        vAttacker.connections.OnBeginUnarmedAttack.Invoke(ref slot, ref action);

        AttackResult result = Combat.DetermineHit(defender, slot.unarmedAttack);

        vDefender.connections.OnBeforeUnarmedAttackTarget.Invoke(ref slot, ref action, ref result);

        vAttacker.connections.OnUnarmedAttackResult.Invoke(ref slot, ref action, ref result);

        if (result == AttackResult.HIT)
        {
            RogueLog.singleton.Log($"{vAttacker.GetName()} hits {vDefender.GetName()} with an unarmed attack!", priority: LogPriority.COMBAT);
            Combat.Hit(attacker, defender, DamageSource.UNARMEDATTACK, slot.unarmedAttack);
        }
        else
        {
            RogueLog.singleton.Log($"The {vAttacker.GetLocalizedName()} misses an unarmed attack!", priority: LogPriority.COMBAT);
        }

        vDefender.connections.OnAfterUnarmedAttackTarget.Invoke(ref slot, ref action, ref result);

        vAttacker.connections.OnEndUnarmedAttack.Invoke(ref slot, ref action, ref result);
    }

    public override string GetDebugString()
    {
        return $"Attack Action: Hitting {target[0].friendlyName}";
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}