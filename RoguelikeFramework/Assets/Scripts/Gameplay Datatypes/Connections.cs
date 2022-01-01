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
    public OrderedEvent OnMove = new OrderedEvent();
    public OrderedEvent OnFullyHealed = new OrderedEvent();
    public OrderedEvent<Monster, DamageType, DamageSource> OnKillMonster = new OrderedEvent<Monster, DamageType, DamageSource>();
    public OrderedEvent OnDeath = new OrderedEvent();
    public OrderedEvent<StatBlock> RegenerateStats = new OrderedEvent<StatBlock>();
    public OrderedEvent<int> OnEnergyGained = new OrderedEvent<int>();
    public OrderedEvent<int, int> OnAttacked = new OrderedEvent<int, int>();
    public OrderedEvent<int, DamageType, DamageSource> OnDealDamage = new OrderedEvent<int, DamageType, DamageSource>();
    public OrderedEvent<int, DamageType, DamageSource> OnTakeDamage = new OrderedEvent<int, DamageType, DamageSource>();
    public OrderedEvent<int> OnHealing = new OrderedEvent<int>();
    public OrderedEvent<Effect[]> OnApplyStatusEffects = new OrderedEvent<Effect[]>();
    public OrderedEvent<AbilityAction, bool> OnCastAbility = new OrderedEvent<AbilityAction, bool>();
    public OrderedEvent<ResourceList> OnGainResources = new OrderedEvent<ResourceList>();
    public OrderedEvent<ResourceList> OnLoseResources = new OrderedEvent<ResourceList>();
    public OrderedEvent<Targeting, AbilityBlock, Ability> OnRegenerateAbilityStats = new OrderedEvent<Targeting, AbilityBlock, Ability>();
    public OrderedEvent<Ability, bool> OnCheckAvailability = new OrderedEvent<Ability, bool>();
    public OrderedEvent<Targeting, Ability> OnTargetsSelected = new OrderedEvent<Targeting, Ability>();
    public OrderedEvent<Ability> OnPreCast = new OrderedEvent<Ability>();
    public OrderedEvent<Ability> OnPostCast = new OrderedEvent<Ability>();
    public OrderedEvent<AbilityAction> OnTargetedByAbility = new OrderedEvent<AbilityAction>();
    public OrderedEvent<AbilityAction> OnHitByAbility = new OrderedEvent<AbilityAction>();

}
