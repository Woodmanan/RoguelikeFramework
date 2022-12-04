using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.Localization;

/*
 * Mostly empty class used as a base for status effects. If you want to create a new
 * status effect, DO NOT EDIT THIS CLASS. Instead, use the template and fill in your
 * child class from there. This class mostly exists to make that process easy, and have
 * process of hooking up complicated effects be really painless.
 *
 * I have suffered so you don't have to ;_;
 */

[System.Serializable]
public class Effect
{
    [HideInInspector] public Connections connectedTo;
    [HideInInspector] public bool ReadyToDelete = false;
    [HideInInspector] public Monster credit;

    [SerializeField] protected LocalizedString name;
    [SerializeField] protected LocalizedString description;

    public Effect Instantiate()
    {
        return (Effect) this.MemberwiseClone();
    }

    public virtual string GetName()
    {
        return name.GetLocalizedString(this);
    }

    public virtual string GetDescription()
    {
        return description.GetLocalizedString(this);
    }

    /* Connect:
     * The method that links this effect to a given monster, and hooks up its event calls.
     *
     * It's an absolute monster of a method. This is horrible and innefficient, BUT,
     * it takes roughly .01 ms to run and there's no way we need 1000 of these per
     * frame. The tradeoff for doing it this way is that new implemented effects only
     * need to override the given methods; once they do that, this function will
     * automatically connect the function to the given event, and we're good to go.
     *
     * Benchmark is ~1000 calls per second still runs at 60 FPS. This gets the greenlight.
     *
     * Adding new events to this stack is a little jank. If you think there needs to
     * be a new connection, let me (Woody) know and we can get it added!
     */
    public virtual void Connect(Connections c)
    {
        Debug.LogError("You should override connection on this object! It should NEVER be called unmodified.");

        connectedTo = c;

        //BEGIN AUTO CONNECT

        c.OnTurnStartGlobal.AddListener(100, OnTurnStartGlobal);
        c.OnTurnEndGlobal.AddListener(100, OnTurnEndGlobal);
        c.OnTurnStartLocal.AddListener(100, OnTurnStartLocal);
        c.OnTurnEndLocal.AddListener(100, OnTurnEndLocal);
        c.OnMoveInitiated.AddListener(100, OnMoveInitiated);
        c.OnMove.AddListener(100, OnMove);
        c.OnFullyHealed.AddListener(100, OnFullyHealed);
        c.OnDeath.AddListener(100, OnDeath);
        c.OnKillMonster.AddListener(100, OnKillMonster);
        c.RegenerateStats.AddListener(100, RegenerateStats);
        c.OnEnergyGained.AddListener(100, OnEnergyGained);
        c.OnAttacked.AddListener(100, OnAttacked);
        c.OnDealDamage.AddListener(100, OnDealDamage);
        c.OnTakeDamage.AddListener(100, OnTakeDamage);
        c.OnHealing.AddListener(100, OnHealing);
        c.OnApplyStatusEffects.AddListener(100, OnApplyStatusEffects);
        c.OnActivateItem.AddListener(100, OnActivateItem);
        c.OnCastAbility.AddListener(100, OnCastAbility);
        c.OnGainResources.AddListener(100, OnGainResources);
        c.OnGainXP.AddListener(100, OnGainXP);
        c.OnLevelUp.AddListener(100, OnLevelUp);
        c.OnLoseResources.AddListener(100, OnLoseResources);
        c.OnRegenerateAbilityStats.AddListener(100, OnRegenerateAbilityStats);
        c.OnCheckAvailability.AddListener(100, OnCheckAvailability);
        c.OnTargetsSelected.AddListener(100, OnTargetsSelected);
        c.OnPreCast.AddListener(100, OnPreCast);
        c.OnPostCast.AddListener(100, OnPostCast);
        c.OnTargetedByAbility.AddListener(100, OnTargetedByAbility);
        c.OnHitByAbility.AddListener(100, OnHitByAbility);
        c.OnStartAttack.AddListener(100, OnStartAttack);
        c.OnGenerateArmedAttacks.AddListener(100, OnGenerateArmedAttacks);
        c.OnBeginPrimaryAttack.AddListener(100, OnBeginPrimaryAttack);
        c.OnPrimaryAttackResult.AddListener(100, OnPrimaryAttackResult);
        c.OnEndPrimaryAttack.AddListener(100, OnEndPrimaryAttack);
        c.OnBeginSecondaryAttack.AddListener(100, OnBeginSecondaryAttack);
        c.OnSecondaryAttackResult.AddListener(100, OnSecondaryAttackResult);
        c.OnEndSecondaryAttack.AddListener(100, OnEndSecondaryAttack);
        c.OnGenerateUnarmedAttacks.AddListener(100, OnGenerateUnarmedAttacks);
        c.OnBeginUnarmedAttack.AddListener(100, OnBeginUnarmedAttack);
        c.OnUnarmedAttackResult.AddListener(100, OnUnarmedAttackResult);
        c.OnEndUnarmedAttack.AddListener(100, OnEndUnarmedAttack);
        c.OnBeforePrimaryAttackTarget.AddListener(100, OnBeforePrimaryAttackTarget);
        c.OnAfterPrimaryAttackTarget.AddListener(100, OnAfterPrimaryAttackTarget);
        c.OnBeforeSecondaryAttackTarget.AddListener(100, OnBeforeSecondaryAttackTarget);
        c.OnAfterSecondaryAttackTarget.AddListener(100, OnAfterSecondaryAttackTarget);
        c.OnBeforeUnarmedAttackTarget.AddListener(100, OnBeforeUnarmedAttackTarget);
        c.OnAfterUnarmedAttackTarget.AddListener(100, OnAfterUnarmedAttackTarget);
        c.OnGenerateLOSPreCollection.AddListener(100, OnGenerateLOSPreCollection);
        c.OnGenerateLOSPostCollection.AddListener(100, OnGenerateLOSPostCollection);
        
        //END AUTO CONNECT

        OnConnection();
    }

