using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connections
{
    public Monster monster;
    public Item item;
    public Ability ability;

    public Connections()
    {

    }

    public Connections(Monster monster)
    {
        this.monster = monster;
    }

    public Connections(Item item)
    {
        this.item = item;
    }

    public Connections(Ability ability)
    {
        this.ability = ability;
    }


    //BEGIN AUTO EVENTS
    public OrderedEvent OnTurnStartGlobal = new OrderedEvent();
    public OrderedEvent OnTurnEndGlobal = new OrderedEvent();
    public OrderedEvent OnTurnStartLocal = new OrderedEvent();
    public OrderedEvent OnTurnEndLocal = new OrderedEvent();
    public OrderedEvent<Vector2Int, bool> OnMoveInitiated = new OrderedEvent<Vector2Int, bool>();
    public OrderedEvent OnMove = new OrderedEvent();
    public OrderedEvent OnFullyHealed = new OrderedEvent();
    public OrderedEvent OnPostDeath = new OrderedEvent();
    public OrderedEvent OnDeath = new OrderedEvent();
    public OrderedEvent<Monster, DamageType, DamageSource> OnKillMonster = new OrderedEvent<Monster, DamageType, DamageSource>();
    public OrderedEvent<Stats> RegenerateStats = new OrderedEvent<Stats>();
    public OrderedEvent<int> OnEnergyGained = new OrderedEvent<int>();
    public OrderedEvent<int, int> OnAttacked = new OrderedEvent<int, int>();
    public OrderedEvent<float, DamageType, DamageSource> OnDealDamage = new OrderedEvent<float, DamageType, DamageSource>();
    public OrderedEvent<float, DamageType, DamageSource> OnTakeDamage = new OrderedEvent<float, DamageType, DamageSource>();
    public OrderedEvent<float> OnHealing = new OrderedEvent<float>();
    public OrderedEvent<Effect[]> OnApplyStatusEffects = new OrderedEvent<Effect[]>();
    public OrderedEvent<Item, bool> OnActivateItem = new OrderedEvent<Item, bool>();
    public OrderedEvent<AbilityAction, bool> OnCastAbility = new OrderedEvent<AbilityAction, bool>();
    public OrderedEvent<Stats> OnGainResources = new OrderedEvent<Stats>();
    public OrderedEvent<float> OnGainXP = new OrderedEvent<float>();
    public OrderedEvent<int> OnLevelUp = new OrderedEvent<int>();
    public OrderedEvent<Stats> OnLoseResources = new OrderedEvent<Stats>();
    public OrderedEvent<Monster, AbilityStats, Ability> OnRegenerateAbilityStats = new OrderedEvent<Monster, AbilityStats, Ability>();
    public OrderedEvent<Ability, bool> OnCheckAvailability = new OrderedEvent<Ability, bool>();
    public OrderedEvent<Targeting, Ability> OnTargetsSelected = new OrderedEvent<Targeting, Ability>();
    public OrderedEvent<Ability> OnPreCast = new OrderedEvent<Ability>();
    public OrderedEvent<Ability> OnPostCast = new OrderedEvent<Ability>();
    public OrderedEvent<AbilityAction> OnTargetedByAbility = new OrderedEvent<AbilityAction>();
    public OrderedEvent<AbilityAction> OnHitByAbility = new OrderedEvent<AbilityAction>();
    public OrderedEvent<AttackAction, bool> OnStartAttack = new OrderedEvent<AttackAction, bool>();
    public OrderedEvent<AttackAction, bool> OnStartAttackTarget = new OrderedEvent<AttackAction, bool>();
    public OrderedEvent<AttackAction, List<Weapon>, List<Weapon>> OnGenerateArmedAttacks = new OrderedEvent<AttackAction, List<Weapon>, List<Weapon>>();
    public OrderedEvent<Weapon, AttackAction> OnBeginPrimaryAttack = new OrderedEvent<Weapon, AttackAction>();
    public OrderedEvent<Weapon, AttackAction, AttackResult> OnPrimaryAttackResult = new OrderedEvent<Weapon, AttackAction, AttackResult>();
    public OrderedEvent<Weapon, AttackAction, AttackResult> OnEndPrimaryAttack = new OrderedEvent<Weapon, AttackAction, AttackResult>();
    public OrderedEvent<Weapon, AttackAction> OnBeginSecondaryAttack = new OrderedEvent<Weapon, AttackAction>();
    public OrderedEvent<Weapon, AttackAction, AttackResult> OnSecondaryAttackResult = new OrderedEvent<Weapon, AttackAction, AttackResult>();
    public OrderedEvent<Weapon, AttackAction, AttackResult> OnEndSecondaryAttack = new OrderedEvent<Weapon, AttackAction, AttackResult>();
    public OrderedEvent<AttackAction, List<EquipmentSlot>> OnGenerateUnarmedAttacks = new OrderedEvent<AttackAction, List<EquipmentSlot>>();
    public OrderedEvent<EquipmentSlot, AttackAction> OnBeginUnarmedAttack = new OrderedEvent<EquipmentSlot, AttackAction>();
    public OrderedEvent<EquipmentSlot, AttackAction, AttackResult> OnUnarmedAttackResult = new OrderedEvent<EquipmentSlot, AttackAction, AttackResult>();
    public OrderedEvent<EquipmentSlot, AttackAction, AttackResult> OnEndUnarmedAttack = new OrderedEvent<EquipmentSlot, AttackAction, AttackResult>();
    public OrderedEvent<Weapon, AttackAction, AttackResult> OnBeforePrimaryAttackTarget = new OrderedEvent<Weapon, AttackAction, AttackResult>();
    public OrderedEvent<Weapon, AttackAction, AttackResult> OnAfterPrimaryAttackTarget = new OrderedEvent<Weapon, AttackAction, AttackResult>();
    public OrderedEvent<Weapon, AttackAction, AttackResult> OnBeforeSecondaryAttackTarget = new OrderedEvent<Weapon, AttackAction, AttackResult>();
    public OrderedEvent<Weapon, AttackAction, AttackResult> OnAfterSecondaryAttackTarget = new OrderedEvent<Weapon, AttackAction, AttackResult>();
    public OrderedEvent<EquipmentSlot, AttackAction, AttackResult> OnBeforeUnarmedAttackTarget = new OrderedEvent<EquipmentSlot, AttackAction, AttackResult>();
    public OrderedEvent<EquipmentSlot, AttackAction, AttackResult> OnAfterUnarmedAttackTarget = new OrderedEvent<EquipmentSlot, AttackAction, AttackResult>();
    public OrderedEvent<LOSData> OnGenerateLOSPreCollection = new OrderedEvent<LOSData>();
    public OrderedEvent<LOSData> OnGenerateLOSPostCollection = new OrderedEvent<LOSData>();

}
