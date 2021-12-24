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
    public OrderedEvent OnDeath = new OrderedEvent();
    public OrderedEvent<StatBlock> RegenerateStats = new OrderedEvent<StatBlock>();
    public OrderedEvent<int> OnEnergyGained = new OrderedEvent<int>();
    public OrderedEvent<int, int> OnAttacked = new OrderedEvent<int, int>();
    public OrderedEvent<int, DamageType> OnTakeDamage = new OrderedEvent<int, DamageType>();
    public OrderedEvent<int> OnHealing = new OrderedEvent<int>();
    public OrderedEvent<Effect[]> OnApplyStatusEffects = new OrderedEvent<Effect[]>();

}