    public virtual void Disconnect()
    {
        Debug.LogError("You should override disconnection on this object! It should NEVER be called unmodified.");

        OnDisconnection();

        //BEGIN AUTO DISCONNECT

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

        //END AUTO DISCONNECT

        ReadyToDelete = true;
        
    }

    public virtual void OnConnection() {}
    public virtual void OnDisconnection() {}
    public virtual void OnStack(Effect other, ref bool addThisEffect) {}

    //AUTO DECLARATIONS

    public virtual void OnTurnStartGlobal() {}
    public virtual void OnTurnEndGlobal() {}
    public virtual void OnTurnStartLocal() {}
    public virtual void OnTurnEndLocal() {}
    public virtual void OnMoveInitiated(ref Vector2Int newLocation, ref bool canMove) {}
    public virtual void OnMove() {}
    public virtual void OnFullyHealed() {}
    public virtual void OnDeath() {}
    public virtual void OnKillMonster(ref Monster monster, ref DamageType type, ref DamageSource source) {}
    public virtual void RegenerateStats(ref Stats stats) {}
    public virtual void OnEnergyGained(ref int energy) {}
    public virtual void OnAttacked(ref int pierce, ref int accuracy) {}
    public virtual void OnDealDamage(ref float damage, ref DamageType damageType, ref DamageSource source) {}
    public virtual void OnTakeDamage(ref float damage, ref DamageType damageType, ref DamageSource source) {}
    public virtual void OnHealing(ref float healAmount) {}
    public virtual void OnApplyStatusEffects(ref Effect[] effects) {}
    public virtual void OnActivateItem(ref Item item, ref bool canContinue) {}
    public virtual void OnCastAbility(ref AbilityAction action, ref bool canContinue) {}
    public virtual void OnGainResources(ref Stats resources) {}
    public virtual void OnGainXP(ref float XPAmount) {}
    public virtual void OnLevelUp(ref int Level) {}
    public virtual void OnLoseResources(ref Stats resources) {}
    public virtual void OnRegenerateAbilityStats(ref Monster caster, ref AbilityStats abilityStats, ref Ability ability) {}
    public virtual void OnCheckAvailability(ref Ability abilityToCheck, ref bool available) {}
    public virtual void OnTargetsSelected(ref Targeting targeting, ref Ability ability) {}
    public virtual void OnPreCast(ref Ability ability) {}
    public virtual void OnPostCast(ref Ability ability) {}
    public virtual void OnTargetedByAbility(ref AbilityAction action) {}
    public virtual void OnHitByAbility(ref AbilityAction action) {}
    public virtual void OnStartAttack(ref AttackAction action, ref bool canContinue) {}
    public virtual void OnGenerateArmedAttacks(ref List<Weapon> primaryWeapons, ref List<Weapon> secondaryWeapons) {}
    public virtual void OnBeginPrimaryAttack(ref Weapon weapon, ref AttackAction action) {}
    public virtual void OnPrimaryAttackResult(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnEndPrimaryAttack(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnBeginSecondaryAttack(ref Weapon weapon, ref AttackAction action) {}
    public virtual void OnSecondaryAttackResult(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnEndSecondaryAttack(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnGenerateUnarmedAttacks(ref List<EquipmentSlot> slots) {}
    public virtual void OnBeginUnarmedAttack(ref EquipmentSlot slot, ref AttackAction action) {}
    public virtual void OnUnarmedAttackResult(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnEndUnarmedAttack(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnBeforePrimaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnAfterPrimaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnBeforeSecondaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnAfterSecondaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnBeforeUnarmedAttackTarget(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnAfterUnarmedAttackTarget(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}
    public virtual void OnGenerateLOSPreCollection(ref LOSData view) {}
    public virtual void OnGenerateLOSPostCollection(ref LOSData view) {}

}
