using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttackAction : AttackAction
{

    //Constuctor for the action; must include caller!
    public RangedAttackAction()
    {
        //Construct me! Assigns caller by default in the base class
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        //See if we have any weapons actively equipped
        List<EquipmentSlot> slots = caller.equipment.equipmentSlots.FindAll(x => x.active && x.equipped.held[0].type == ItemType.RANGED_WEAPON);

        //Do we have any weapons equipped?
        if (slots.Count > 0)
        {
            //Begin attack
            foreach (EquipmentSlot s in slots)
            {
                if (!primaryWeapons.Contains(s.equipped.held[0].ranged) && !secondaryWeapons.Contains(s.equipped.held[0].ranged))
                {
                    if (s.type.Contains(EquipSlotType.PRIMARY_HAND))
                    {
                        primaryWeapons.Add(s.equipped.held[0].ranged);
                    }
                    else
                    {
                        secondaryWeapons.Add(s.equipped.held[0].ranged);
                    }
                }
            }

            AttackAction action = this;

            caller.connections.OnGenerateArmedAttacks.Invoke(ref action, ref primaryWeapons, ref secondaryWeapons);
            int numShots = 0;
            bool canFire = false; //Assume we CANNOT fire by default.

            foreach (Weapon w in primaryWeapons)
            {
                RangedWeapon weapon = (RangedWeapon)w;
                canFire = false;
                IEnumerator targeting = caller.controller.DetermineTarget(weapon.targeting, (b) => canFire = b);

                while (targeting.MoveNext())
                {
                    yield return targeting.Current;
                }

                if (canFire)
                {
                    numShots++;
                    foreach (Monster m in weapon.targeting.affected)
                    {
                        target = m;
                        w.PrimaryAttack(caller, m, this);
                    }
                }
                else
                {
                    if (numShots == 0)
                    {
                        break;
                    }
                }
            }

            //Cancel early - we failed to fire a primary weapon
            if (!canFire && numShots == 0 && primaryWeapons.Count > 0)
            {
                yield break;
            }

            foreach (Weapon w in secondaryWeapons)
            {
                RangedWeapon weapon = (RangedWeapon)w;

                canFire = false;
                IEnumerator targeting = caller.controller.DetermineTarget(weapon.targeting, (b) => canFire = b);
                while (targeting.MoveNext())
                {
                    yield return targeting.Current;
                }

                if (canFire)
                {
                    numShots++;
                    foreach (Monster m in weapon.targeting.affected)
                    {
                        target = m;
                        w.SecondaryAttack(caller, m, this);
                    }
                }
                else
                {
                    if (numShots == 0)
                    {
                        break;
                    }
                }
            }

            if (!canFire && numShots == 0)
            {
                yield break;
            }

            caller.energy -= 100;
        }
        else
        {
            Debug.Log("Console Log: Can't fire with no ranged weapon equipped!");
            yield break;
        }
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}