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
    public void Use(Monster weilding, Monster target)
    {
        if (target.Attack(piercing, accuracy))
        {
            //We hit!
            foreach (DamagePairing p in damage)
            {
                target.TakeDamage(p.damage.evaluate(), p.type);
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
