using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("P_Attributes")]
[Priority(10)]
public class GreySkin : Effect
{
    public float percentOfLastDamage;
    public float overXTurns;

    float healthPerTurn;
    float turnsRemaining = 0;
    /* The default priority of all functions in this class - the order in which they'll be called
     * relative to other status effects
     * 
     * To override for individual functions, use the [Priority(int)] attribute 
     */
    //public override int priority { get { return 10; } }

    //Constuctor for the object; use this in code if you're not using the asset version!
    //Generally nice to include, just for future feature proofing
    public GreySkin()
    {
        //Construct me!
    }

    //Called the moment an effect connects to a monster
    //Use this to apply effects or stats immediately, before the next frame
    /*public override void OnConnection() {}*/

    //Called when an effect gets disconnected from a monster
    /*public override void OnDisconnection() {} */

    //Called when an effect "Clashes" with an effect of the same type
    /* public override void OnStack(Effect other, ref bool addThisEffect) {} */

    //Called at the start of the global turn sequence
    //public override void OnTurnStartGlobal() {}

    //Called at the end of the global turn sequence
    public override void OnTurnEndGlobal()
    {
        if (turnsRemaining > 0)
        {
            turnsRemaining--;
            connectedTo.monster.Heal(healthPerTurn, false);
        }
    }

    //Called at the start of a monster's turn
    //public override void OnTurnStartLocal() {}

    //Called at the end of a monster's turn
    //public override void OnTurnEndLocal() {}

    //Called whenever a monster wants to take a step.
    //public override void OnMoveInitiated(ref Vector2Int newLocation, ref bool canMove) {}

    //Called whenever a monster sucessfully takes a step.
    //public override void OnMove() {}

    //Called whenever a monster returns to full health
    //public override void OnFullyHealed() {}

    //Called when the connected monster dies
    //public override void OnDeath() {}

    //Called when a monster is killed by this unit.
    //public override void OnKillMonster(ref Monster monster, ref DamageType type, ref DamageSource source) {}

    //Called often, whenever a monster needs up-to-date stats.
    //public override void RegenerateStats(ref Stats stats) {}

    //Called wenever a monster gains energy
    //public override void OnEnergyGained(ref int energy) {}

    //Called when a monster gets attacked (REWORKING SOON!)
    //public override void OnAttacked(ref int pierce, ref int accuracy) {}

    //Called by the dealer of damage, when applicable. Modifications here happen before damage is dealt.
    //public override void OnDealDamage(ref float damage, ref DamageType damageType, ref DamageSource source) {}

    //Called when a monster takes damage from any source, good for making effects fire upon certain types of damage
    public override void OnTakeDamage(ref float damage, ref DamageType damageType, ref DamageSource source)
    {
        turnsRemaining = overXTurns;
        healthPerTurn = damage * (percentOfLastDamage / 100) / overXTurns;
    }

    //Called when a monster recieves a healing event request
    //public override void OnHealing(ref float healAmount) {}

    //Called when new status effects are added. All status effects coming through are bunched together as a list.
    //public override void OnApplyStatusEffects(ref Effect[] effects) {}

    //Called when this monster attempts to activate an item.
    //public override void OnActivateItem(ref Item item, ref bool canContinue) {}

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
    //public override void OnRegenerateAbilityStats(ref Monster caster, ref AbilityStats abilityStats, ref Ability ability) {}

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
    //public override void OnPrimaryAttackResult(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called after an attack has completely finished - results are final
    //public override void OnEndPrimaryAttack(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

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

    //Called after this monster generates LOS, but before visible entity collection.
    //public override void OnGenerateLOSPreCollection(ref LOSData view) {}

    //Called after this monster generates LOS and visible entities.
    //public override void OnGenerateLOSPostCollection(ref LOSData view) {}


