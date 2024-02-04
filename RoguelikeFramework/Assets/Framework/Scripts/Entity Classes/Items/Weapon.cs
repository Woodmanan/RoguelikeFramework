using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Weapon : MonoBehaviour
{
    public WeaponBlock primary;
    public WeaponBlock secondary;

    [HideInInspector] public DamageSource source;
    [System.NonSerialized]
    public Connections connections;

    [HideInInspector] public Item item;

    // Start is called before the first frame update
    public virtual void Awake()
    {
        item = GetComponent<Item>();
        item.Setup();
        connections = item.connections;
        
        source = (item.type == ItemType.MELEE_WEAPON) ? DamageSource.MELEEATTACK : DamageSource.RANGEDATTACK;
    }

    public void AddEffect(params Effect[] effects)
    {
        item.AddEffect(effects);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public AttackResult PrimaryAttack(Monster attacker, Monster defender, AttackAction action)
    {
        if (attacker.IsDead() || defender.IsDead()) return AttackResult.MISSED;

        attacker.AddConnection(connections);
        Weapon weapon = this;

        attacker.connections.OnBeginPrimaryAttack
            .BlendInvoke(connections.OnBeginPrimaryAttack, ref weapon, ref action);

        AttackResult result = Combat.DetermineHit(action.target, primary);

        defender.connections.OnBeforePrimaryAttackTarget
            .Invoke(ref weapon, ref action, ref result);

        attacker.connections.OnPrimaryAttackResult
            .BlendInvoke(connections.OnPrimaryAttackResult, ref weapon, ref action, ref result);

        if (result == AttackResult.HIT)
        {
            RogueLog.singleton.Log($"{attacker.GetName()} hits {defender.GetName()} with its {item.GetNameClean()}!", priority: LogPriority.COMBAT);
            Combat.Hit(attacker, defender, source, primary, enchantment: GetEnchantment());
        }
        else
        {
            RogueLog.singleton.Log($"The {attacker.GetName()} misses with its {item.GetNameClean()}!", priority: LogPriority.COMBAT);
        }

        defender.connections.OnAfterPrimaryAttackTarget
            .Invoke(ref weapon, ref action, ref result);

        attacker.connections.OnEndPrimaryAttack
            .BlendInvoke(connections.OnEndPrimaryAttack, ref weapon, ref action, ref result);

        attacker.RemoveConnection(connections);

        return result;
    }

    public AttackResult SecondaryAttack(Monster attacker, Monster defender, AttackAction action)
    {
        if (attacker.IsDead() || defender.IsDead()) return AttackResult.MISSED;

        attacker.AddConnection(connections);
        Weapon weapon = this;

        attacker.connections.OnBeginSecondaryAttack
            .BlendInvoke(connections.OnBeginSecondaryAttack, ref weapon, ref action);

        AttackResult result = Combat.DetermineHit(action.target, secondary);

        defender.connections.OnBeforeSecondaryAttackTarget
            .Invoke(ref weapon, ref action, ref result);

        attacker.connections.OnSecondaryAttackResult
            .BlendInvoke(connections.OnSecondaryAttackResult, ref weapon, ref action, ref result);

        if (result == AttackResult.HIT)
        {
            RogueLog.singleton.Log($"{attacker.GetName()} hits {defender.GetName()} with its {item.GetNameClean()}!", priority: LogPriority.COMBAT);
            Combat.Hit(attacker, defender, source, secondary, enchantment: GetEnchantment());
        }
        else
        {
            RogueLog.singleton.Log($"The {attacker.GetLocalizedName()} misses with its {item.GetNameClean()}!", priority: LogPriority.COMBAT);
        }

        defender.connections.OnAfterSecondaryAttackTarget
            .Invoke(ref weapon, ref action, ref result);

        attacker.connections.OnEndSecondaryAttack
            .BlendInvoke(connections.OnEndSecondaryAttack, ref weapon, ref action, ref result);

        attacker.RemoveConnection(connections);

        return result;
    }

    public int GetEnchantment()
    {
        return (item != null) ? item.enchantment : 0;
    }
}
