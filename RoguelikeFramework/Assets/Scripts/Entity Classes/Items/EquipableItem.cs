using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipableItem : MonoBehaviour
{
    [Header("Equipable Attributes")]
    public EquipSlotType primarySlot;
    public List<EquipSlotType> secondarySlots;
    public StatBlock addedStats;
    public StatusEffectList addedEffects;
    private List<Effect> clonedEffects = new List<Effect>();
    public bool isEquipped = false;
    public bool removable = true;

    Monster equippedTo;
    int equippedIndex; //Primary index
    Item itemData;

    // Start is called before the first frame update
    void Start()
    {
        //Variable compile for expensive assertion
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Assert(!GetComponent<Item>().stackable, "Equipable item should never be stackable!", this);
        #endif

        itemData = GetComponent<Item>();
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
            equippedTo.equipment.Unequip(itemData);
        }
    }

    public void OnEquip(Monster m)
    {
        isEquipped = true;
        equippedTo = m;
        m.stats += addedStats; //Immediate stat benefit
        m.connections.RegenerateStats.AddListener(0, RegenerateStats); //Hook up for next regen

        //Clone effects, so they can reapply
        clonedEffects.Clear();
        foreach (StatusEffect e in addedEffects)
        {
            clonedEffects.Add(e.Instantiate());
        }

        m.AddEffect(clonedEffects.ToArray()); //Immediate status effect add
    }



    public void OnUnequip()
    {
        equippedTo.stats -= addedStats;

        //Disconnect all old effects
        foreach (Effect e in clonedEffects)
        {
            e.Disconnect();
        }
        equippedTo.connections.RegenerateStats.RemoveListener(RegenerateStats);
        isEquipped = false;
        equippedTo = null;
    }

    public void RegenerateStats(ref StatBlock block)
    {
        block += addedStats;
    }
}