    //BEGIN CONNECTION
    //TEMPORARY CONNECTION - THIS WILL BE AUTO-REMOVED ONCE EFFECTS ARE REBUILT.
    public override void Connect(Connections c)
    {
        connectedTo = c;
        connectedTo.OnTurnStartGlobal.AddListener(100, OnTurnStartGlobal);
        connectedTo.OnTurnEndGlobal.AddListener(100, OnTurnEndGlobal);
        connectedTo.OnTurnStartLocal.AddListener(100, OnTurnStartLocal);
        connectedTo.OnTurnEndLocal.AddListener(100, OnTurnEndLocal);
        connectedTo.OnMoveInitiated.AddListener(100, OnMoveInitiated);
        connectedTo.OnMove.AddListener(100, OnMove);
        connectedTo.OnFullyHealed.AddListener(100, OnFullyHealed);
        connectedTo.OnDeath.AddListener(100, OnDeath);
        connectedTo.OnKillMonster.AddListener(100, OnKillMonster);
        connectedTo.RegenerateStats.AddListener(100, RegenerateStats);
        connectedTo.OnEnergyGained.AddListener(100, OnEnergyGained);
        connectedTo.OnAttacked.AddListener(100, OnAttacked);
        connectedTo.OnDealDamage.AddListener(100, OnDealDamage);
        connectedTo.OnTakeDamage.AddListener(100, OnTakeDamage);
        connectedTo.OnHealing.AddListener(100, OnHealing);
        connectedTo.OnApplyStatusEffects.AddListener(100, OnApplyStatusEffects);
        connectedTo.OnActivateItem.AddListener(100, OnActivateItem);
        connectedTo.OnCastAbility.AddListener(100, OnCastAbility);
        connectedTo.OnGainResources.AddListener(100, OnGainResources);
        connectedTo.OnGainXP.AddListener(100, OnGainXP);
        connectedTo.OnLevelUp.AddListener(100, OnLevelUp);
        connectedTo.OnLoseResources.AddListener(100, OnLoseResources);
        connectedTo.OnRegenerateAbilityStats.AddListener(100, OnRegenerateAbilityStats);
        connectedTo.OnCheckAvailability.AddListener(100, OnCheckAvailability);
        connectedTo.OnTargetsSelected.AddListener(100, OnTargetsSelected);
        connectedTo.OnPreCast.AddListener(100, OnPreCast);
        connectedTo.OnPostCast.AddListener(100, OnPostCast);
        connectedTo.OnTargetedByAbility.AddListener(100, OnTargetedByAbility);
        connectedTo.OnHitByAbility.AddListener(100, OnHitByAbility);
        connectedTo.OnStartAttack.AddListener(100, OnStartAttack);
        connectedTo.OnGenerateArmedAttacks.AddListener(100, OnGenerateArmedAttacks);
        connectedTo.OnBeginPrimaryAttack.AddListener(100, OnBeginPrimaryAttack);
        connectedTo.OnPrimaryAttackResult.AddListener(100, OnPrimaryAttackResult);
        connectedTo.OnEndPrimaryAttack.AddListener(100, OnEndPrimaryAttack);
        connectedTo.OnBeginSecondaryAttack.AddListener(100, OnBeginSecondaryAttack);
        connectedTo.OnSecondaryAttackResult.AddListener(100, OnSecondaryAttackResult);
        connectedTo.OnEndSecondaryAttack.AddListener(100, OnEndSecondaryAttack);
        connectedTo.OnGenerateUnarmedAttacks.AddListener(100, OnGenerateUnarmedAttacks);
        connectedTo.OnBeginUnarmedAttack.AddListener(100, OnBeginUnarmedAttack);
        connectedTo.OnUnarmedAttackResult.AddListener(100, OnUnarmedAttackResult);
        connectedTo.OnEndUnarmedAttack.AddListener(100, OnEndUnarmedAttack);
        connectedTo.OnBeforePrimaryAttackTarget.AddListener(100, OnBeforePrimaryAttackTarget);
        connectedTo.OnAfterPrimaryAttackTarget.AddListener(100, OnAfterPrimaryAttackTarget);
        connectedTo.OnBeforeSecondaryAttackTarget.AddListener(100, OnBeforeSecondaryAttackTarget);
        connectedTo.OnAfterSecondaryAttackTarget.AddListener(100, OnAfterSecondaryAttackTarget);
        connectedTo.OnBeforeUnarmedAttackTarget.AddListener(100, OnBeforeUnarmedAttackTarget);
        connectedTo.OnAfterUnarmedAttackTarget.AddListener(100, OnAfterUnarmedAttackTarget);
        connectedTo.OnGenerateLOSPreCollection.AddListener(100, OnGenerateLOSPreCollection);
        connectedTo.OnGenerateLOSPostCollection.AddListener(100, OnGenerateLOSPostCollection);
        OnConnection();
    }
    //END CONNECTION

