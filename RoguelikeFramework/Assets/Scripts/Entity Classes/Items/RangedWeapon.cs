using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RangedWeapon : TargetableItem
{
    public int accuracy;
    public int piercing;
    public List<DamagePairing> damage;
    public List<ChanceEffect> effects;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Fire(Monster firing)
    {
        foreach (Monster m in targeting.affected)
        {
            if (m.Attack(piercing, accuracy))
            {
                //We hit!
                foreach (DamagePairing p in damage)
                {
                    m.TakeDamage(p.damage.evaluate(), p.type, "{name} %s{get|gets} shot for {damage} damage");
                }
                foreach (ChanceEffect c in effects)
                {
                    if (c.evaluate())
                    {
                        m.AddEffect(c.appliedEffects.ToArray());
                    }
                }
            }
        }
    }
}
