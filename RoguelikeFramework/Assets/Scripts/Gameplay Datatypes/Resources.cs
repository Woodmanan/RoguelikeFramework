using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct ResourceList
{ 
    //AUTO VARIABLES
    public int health;
    public int mana;
    public int stamina;

    public int this[Resource resource]
    {
        get { 
            switch ((int) resource)
            {
                //AUTO GET SWITCH
                case 0: //RESOURCE.HEALTH
                    return health;
                case 1: //RESOURCE.MANA
                    return mana;
                case 2: //RESOURCE.STAMINA
                    return stamina;
                default:
                    return -1;
            }
        }
        set 
        { 
            switch ((int) resource)
            {
                //AUTO SET SWITCH
                case 0: //RESOURCE.HEALTH
                    health = value;
                    break;
                case 1: //RESOURCE.MANA
                    mana = value;
                    break;
                case 2: //RESOURCE.STAMINA
                    stamina = value;
                    break;
            }
        }
    }

    public static ResourceList operator +(ResourceList a, ResourceList b)
    {
        ResourceList r = new ResourceList();
        
        //AUTO PLUS
        r.health = a.health + b.health;
        r.mana = a.mana + b.mana;
        r.stamina = a.stamina + b.stamina;

        return r;
    }

    public static ResourceList operator -(ResourceList a, ResourceList b)
    {
        ResourceList r = new ResourceList();
        
        //AUTO MINUS
        r.health = a.health - b.health;
        r.mana = a.mana - b.mana;
        r.stamina = a.stamina - b.stamina;

        return r;
    }
}

public struct ResourceCost
{
    public Resource resource;
    public int amount;
}
