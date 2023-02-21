using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipableItem : MonoBehaviour
{
    [Header("Equipable Attributes")]
    public EquipSlotType primarySlot;
    public List<EquipSlotType> secondarySlots;
    public Stats addedStats;
    public Stats statsPerEnchantment;

    [SerializeReference] public List<Effect> addedEffects;

    private List<Effect> clonedEffects = new List<Effect>();
    public bool isEquipped = false;
    public bool removable = true;
    public bool blocksUnarmed = true;

    Monster equippedTo;
    int equippedIndex; //Primary index
    Item item;

    // Start is called before the first frame update
    void Start()
    {
        //Variable compile for expensive assertion
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Assert(!GetComponent<Item>().stackable, "Equipable item should never be stackable!", this);
        #endif

        item = GetComponent<Item>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Removes this item from the monster it's attached to
    public void Unequip()
    {
        if (equippedTo)
        {
            equippedTo.equipment.Unequip(item);
        }
    }

    public void OnEquip(Monster m)
    {
        isEquipped = true;
        equippedTo = m;
        m.currentStats += GetStats(); //Immediate stat benefit
        m.connections.RegenerateStats.AddListener(0, RegenerateStats); //Hook up for next regen

        //Clone effects, so they can reapply
        clonedEffects.Clear();
        foreach (Effect e in addedEffects)
        {
            clonedEffects.Add(e.Instantiate());
        }

        //Attach connections to item
        foreach (Effect effect in clonedEffects)
        {
            effect.connectedTo.item = item;
        }

        m.AddEffect(clonedEffects.ToArray()); //Immediate status effect add
    }



    public void OnUnequip()
    {
        equippedTo.currentStats -= addedStats;

        //Disconnect all old effects
        foreach (Effect e in clonedEffects)
        {
            e.Disconnect();
        }
        equippedTo.connections.RegenerateStats.RemoveListener(RegenerateStats);
        isEquipped = false;
        equippedTo = null;
    }

    public void RegenerateStats(ref Stats block)
    {
        block += GetStats();
    }

    public Stats GetStats()
    {
        if (item && item.enchantment > 0)
        {
            return addedStats + (statsPerEnchantment * item.enchantment);
        }
        return addedStats;
    }
}
