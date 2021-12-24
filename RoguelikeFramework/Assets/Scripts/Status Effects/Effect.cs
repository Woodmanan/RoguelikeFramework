using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

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
    //AUTO: Connection count
    [HideInInspector] public const int connectionCount = 15;

    [HideInInspector] public Connections connectedTo;
    [HideInInspector] public bool ReadyToDelete = false;

    public static Dictionary<Type, int[]> connectionDict = new Dictionary<Type, int[]>();

    public virtual int priority { get { return 5; } }

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
    public void Connect(Connections c)
    {
        connectedTo = c;
        
        if (!connectionDict.ContainsKey(this.GetType()))
        {
            SetupConnections();
        }

        int[] connections;
        if (!connectionDict.TryGetValue(this.GetType(), out connections))
        {
            Debug.LogError($"Effect {this.GetType().Name} was unable to find its connection list. This is bad.");
        }
        Type current = this.GetType();
        Type defaultType = typeof(Effect);

        //BEGIN AUTO CONNECT

        if (connections[0] >= 0) { c.OnTurnStartGlobal.AddListener(connections[0], OnTurnStartGlobal); }

        if (connections[1] >= 0) { c.OnTurnEndGlobal.AddListener(connections[1], OnTurnEndGlobal); }

        if (connections[2] >= 0) { c.OnTurnStartLocal.AddListener(connections[2], OnTurnStartLocal); }

        if (connections[3] >= 0) { c.OnTurnEndLocal.AddListener(connections[3], OnTurnEndLocal); }

        if (connections[4] >= 0) { c.OnMove.AddListener(connections[4], OnMove); }

        if (connections[5] >= 0) { c.OnFullyHealed.AddListener(connections[5], OnFullyHealed); }

        if (connections[6] >= 0) { c.OnDeath.AddListener(connections[6], OnDeath); }

        if (connections[7] >= 0) { c.RegenerateStats.AddListener(connections[7], RegenerateStats); }

        if (connections[8] >= 0) { c.OnEnergyGained.AddListener(connections[8], OnEnergyGained); }

        if (connections[9] >= 0) { c.OnAttacked.AddListener(connections[9], OnAttacked); }

        if (connections[10] >= 0) { c.OnTakeDamage.AddListener(connections[10], OnTakeDamage); }

        if (connections[11] >= 0) { c.OnHealing.AddListener(connections[11], OnHealing); }

        if (connections[12] >= 0) { c.OnApplyStatusEffects.AddListener(connections[12], OnApplyStatusEffects); }


        OnConnection();
    }

    //Extremely expensive, and terrible. On the flipside, this now only needs to happen
    //once per time the game is opened, instead of most of this work happening every frame. Win!
    public void SetupConnections()
    {
        //AUTO VARIABLE
        int numConnections = 13;

        int[] connections = new int[numConnections];

        //This part of the code is made autonomously, and gets kind of messy.
        //This manual way of doing it sucks, but should be MUCH faster than the old way of
        //doing it. You (the coder) shouldn't need to worry about how this works, and it should
        //just behave like ~magic~. If you're curious, though, this is the gross manual way of 
        //doing this.

        MethodInfo method;

        //AUTO SETUP

        //-------------------- OnTurnStartGlobal --------------------
        method = ((ActionRef) OnTurnStartGlobal).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[0] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[0] = this.priority;
            }

        }
        else
        {
            connections[0] = -1;
        }
        //------------------------------------------------------------

        //--------------------- OnTurnEndGlobal ---------------------
        method = ((ActionRef) OnTurnEndGlobal).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[1] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[1] = this.priority;
            }

        }
        else
        {
            connections[1] = -1;
        }
        //------------------------------------------------------------

        //--------------------- OnTurnStartLocal ---------------------
        method = ((ActionRef) OnTurnStartLocal).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[2] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[2] = this.priority;
            }

        }
        else
        {
            connections[2] = -1;
        }
        //------------------------------------------------------------

        //---------------------- OnTurnEndLocal ----------------------
        method = ((ActionRef) OnTurnEndLocal).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[3] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[3] = this.priority;
            }

        }
        else
        {
            connections[3] = -1;
        }
        //------------------------------------------------------------

        //-------------------------- OnMove --------------------------
        method = ((ActionRef) OnMove).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[4] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[4] = this.priority;
            }

        }
        else
        {
            connections[4] = -1;
        }
        //------------------------------------------------------------

        //---------------------- OnFullyHealed ----------------------
        method = ((ActionRef) OnFullyHealed).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[5] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[5] = this.priority;
            }

        }
        else
        {
            connections[5] = -1;
        }
        //------------------------------------------------------------

        //------------------------- OnDeath -------------------------
        method = ((ActionRef) OnDeath).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[6] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[6] = this.priority;
            }

        }
        else
        {
            connections[6] = -1;
        }
        //------------------------------------------------------------

        //--------------------- RegenerateStats ---------------------
        method = ((ActionRef<StatBlock>) RegenerateStats).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[7] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[7] = this.priority;
            }

        }
        else
        {
            connections[7] = -1;
        }
        //------------------------------------------------------------

        //---------------------- OnEnergyGained ----------------------
        method = ((ActionRef<int>) OnEnergyGained).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[8] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[8] = this.priority;
            }

        }
        else
        {
            connections[8] = -1;
        }
        //------------------------------------------------------------

        //------------------------ OnAttacked ------------------------
        method = ((ActionRef<int, int>) OnAttacked).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[9] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[9] = this.priority;
            }

        }
        else
        {
            connections[9] = -1;
        }
        //------------------------------------------------------------

        //----------------------- OnTakeDamage -----------------------
        method = ((ActionRef<int, DamageType>) OnTakeDamage).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[10] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[10] = this.priority;
            }

        }
        else
        {
            connections[10] = -1;
        }
        //------------------------------------------------------------

        //------------------------ OnHealing ------------------------
        method = ((ActionRef<int>) OnHealing).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[11] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[11] = this.priority;
            }

        }
        else
        {
            connections[11] = -1;
        }
        //------------------------------------------------------------

        //------------------- OnApplyStatusEffects -------------------
        method = ((ActionRef<Effect[]>) OnApplyStatusEffects).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[12] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[12] = this.priority;
            }

        }
        else
        {
            connections[12] = -1;
        }
        //------------------------------------------------------------



        connectionDict.Add(this.GetType(), connections);
    }

    public void Disconnect()
    {
        Debug.Log("Disconnecting!");
        OnDisconnection();
        Connections c = connectedTo;
        Type current = this.GetType();
        Type defaultType = typeof(Effect);

        //BEGIN AUTO DISCONNECT

        c.RegenerateStats.RemoveListener(RegenerateStats);
        
        c.OnTurnStartGlobal.RemoveListener(OnTurnStartGlobal);

        c.OnTurnEndGlobal.RemoveListener(OnTurnEndGlobal);

        Debug.Log($"Number before disconnect: {c.OnTurnStartLocal.delegates.Count}");
        c.OnTurnStartLocal.RemoveListener(OnTurnStartLocal);
        Debug.Log($"Number after disconnect: {c.OnTurnStartLocal.delegates.Count}");

        c.OnTurnEndLocal.RemoveListener(OnTurnEndLocal);

        c.OnMove.RemoveListener(OnMove);

        c.OnFullyHealed.RemoveListener(OnFullyHealed);

        c.OnDeath.RemoveListener(OnDeath);

        c.OnEnergyGained.RemoveListener(OnEnergyGained);

        c.OnAttacked.RemoveListener(OnAttacked);

        c.OnTakeDamage.RemoveListener(OnTakeDamage);

        c.OnHealing.RemoveListener(OnHealing);

        c.OnApplyStatusEffects.RemoveListener(OnApplyStatusEffects);

        //END AUTO DISCONNECT

        ReadyToDelete = true;
        
    }

    public virtual void OnConnection() {}
    public virtual void OnDisconnection() {}

    //AUTO DECLARATIONS

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
