using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[EffectGroup("Sample Group/Sample Subgroup")]
[Priority(10)]
[Group("Class Effects/Warrior")]
public class CowardStance : Effect
{
    int numMonstersInSight = 0;
    bool addsExtraDamage = false;
    bool isAfraid = false;

    public int refundEnergyOnScared;
    public float percentDamageIncrease;
    public float percentDamageDecreaseFear;
    
    /* The default priority of all functions in this class - the order in which they'll be called
     * relative to other status effects
     * 
     * To override for individual functions, use the [Priority(int)] attribute 
     */
    //public override int priority { get { return 10; } }

    //Constuctor for the object; use this in code if you're not using the asset version!
    //Generally nice to include, just for future feature proofing
    public CowardStance()
    {
        //Construct me!
    }

    //Called the moment an effect connects to a monster
    //Use this to apply effects or stats immediately, before the next frame
    /*public override void OnConnection() {}*/

    //Called when an effect gets disconnected from a monster
    /*public override void OnDisconnection() {} */

    //Called at the start of the global turn sequence
    //public override void OnTurnStartGlobal() {}

    //Called at the end of the global turn sequence
    //public override void OnTurnEndGlobal() {}

    //Called at the start of a monster's turn
    public override void OnTurnStartLocal()
    {
        bool hasSpeed = (numMonstersInSight >= 2);
        numMonstersInSight = 0;
        foreach (Monster m in connectedTo.monster.view.visibleMonsters)
        {
            if (m.IsEnemy(connectedTo.monster))
            {
                numMonstersInSight++;
            }
        }

        if (!hasSpeed && (numMonstersInSight >= 2))
        {
            Debug.Log("Console: You feel afraid - run away!!");
        }
        
        if (hasSpeed && (numMonstersInSight < 2))
        {
            Debug.Log("You slow down.");
        }
    }

    //Called at the end of a monster's turn
    //public override void OnTurnEndLocal() {}

    //Called whenever a monster takes a step
    public override void OnMove()
    {
        if (numMonstersInSight >= 2)
        {
            connectedTo.monster.AddEnergy(refundEnergyOnScared);
        }
    }

    //Called whenever a monster returns to full health
    //public override void OnFullyHealed() {}

    //Called when a monster is killed by this unit.
    //public override void OnKillMonster(ref Monster monster, ref DamageType type, ref DamageSource source) {}

    //Called when the connected monster dies
    //public override void OnDeath() {}

    //Called often, whenever a monster needs up-to-date stats.
    //public override void RegenerateStats(ref Stats stats) {}

    //Called wenever a monster gains energy
    //public override void OnEnergyGained(ref int energy) {}

    //Called when a monster gets attacked (REWORKING SOON!)
    //public override void OnAttacked(ref int pierce, ref int accuracy) {}

    //Called by the dealer of damage, when applicable. Modifications here happen before damage is dealt.
    public override void OnDealDamage(ref float damage, ref DamageType damageType, ref DamageSource source)
    {
        if (addsExtraDamage && source == DamageSource.MELEEATTACK)
        {
            Debug.Log("Adding extra damage!");
            damage += (damage * percentDamageIncrease / 100f);
        }
        else if (isAfraid && source == DamageSource.MELEEATTACK)
        {
            damage -= (damage * percentDamageDecreaseFear / 100f);
        }
    }

    //Called when a monster takes damage from any source, good for making effects fire upon certain types of damage
    //public override void OnTakeDamage(ref float damage, ref DamageType damageType, ref DamageSource source) {}

    //Called when a monster recieves a healing event request
    //public override void OnHealing(ref float healAmount) {}

    //Called when new status effects are added. All status effects coming through are bunched together as a list.
    //public override void OnApplyStatusEffects(ref Effect[] effects) {}

    //Called when a spell is cast. Modify spell, or set continue to false in order to cancel the action!
    //public override void OnCastAbility(ref AbilityAction action, ref bool canContinue) {}

    //Called when this monster gains resources. (Different from healing, but can give health)
    //public override void OnGainResources(ref Stats resources) {}

    //Called when this monster gains XP from any source.
    //public override void OnGainXP(ref float XPAmount) {}

    //Called when this monster levels up! Level CANNOT be modified.
    //public override void OnLevelUp(ref int Level) {}

    //Called when this monster loses resources. (Different from damage, but can take health)
    //public override void OnLoseResources(ref Stats resources) {}

    //Called when new status effects are added. All status effects coming through are bunched together as a list.
    //public override void OnRegenerateAbilityStats(ref Targeting targeting, ref AbilityBlock abilityBlock, ref Ability ability) {}

    //Called by spells, in order to determine whether they are allowed to be cast.
    //public override void OnCheckAvailability(ref Ability abilityToCheck, ref bool available) {}