    //BEGIN DISCONNECTION
    //TEMPORARY DISCONNECTION - THIS WILL BE AUTO-REMOVED ONCE EFFECTS ARE REBUILT.
    public override void Disconnect()
    {
        OnDisconnection();
        connectedTo.OnTurnStartGlobal.RemoveListener(OnTurnStartGlobal);
        connectedTo.OnTurnEndGlobal.RemoveListener(OnTurnEndGlobal);
        connectedTo.OnTurnStartLocal.RemoveListener(OnTurnStartLocal);
        connectedTo.OnTurnEndLocal.RemoveListener(OnTurnEndLocal);
        connectedTo.OnMoveInitiated.RemoveListener(OnMoveInitiated);
        connectedTo.OnMove.RemoveListener(OnMove);
        connectedTo.OnFullyHealed.RemoveListener(OnFullyHealed);
        connectedTo.OnDeath.RemoveListener(OnDeath);
        connectedTo.OnKillMonster.RemoveListener(OnKillMonster);
        connectedTo.RegenerateStats.RemoveListener(RegenerateStats);
        connectedTo.OnEnergyGained.RemoveListener(OnEnergyGained);
        connectedTo.OnAttacked.RemoveListener(OnAttacked);
        connectedTo.OnDealDamage.RemoveListener(OnDealDamage);
        connectedTo.OnTakeDamage.RemoveListener(OnTakeDamage);
        connectedTo.OnHealing.RemoveListener(OnHealing);
        connectedTo.OnApplyStatusEffects.RemoveListener(OnApplyStatusEffects);
        connectedTo.OnActivateItem.RemoveListener(OnActivateItem);
        connectedTo.OnCastAbility.RemoveListener(OnCastAbility);
        connectedTo.OnGainResources.RemoveListener(OnGainResources);
        connectedTo.OnGainXP.RemoveListener(OnGainXP);
        connectedTo.OnLevelUp.RemoveListener(OnLevelUp);
        connectedTo.OnLoseResources.RemoveListener(OnLoseResources);
        connectedTo.OnRegenerateAbilityStats.RemoveListener(OnRegenerateAbilityStats);
        connectedTo.OnCheckAvailability.RemoveListener(OnCheckAvailability);
        connectedTo.OnTargetsSelected.RemoveListener(OnTargetsSelected);
        connectedTo.OnPreCast.RemoveListener(OnPreCast);
        connectedTo.OnPostCast.RemoveListener(OnPostCast);
        connectedTo.OnTargetedByAbility.RemoveListener(OnTargetedByAbility);
        connectedTo.OnHitByAbility.RemoveListener(OnHitByAbility);
        connectedTo.OnStartAttack.RemoveListener(OnStartAttack);
        connectedTo.OnGenerateArmedAttacks.RemoveListener(OnGenerateArmedAttacks);
        connectedTo.OnBeginPrimaryAttack.RemoveListener(OnBeginPrimaryAttack);
        connectedTo.OnPrimaryAttackResult.RemoveListener(OnPrimaryAttackResult);
        connectedTo.OnEndPrimaryAttack.RemoveListener(OnEndPrimaryAttack);
        connectedTo.OnBeginSecondaryAttack.RemoveListener(OnBeginSecondaryAttack);
        connectedTo.OnSecondaryAttackResult.RemoveListener(OnSecondaryAttackResult);
        connectedTo.OnEndSecondaryAttack.RemoveListener(OnEndSecondaryAttack);
        connectedTo.OnGenerateUnarmedAttacks.RemoveListener(OnGenerateUnarmedAttacks);
        connectedTo.OnBeginUnarmedAttack.RemoveListener(OnBeginUnarmedAttack);
        connectedTo.OnUnarmedAttackResult.RemoveListener(OnUnarmedAttackResult);
        connectedTo.OnEndUnarmedAttack.RemoveListener(OnEndUnarmedAttack);
        connectedTo.OnBeforePrimaryAttackTarget.RemoveListener(OnBeforePrimaryAttackTarget);
        connectedTo.OnAfterPrimaryAttackTarget.RemoveListener(OnAfterPrimaryAttackTarget);
        connectedTo.OnBeforeSecondaryAttackTarget.RemoveListener(OnBeforeSecondaryAttackTarget);
        connectedTo.OnAfterSecondaryAttackTarget.RemoveListener(OnAfterSecondaryAttackTarget);
        connectedTo.OnBeforeUnarmedAttackTarget.RemoveListener(OnBeforeUnarmedAttackTarget);
        connectedTo.OnAfterUnarmedAttackTarget.RemoveListener(OnAfterUnarmedAttackTarget);
        connectedTo.OnGenerateLOSPreCollection.RemoveListener(OnGenerateLOSPreCollection);
        connectedTo.OnGenerateLOSPostCollection.RemoveListener(OnGenerateLOSPostCollection);
        ReadyToDelete = true;
    }
    //END DISCONNECTION
}
