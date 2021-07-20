using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * Mostly empty class used as a base for status effects. If you want to create a new
 * status effect, DO NOT EDIT THIS CLASS. Instead, use the template and fill in your
 * child class from there. This class mostly exists to make that process easy, and have
 * process of hooking up complicated effects be really painless.
 *
 * I have suffered so you don't have to ;_;
 */

public class Effect : ScriptableObject
{
    [HideInInspector] public Monster target;
    [HideInInspector] public bool ReadyToDelete = false;

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
    public void Connect(Monster m)
    {
        target = m;
        Type current = this.GetType();
        Type defaultType = typeof(Effect);

        if (current.GetMethod("RegenerateStats").DeclaringType != defaultType)
        {
            m.RegenerateStats += RegenerateStats;
        }

        if (current.GetMethod("OnTurnStartGlobal").DeclaringType != defaultType)
        {
            m.OnTurnStartGlobal += OnTurnStartGlobal;
        }

        if (current.GetMethod("OnTurnEndGlobal").DeclaringType != defaultType)
        {
            m.OnTurnEndGlobal += OnTurnEndGlobal;
        }

        if (current.GetMethod("OnTurnStartLocal").DeclaringType != defaultType)
        {
            m.OnTurnStartLocal += OnTurnStartLocal;
        }

        if (current.GetMethod("OnTurnEndLocal").DeclaringType != defaultType)
        {
            m.OnTurnEndLocal += OnTurnEndLocal;
        }

        if (current.GetMethod("OnMove").DeclaringType != defaultType)
        {
            m.OnMove += OnMove;
        }

        if (current.GetMethod("OnFullyHealed").DeclaringType != defaultType)
        {
            m.OnFullyHealed += OnFullyHealed;
        }
        
        if (current.GetMethod("OnDeath").DeclaringType != defaultType)
        {
            m.OnDeath += OnDeath;
        }

        if (current.GetMethod("OnEnergyGained").DeclaringType != defaultType)
        {
            m.OnEnergyGained += OnEnergyGained;
        }

        if (current.GetMethod("OnAttacked").DeclaringType != defaultType)
        {
            m.OnAttacked += OnAttacked;
        }

        if (current.GetMethod("OnTakeDamage").DeclaringType != defaultType)
        {
            m.OnTakeDamage += OnTakeDamage;
        }

        if (current.GetMethod("OnHealing").DeclaringType != defaultType)
        {
            m.OnHealing += OnHealing;
        }

        if (current.GetMethod("OnApplyStatusEffects").DeclaringType != defaultType)
        {
            m.OnApplyStatusEffects += OnApplyStatusEffects;
        }

        OnConnection();
    }

    public void Disconnect()
    {
        OnDisconnection();
        Monster m = target;
        Type current = this.GetType();
        Type defaultType = typeof(Effect);

        if (current.GetMethod("RegenerateStats").DeclaringType != defaultType)
        {
            m.RegenerateStats -= RegenerateStats;
        }

        if (current.GetMethod("OnTurnStartGlobal").DeclaringType != defaultType)
        {
            m.OnTurnStartGlobal -= OnTurnStartGlobal;
        }

        if (current.GetMethod("OnTurnEndGlobal").DeclaringType != defaultType)
        {
            m.OnTurnEndGlobal -= OnTurnEndGlobal;
        }

        if (current.GetMethod("OnTurnStartLocal").DeclaringType != defaultType)
        {
            m.OnTurnStartLocal -= OnTurnStartLocal;
        }

        if (current.GetMethod("OnTurnEndLocal").DeclaringType != defaultType)
        {
            m.OnTurnEndLocal -= OnTurnEndLocal;
        }

        if (current.GetMethod("OnMove").DeclaringType != defaultType)
        {
            m.OnMove -= OnMove;
        }

        if (current.GetMethod("OnFullyHealed").DeclaringType != defaultType)
        {
            m.OnFullyHealed -= OnFullyHealed;
        }
        
        if (current.GetMethod("OnDeath").DeclaringType != defaultType)
        {
            m.OnDeath -= OnDeath;
        }

        if (current.GetMethod("OnEnergyGained").DeclaringType != defaultType)
        {
            m.OnEnergyGained -= OnEnergyGained;
        }

        if (current.GetMethod("OnAttacked").DeclaringType != defaultType)
        {
            m.OnAttacked -= OnAttacked;
        }

        if (current.GetMethod("OnTakeDamage").DeclaringType != defaultType)
        {
            m.OnTakeDamage -= OnTakeDamage;
        }

        if (current.GetMethod("OnHealing").DeclaringType != defaultType)
        {
            m.OnHealing -= OnHealing;
        }

        if (current.GetMethod("OnApplyStatusEffects").DeclaringType != defaultType)
        {
            m.OnApplyStatusEffects -= OnApplyStatusEffects;
        }

        ReadyToDelete = true;
    }

    public virtual void OnConnection() {}

    public virtual void OnDisconnection() { }

    //Empty 
    public virtual void OnTurnStartGlobal() {}
    public virtual void OnTurnEndGlobal() {}
    public virtual void OnTurnStartLocal() {}
    public virtual void OnTurnEndLocal() {}
    public virtual void OnMove() {}
    public virtual void OnFullyHealed() {}
    public virtual void OnDeath() {}
    
    //Stat block
    public virtual void RegenerateStats(ref StatBlock stats) {}


    //EntityEvent events
    public virtual void OnEnergyGained(ref int energy) {}
    public virtual void OnAttacked(ref int pierce, ref int accuracy) {}
    public virtual void OnTakeDamage(ref int damage, ref DamageType damageType) { }
    public virtual void OnHealing(ref int healAmount) {}
    public virtual void OnApplyStatusEffects(ref Effect[] effects) {}
}
