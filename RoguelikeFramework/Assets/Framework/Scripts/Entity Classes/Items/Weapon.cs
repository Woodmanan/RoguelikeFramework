using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Weapon : MonoBehaviour
{
    public WeaponBlock primary;
    public WeaponBlock secondary;
    public DamageSource source;
    public Connections connections;

    [HideInInspector] public Item item;

    // Start is called before the first frame update
    public virtual void Start()
    {
        item = GetComponent<Item>();
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
        attacker.other = connections;
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
            Combat.Hit(attacker, defender, source, primary);
        }

        defender.connections.OnAfterPrimaryAttackTarget
            .Invoke(ref weapon, ref action, ref result);

        attacker.connections.OnEndPrimaryAttack
            .BlendInvoke(connections.OnEndPrimaryAttack, ref weapon, ref action, ref result);

        attacker.other = null;

        return result;
    }

    public AttackResult SecondaryAttack(Monster attacker, Monster defender, AttackAction action)
    {
        attacker.other = connections;
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
            Combat.Hit(attacker, defender, source, secondary);
        }

        defender.connections.OnAfterSecondaryAttackTarget
            .Invoke(ref weapon, ref action, ref result);

        attacker.connections.OnEndSecondaryAttack
            .BlendInvoke(connections.OnEndSecondaryAttack, ref weapon, ref action, ref result);

        attacker.other = null;

        return result;
    }
}
