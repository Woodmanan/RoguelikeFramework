using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipableItem : MonoBehaviour
{
    [Header("Equipable Attributes")]
    public EquipSlotType primarySlot;
    public List<EquipSlotType> secondarySlots;
    [ResourceGroup(ResourceType.Monster)]
    public Stats addedStats;
    [ResourceGroup(ResourceType.Monster)]
    public Stats statsPerEnchantment;

    [SerializeReference] public List<Effect> addedEffects;

    private List<Effect> clonedEffects = new List<Effect>();
    public bool isEquipped = false;
    public bool removable = true;
    public bool blocksUnarmed = true;

    RogueHandle<Monster> equippedTo;
    int equippedIndex; //Primary index
    Item item;

    // Start is called before the first frame update
    void Awake()
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
        if (equippedTo.IsValid())
        {
            equippedTo[0].equipment.Unequip(item);
        }
    }

    public void OnEquip(RogueHandle<Monster> m)
    {
        item.Setup();
        isEquipped = true;
        equippedTo = m;
        m[0].currentStats &= GetStats(); //Immediate stat benefit
        m[0].connections.RegenerateStats.AddListener(0, RegenerateStats); //Hook up for next regen

        //Clone effects, so they can reapply
        clonedEffects.Clear();
        foreach (Effect e in addedEffects)
        {
            clonedEffects.Add(e.Instantiate());
        }

        m[0].AddEffect(clonedEffects.ToArray()); //Immediate status effect add
    }



    public void OnUnequip()
    {
        equippedTo[0].currentStats ^= addedStats;

        //Disconnect all old effects
        foreach (Effect e in clonedEffects)
        {
            e.Disconnect();
        }
        equippedTo[0].connections.RegenerateStats.RemoveListener(RegenerateStats);
        isEquipped = false;
        equippedTo = RogueHandle<Monster>.Default;
    }

    public void RegenerateStats(ref Stats block)
    {
        block &= GetStats();
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