    //Called by spells once targets are selected.
    //public override void OnTargetsSelected(ref Targeting targeting, ref Ability ability) {}

    //Called before spell is cast
    //public override void OnPreCast(ref Ability ability) {}

    //Called after a spell is cast.
    //public override void OnPostCast(ref Ability ability) {}

    //Called when this monster is selected to be hit by a cast. (Right before hit)
    //public override void OnTargetedByAbility(ref AbilityAction action) {}

    //Called after an ability is cast on this monster. (Right after hit)
    //public override void OnHitByAbility(ref AbilityAction action) {}

    //Called when this monster starts an attack action
    //public override void OnStartAttack(ref AttackAction action, ref bool canContinue) {}

    //Called when an attack has collected the weapons that it will use.
    //public override void OnGenerateArmedAttacks(ref List<Weapon> primaryWeapons, ref List<Weapon> secondaryWeapons) {}

    //Called before a primary attack happens
    //public override void OnBeginPrimaryAttack(ref Weapon weapon, ref AttackAction action) {}

    //Called once a primary attack has generated a result. (Before result is used)
    public override void OnPrimaryAttackResult(ref Weapon weapon, ref AttackAction action, ref AttackResult result)
    {
        addsExtraDamage = false;
        isAfraid = false;
        if (result == AttackResult.HIT)
        {
            int numMonsters = 0;
            Monster lastMonster = null;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Monster temp = Map.current.GetTile(action.caller.location + new Vector2Int(i, j)).currentlyStanding;
                    if (temp && temp.IsEnemy(action.caller))
                    {
                        numMonsters++;
                        lastMonster = temp;
                    }
                }
            }

            if (numMonsters == 1 && lastMonster == action.target)
            {
                addsExtraDamage = true;
            }
            else if (numMonsters > 1)
            {
                isAfraid = true;
            }
        }
    }

    //Called after an attack has completely finished - results are final
    public override void OnEndPrimaryAttack(ref Weapon weapon, ref AttackAction action, ref AttackResult result)
    {
        addsExtraDamage = false;
    }

    //Called before a secondary attack happens
    //public override void OnBeginSecondaryAttack(ref Weapon weapon, ref AttackAction action) {}

    //Called once a primary attack has generated a result. (Before result is used)
    //public override void OnSecondaryAttackResult(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called after a seconary attack has completely finished - results are final
    //public override void OnEndSecondaryAttack(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called when an attack has collected the unarmed slots that it will use.
    //public override void OnGenerateUnarmedAttacks(ref List<EquipmentSlot> slots) {}

    //Called before an unarmed attack begins.
    //public override void OnBeginUnarmedAttack(ref EquipmentSlot slot, ref AttackAction action) {}

    //Called when an unarmed attack has a determined a result, before that result is used.
    //public override void OnUnarmedAttackResult(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}

    //Called when an unarmed attack has a determined a result, after that result is used.
    //public override void OnEndUnarmedAttack(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}

    //Called before this monster is hit by a primary attack from another monster.
    //public override void OnBeforePrimaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called after this monster is hit by a primary attack from another monster. (Can't modify anymore)
    //public override void OnAfterPrimaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called before this monster is hit by a secondary attack from another monster.
    //public override void OnBeforeSecondaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called after this monster is hit by a secondary attack from another monster. (Can't modify anymore)
    //public override void OnAfterSecondaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called before this monster is hit by an unarmed attack from another monster.
    //public override void OnBeforeUnarmedAttackTarget(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}

    //Called after this monster is hit by an unarmed attack from another monster. (Can't modify anymore)
    //public override void OnAfterUnarmedAttackTarget(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}


    //BEGIN CONNECTION
    public override void Connect(Connections c)
    {
        connectedTo = c;

        c.OnTurnStartLocal.AddListener(10, OnTurnStartLocal);

        c.OnMove.AddListener(10, OnMove);

        c.OnDealDamage.AddListener(10, OnDealDamage);

        c.OnPrimaryAttackResult.AddListener(10, OnPrimaryAttackResult);

        c.OnEndPrimaryAttack.AddListener(10, OnEndPrimaryAttack);

        OnConnection();
    }
    //END CONNECTION

    //BEGIN DISCONNECTION
    public override void Disconnect()
    {
        OnDisconnection();

        connectedTo.OnTurnStartLocal.RemoveListener(OnTurnStartLocal);

        connectedTo.OnMove.RemoveListener(OnMove);

        connectedTo.OnDealDamage.RemoveListener(OnDealDamage);

        connectedTo.OnPrimaryAttackResult.RemoveListener(OnPrimaryAttackResult);

        connectedTo.OnEndPrimaryAttack.RemoveListener(OnEndPrimaryAttack);

        ReadyToDelete = true;
    }
    //END DISCONNECTION
}
