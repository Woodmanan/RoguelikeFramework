using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Weapon : MonoBehaviour
{
    public WeaponBlock primary;
    public WeaponBlock secondary;
    public int enchantment;
    public int maxEnchantment;

    [HideInInspector] public DamageSource source;
    public Connections connections;

    [HideInInspector] public Item item;

    // Start is called before the first frame update
    public virtual void Start()
    {
        item = GetComponent<Item>();
        connections = item.connections;
        
        source = (item.type == ItemType.MELEE_WEAPON) ? DamageSource.MELEEATTACK : DamageSource.RANGEDATTACK;

        enchantment = Mathf.Clamp(enchantment, -1 * maxEnchantment, maxEnchantment);
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
            Combat.Hit(attacker, defender, source, primary, enchantment: enchantment);
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
            Combat.Hit(attacker, defender, source, secondary, enchantment: enchantment);
        }

        defender.connections.OnAfterSecondaryAttackTarget
            .Invoke(ref weapon, ref action, ref result);

        attacker.connections.OnEndSecondaryAttack
            .BlendInvoke(connections.OnEndSecondaryAttack, ref weapon, ref action, ref result);

        attacker.RemoveConnection(connections);

        return result;
    }

    public bool CanAddEnchantment()
    {
        return enchantment < maxEnchantment;
    }


    public void AddEnchantment(int amount)
    {
        enchantment = Mathf.Clamp(enchantment + amount, -maxEnchantment, maxEnchantment);
    }
}
