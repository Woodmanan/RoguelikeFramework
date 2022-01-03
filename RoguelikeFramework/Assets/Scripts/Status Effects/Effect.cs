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

        if (connections[6] >= 0) { c.OnKillMonster.AddListener(connections[6], OnKillMonster); }

        if (connections[7] >= 0) { c.OnDeath.AddListener(connections[7], OnDeath); }

        if (connections[8] >= 0) { c.RegenerateStats.AddListener(connections[8], RegenerateStats); }

        if (connections[9] >= 0) { c.OnEnergyGained.AddListener(connections[9], OnEnergyGained); }

        if (connections[10] >= 0) { c.OnAttacked.AddListener(connections[10], OnAttacked); }

        if (connections[11] >= 0) { c.OnDealDamage.AddListener(connections[11], OnDealDamage); }

        if (connections[12] >= 0) { c.OnTakeDamage.AddListener(connections[12], OnTakeDamage); }

        if (connections[13] >= 0) { c.OnHealing.AddListener(connections[13], OnHealing); }

        if (connections[14] >= 0) { c.OnApplyStatusEffects.AddListener(connections[14], OnApplyStatusEffects); }

        if (connections[15] >= 0) { c.OnCastAbility.AddListener(connections[15], OnCastAbility); }

        if (connections[16] >= 0) { c.OnGainResources.AddListener(connections[16], OnGainResources); }

        if (connections[17] >= 0) { c.OnLoseResources.AddListener(connections[17], OnLoseResources); }

        if (connections[18] >= 0) { c.OnRegenerateAbilityStats.AddListener(connections[18], OnRegenerateAbilityStats); }

        if (connections[19] >= 0) { c.OnCheckAvailability.AddListener(connections[19], OnCheckAvailability); }

        if (connections[20] >= 0) { c.OnTargetsSelected.AddListener(connections[20], OnTargetsSelected); }

        if (connections[21] >= 0) { c.OnPreCast.AddListener(connections[21], OnPreCast); }

        if (connections[22] >= 0) { c.OnPostCast.AddListener(connections[22], OnPostCast); }

        if (connections[23] >= 0) { c.OnTargetedByAbility.AddListener(connections[23], OnTargetedByAbility); }

        if (connections[24] >= 0) { c.OnHitByAbility.AddListener(connections[24], OnHitByAbility); }

        if (connections[25] >= 0) { c.OnStartAttack.AddListener(connections[25], OnStartAttack); }

        if (connections[26] >= 0) { c.OnGenerateArmedAttacks.AddListener(connections[26], OnGenerateArmedAttacks); }

        if (connections[27] >= 0) { c.OnBeginPrimaryAttack.AddListener(connections[27], OnBeginPrimaryAttack); }

        if (connections[28] >= 0) { c.OnPrimaryAttackResult.AddListener(connections[28], OnPrimaryAttackResult); }

        if (connections[29] >= 0) { c.OnEndPrimaryAttack.AddListener(connections[29], OnEndPrimaryAttack); }

        if (connections[30] >= 0) { c.OnBeginSecondaryAttack.AddListener(connections[30], OnBeginSecondaryAttack); }

        if (connections[31] >= 0) { c.OnSecondaryAttackResult.AddListener(connections[31], OnSecondaryAttackResult); }

        if (connections[32] >= 0) { c.OnEndSecondaryAttack.AddListener(connections[32], OnEndSecondaryAttack); }

        if (connections[33] >= 0) { c.OnGenerateUnarmedAttacks.AddListener(connections[33], OnGenerateUnarmedAttacks); }

        if (connections[34] >= 0) { c.OnBeginUnarmedAttack.AddListener(connections[34], OnBeginUnarmedAttack); }

        if (connections[35] >= 0) { c.OnUnarmedAttackResult.AddListener(connections[35], OnUnarmedAttackResult); }

        if (connections[36] >= 0) { c.OnEndUnarmedAttack.AddListener(connections[36], OnEndUnarmedAttack); }

        if (connections[37] >= 0) { c.OnBeforePrimaryAttackTarget.AddListener(connections[37], OnBeforePrimaryAttackTarget); }

        if (connections[38] >= 0) { c.OnAfterPrimaryAttackTarget.AddListener(connections[38], OnAfterPrimaryAttackTarget); }

        if (connections[39] >= 0) { c.OnBeforeSecondaryAttackTarget.AddListener(connections[39], OnBeforeSecondaryAttackTarget); }

        if (connections[40] >= 0) { c.OnAfterSecondaryAttackTarget.AddListener(connections[40], OnAfterSecondaryAttackTarget); }

        if (connections[41] >= 0) { c.OnBeforeUnarmedAttackTarget.AddListener(connections[41], OnBeforeUnarmedAttackTarget); }

        if (connections[42] >= 0) { c.OnAfterUnarmedAttackTarget.AddListener(connections[42], OnAfterUnarmedAttackTarget); }


        OnConnection();
    }

    //Extremely expensive, and terrible. On the flipside, this now only needs to happen
    //once per time the game is opened, instead of most of this work happening every frame. Win!
    public void SetupConnections()
    {
        //AUTO VARIABLE
        int numConnections = 43;

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

        //---------------------- OnKillMonster ----------------------
        method = ((ActionRef<Monster, DamageType, DamageSource>) OnKillMonster).Method;
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

        //------------------------- OnDeath -------------------------
        method = ((ActionRef) OnDeath).Method;
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

        //--------------------- RegenerateStats ---------------------
        method = ((ActionRef<StatBlock>) RegenerateStats).Method;
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

        //---------------------- OnEnergyGained ----------------------
        method = ((ActionRef<int>) OnEnergyGained).Method;
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

        //------------------------ OnAttacked ------------------------
        method = ((ActionRef<int, int>) OnAttacked).Method;
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

        //----------------------- OnDealDamage -----------------------
        method = ((ActionRef<int, DamageType, DamageSource>) OnDealDamage).Method;
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

        //----------------------- OnTakeDamage -----------------------
        method = ((ActionRef<int, DamageType, DamageSource>) OnTakeDamage).Method;
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

        //------------------------ OnHealing ------------------------
        method = ((ActionRef<int>) OnHealing).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[13] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[13] = this.priority;
            }

        }
        else
        {
            connections[13] = -1;
        }
        //------------------------------------------------------------

        //------------------- OnApplyStatusEffects -------------------
        method = ((ActionRef<Effect[]>) OnApplyStatusEffects).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[14] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[14] = this.priority;
            }

        }
        else
        {
            connections[14] = -1;
        }
        //------------------------------------------------------------

        //---------------------- OnCastAbility ----------------------
        method = ((ActionRef<AbilityAction, bool>) OnCastAbility).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[15] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[15] = this.priority;
            }

        }
        else
        {
            connections[15] = -1;
        }
        //------------------------------------------------------------

        //--------------------- OnGainResources ---------------------
        method = ((ActionRef<ResourceList>) OnGainResources).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[16] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[16] = this.priority;
            }

        }
        else
        {
            connections[16] = -1;
        }
        //------------------------------------------------------------

        //--------------------- OnLoseResources ---------------------
        method = ((ActionRef<ResourceList>) OnLoseResources).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[17] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[17] = this.priority;
            }

        }
        else
        {
            connections[17] = -1;
        }
        //------------------------------------------------------------

        //----------------- OnRegenerateAbilityStats -----------------
        method = ((ActionRef<Targeting, AbilityBlock, Ability>) OnRegenerateAbilityStats).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[18] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[18] = this.priority;
            }

        }
        else
        {
            connections[18] = -1;
        }
        //------------------------------------------------------------

        //------------------- OnCheckAvailability -------------------
        method = ((ActionRef<Ability, bool>) OnCheckAvailability).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[19] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[19] = this.priority;
            }

        }
        else
        {
            connections[19] = -1;
        }
        //------------------------------------------------------------

        //-------------------- OnTargetsSelected --------------------
        method = ((ActionRef<Targeting, Ability>) OnTargetsSelected).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[20] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[20] = this.priority;
            }

        }
        else
        {
            connections[20] = -1;
        }
        //------------------------------------------------------------

        //------------------------ OnPreCast ------------------------
        method = ((ActionRef<Ability>) OnPreCast).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[21] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[21] = this.priority;
            }

        }
        else
        {
            connections[21] = -1;
        }
        //------------------------------------------------------------

        //------------------------ OnPostCast ------------------------
        method = ((ActionRef<Ability>) OnPostCast).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[22] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[22] = this.priority;
            }

        }
        else
        {
            connections[22] = -1;
        }
        //------------------------------------------------------------

        //------------------- OnTargetedByAbility -------------------
        method = ((ActionRef<AbilityAction>) OnTargetedByAbility).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[23] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[23] = this.priority;
            }

        }
        else
        {
            connections[23] = -1;
        }
        //------------------------------------------------------------

        //---------------------- OnHitByAbility ----------------------
        method = ((ActionRef<AbilityAction>) OnHitByAbility).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[24] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[24] = this.priority;
            }

        }
        else
        {
            connections[24] = -1;
        }
        //------------------------------------------------------------

        //---------------------- OnStartAttack ----------------------
        method = ((ActionRef<AttackAction, bool>) OnStartAttack).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[25] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[25] = this.priority;
            }

        }
        else
        {
            connections[25] = -1;
        }
        //------------------------------------------------------------

        //------------------ OnGenerateArmedAttacks ------------------
        method = ((ActionRef<List<Weapon>, List<Weapon>>) OnGenerateArmedAttacks).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[26] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[26] = this.priority;
            }

        }
        else
        {
            connections[26] = -1;
        }
        //------------------------------------------------------------

        //------------------- OnBeginPrimaryAttack -------------------
        method = ((ActionRef<Weapon, AttackAction>) OnBeginPrimaryAttack).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[27] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[27] = this.priority;
            }

        }
        else
        {
            connections[27] = -1;
        }
        //------------------------------------------------------------

        //------------------ OnPrimaryAttackResult ------------------
        method = ((ActionRef<Weapon, AttackAction, AttackResult>) OnPrimaryAttackResult).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[28] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[28] = this.priority;
            }

        }
        else
        {
            connections[28] = -1;
        }
        //------------------------------------------------------------

        //-------------------- OnEndPrimaryAttack --------------------
        method = ((ActionRef<Weapon, AttackAction, AttackResult>) OnEndPrimaryAttack).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[29] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[29] = this.priority;
            }

        }
        else
        {
            connections[29] = -1;
        }
        //------------------------------------------------------------

        //------------------ OnBeginSecondaryAttack ------------------
        method = ((ActionRef<Weapon, AttackAction>) OnBeginSecondaryAttack).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[30] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[30] = this.priority;
            }

        }
        else
        {
            connections[30] = -1;
        }
        //------------------------------------------------------------

        //----------------- OnSecondaryAttackResult -----------------
        method = ((ActionRef<Weapon, AttackAction, AttackResult>) OnSecondaryAttackResult).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[31] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[31] = this.priority;
            }

        }
        else
        {
            connections[31] = -1;
        }
        //------------------------------------------------------------

        //------------------- OnEndSecondaryAttack -------------------
        method = ((ActionRef<Weapon, AttackAction, AttackResult>) OnEndSecondaryAttack).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[32] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[32] = this.priority;
            }

        }
        else
        {
            connections[32] = -1;
        }
        //------------------------------------------------------------

        //----------------- OnGenerateUnarmedAttacks -----------------
        method = ((ActionRef<List<EquipmentSlot>>) OnGenerateUnarmedAttacks).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[33] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[33] = this.priority;
            }

        }
        else
        {
            connections[33] = -1;
        }
        //------------------------------------------------------------

        //------------------- OnBeginUnarmedAttack -------------------
        method = ((ActionRef<EquipmentSlot, AttackAction>) OnBeginUnarmedAttack).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[34] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[34] = this.priority;
            }

        }
        else
        {
            connections[34] = -1;
        }
        //------------------------------------------------------------

        //------------------ OnUnarmedAttackResult ------------------
        method = ((ActionRef<EquipmentSlot, AttackAction, AttackResult>) OnUnarmedAttackResult).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[35] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[35] = this.priority;
            }

        }
        else
        {
            connections[35] = -1;
        }
        //------------------------------------------------------------

        //-------------------- OnEndUnarmedAttack --------------------
        method = ((ActionRef<EquipmentSlot, AttackAction, AttackResult>) OnEndUnarmedAttack).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[36] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[36] = this.priority;
            }

        }
        else
        {
            connections[36] = -1;
        }
        //------------------------------------------------------------

        //--------------- OnBeforePrimaryAttackTarget ---------------
        method = ((ActionRef<Weapon, AttackAction, AttackResult>) OnBeforePrimaryAttackTarget).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[37] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[37] = this.priority;
            }

        }
        else
        {
            connections[37] = -1;
        }
        //------------------------------------------------------------

        //---------------- OnAfterPrimaryAttackTarget ----------------
        method = ((ActionRef<Weapon, AttackAction, AttackResult>) OnAfterPrimaryAttackTarget).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[38] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[38] = this.priority;
            }

        }
        else
        {
            connections[38] = -1;
        }
        //------------------------------------------------------------

        //-------------- OnBeforeSecondaryAttackTarget --------------
        method = ((ActionRef<Weapon, AttackAction, AttackResult>) OnBeforeSecondaryAttackTarget).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[39] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[39] = this.priority;
            }

        }
        else
        {
            connections[39] = -1;
        }
        //------------------------------------------------------------

        //--------------- OnAfterSecondaryAttackTarget ---------------
        method = ((ActionRef<Weapon, AttackAction, AttackResult>) OnAfterSecondaryAttackTarget).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[40] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[40] = this.priority;
            }

        }
        else
        {
            connections[40] = -1;
        }
        //------------------------------------------------------------

        //--------------- OnBeforeUnarmedAttackTarget ---------------
        method = ((ActionRef<EquipmentSlot, AttackAction, AttackResult>) OnBeforeUnarmedAttackTarget).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[41] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[41] = this.priority;
            }

        }
        else
        {
            connections[41] = -1;
        }
        //------------------------------------------------------------

        //---------------- OnAfterUnarmedAttackTarget ----------------
        method = ((ActionRef<EquipmentSlot, AttackAction, AttackResult>) OnAfterUnarmedAttackTarget).Method;
        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[42] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[42] = this.priority;
            }

        }
        else
        {
            connections[42] = -1;
        }
        //------------------------------------------------------------



        connectionDict.Add(this.GetType(), connections);
    }

    public void Disconnect()
    {
        OnDisconnection();

        int[] connections;
        if (!connectionDict.TryGetValue(this.GetType(), out connections))
        {
            Debug.LogError($"Effect {this.GetType().Name} was unable to find its connection list. This is bad.");
        }

        Connections c = connectedTo;

        //BEGIN AUTO DISCONNECT

        if (connections[0] >= 0) { c.OnTurnStartGlobal.RemoveListener(OnTurnStartGlobal); }

        if (connections[1] >= 0) { c.OnTurnEndGlobal.RemoveListener(OnTurnEndGlobal); }

        if (connections[2] >= 0) { c.OnTurnStartLocal.RemoveListener(OnTurnStartLocal); }

        if (connections[3] >= 0) { c.OnTurnEndLocal.RemoveListener(OnTurnEndLocal); }

        if (connections[4] >= 0) { c.OnMove.RemoveListener(OnMove); }

        if (connections[5] >= 0) { c.OnFullyHealed.RemoveListener(OnFullyHealed); }

        if (connections[6] >= 0) { c.OnKillMonster.RemoveListener(OnKillMonster); }

        if (connections[7] >= 0) { c.OnDeath.RemoveListener(OnDeath); }

        if (connections[8] >= 0) { c.RegenerateStats.RemoveListener(RegenerateStats); }

        if (connections[9] >= 0) { c.OnEnergyGained.RemoveListener(OnEnergyGained); }

        if (connections[10] >= 0) { c.OnAttacked.RemoveListener(OnAttacked); }

        if (connections[11] >= 0) { c.OnDealDamage.RemoveListener(OnDealDamage); }

        if (connections[12] >= 0) { c.OnTakeDamage.RemoveListener(OnTakeDamage); }

        if (connections[13] >= 0) { c.OnHealing.RemoveListener(OnHealing); }

        if (connections[14] >= 0) { c.OnApplyStatusEffects.RemoveListener(OnApplyStatusEffects); }

        if (connections[15] >= 0) { c.OnCastAbility.RemoveListener(OnCastAbility); }

        if (connections[16] >= 0) { c.OnGainResources.RemoveListener(OnGainResources); }

        if (connections[17] >= 0) { c.OnLoseResources.RemoveListener(OnLoseResources); }

        if (connections[18] >= 0) { c.OnRegenerateAbilityStats.RemoveListener(OnRegenerateAbilityStats); }

        if (connections[19] >= 0) { c.OnCheckAvailability.RemoveListener(OnCheckAvailability); }

        if (connections[20] >= 0) { c.OnTargetsSelected.RemoveListener(OnTargetsSelected); }

        if (connections[21] >= 0) { c.OnPreCast.RemoveListener(OnPreCast); }

        if (connections[22] >= 0) { c.OnPostCast.RemoveListener(OnPostCast); }

        if (connections[23] >= 0) { c.OnTargetedByAbility.RemoveListener(OnTargetedByAbility); }

        if (connections[24] >= 0) { c.OnHitByAbility.RemoveListener(OnHitByAbility); }

        if (connections[25] >= 0) { c.OnStartAttack.RemoveListener(OnStartAttack); }

        if (connections[26] >= 0) { c.OnGenerateArmedAttacks.RemoveListener(OnGenerateArmedAttacks); }

        if (connections[27] >= 0) { c.OnBeginPrimaryAttack.RemoveListener(OnBeginPrimaryAttack); }

        if (connections[28] >= 0) { c.OnPrimaryAttackResult.RemoveListener(OnPrimaryAttackResult); }

        if (connections[29] >= 0) { c.OnEndPrimaryAttack.RemoveListener(OnEndPrimaryAttack); }

        if (connections[30] >= 0) { c.OnBeginSecondaryAttack.RemoveListener(OnBeginSecondaryAttack); }

        if (connections[31] >= 0) { c.OnSecondaryAttackResult.RemoveListener(OnSecondaryAttackResult); }

        if (connections[32] >= 0) { c.OnEndSecondaryAttack.RemoveListener(OnEndSecondaryAttack); }

        if (connections[33] >= 0) { c.OnGenerateUnarmedAttacks.RemoveListener(OnGenerateUnarmedAttacks); }

        if (connections[34] >= 0) { c.OnBeginUnarmedAttack.RemoveListener(OnBeginUnarmedAttack); }

        if (connections[35] >= 0) { c.OnUnarmedAttackResult.RemoveListener(OnUnarmedAttackResult); }

        if (connections[36] >= 0) { c.OnEndUnarmedAttack.RemoveListener(OnEndUnarmedAttack); }

        if (connections[37] >= 0) { c.OnBeforePrimaryAttackTarget.RemoveListener(OnBeforePrimaryAttackTarget); }

        if (connections[38] >= 0) { c.OnAfterPrimaryAttackTarget.RemoveListener(OnAfterPrimaryAttackTarget); }

        if (connections[39] >= 0) { c.OnBeforeSecondaryAttackTarget.RemoveListener(OnBeforeSecondaryAttackTarget); }

        if (connections[40] >= 0) { c.OnAfterSecondaryAttackTarget.RemoveListener(OnAfterSecondaryAttackTarget); }

        if (connections[41] >= 0) { c.OnBeforeUnarmedAttackTarget.RemoveListener(OnBeforeUnarmedAttackTarget); }

        if (connections[42] >= 0) { c.OnAfterUnarmedAttackTarget.RemoveListener(OnAfterUnarmedAttackTarget); }

        //END AUTO DISCONNECT

        ReadyToDelete = true;
        
    }

    public virtual void OnConnection() {}
    public virtual void OnDisconnection() {}

    //AUTO DECLARATIONS

    public virtual void OnTurnStartGlobal() {}
    public virtual void OnTurnEndGlobal() {}
    public virtual void OnTurnStartLocal() {}
    public virtual void OnTurnEndLocal() {}
    public virtual void OnMove() {}
    public virtual void OnFullyHealed() {}
    public virtual void OnKillMonster(ref Monster monster, ref DamageType type, ref DamageSource source) {}
    public virtual void OnDeath() {}
    public virtual void RegenerateStats(ref StatBlock stats) {}
    public virtual void OnEnergyGained(ref int energy) {}
    public virtual void OnAttacked(ref int pierce, ref int accuracy) {}
    public virtual void OnDealDamage(ref int damage, ref DamageType damageType, ref DamageSource source) {}
    public virtual void OnTakeDamage(ref int damage, ref DamageType damageType, ref DamageSource source) {}
    public virtual void OnHealing(ref int healAmount) {}
    public virtual void OnApplyStatusEffects(ref Effect[] effects) {}
    public virtual void OnCastAbility(ref AbilityAction action, ref bool canContinue) {}
    public virtual void OnGainResources(ref ResourceList resources) {}
    public virtual void OnLoseResources(ref ResourceList resources) {}
    public virtual void OnRegenerateAbilityStats(ref Targeting targeting, ref AbilityBlock abilityBlock, ref Ability ability) {}
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
}
