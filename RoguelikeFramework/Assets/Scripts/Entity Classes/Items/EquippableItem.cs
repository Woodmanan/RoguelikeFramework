using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippableItem : MonoBehaviour
{
    [Header("Equippable Attributes")];
    public EquipSlotType primarySlot;
    public List<EquipSlotType> secondarySlots;
    public StatBlock addedStats;
    public List<Effect> addedEffects;
    public bool isEquipped = false;

    Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnFirstEquip()
    {

    }
}
