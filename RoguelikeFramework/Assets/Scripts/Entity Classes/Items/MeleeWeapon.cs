using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This one inherits from equippable, because it's essentially just a small bit of extra behaviour
public class MeleeWeapon : EquipableItem
{
    public int accuracy;
    public int piercing;
    public List<DamagePairing> damage;
    public List<ChanceEffect> effects;

    //TODO: CONSOLE LOG!
    public void Use(Monster wielding, Monster target)
    {
        if (target.Attack(piercing, accuracy))
        {
            //We hit!
            foreach (DamagePairing p in damage)
            {
                target.Damage(wielding, p.damage.evaluate(), p.type, DamageSource.MELEEATTACK);
            }
            foreach (ChanceEffect c in effects)
            {
                if (c.evaluate())
                {
                    target.AddEffect(c.appliedEffects.ToArray());
                }
            }
        }
    }
}
