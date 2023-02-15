using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Class", menuName = "New Class", order = 2)]
public class Class : ScriptableObject
{
    public List<Item> items;

    //Should be replaced by a book at some point!
    public List<Ability> abilities;

    [SerializeReference]
    public List<Effect> effects;

    public void Apply(Monster m, bool giveItems = true)
    {
        //Attempt setup, in case the monster hasn't been configured yet.
        m.Setup();

        if (giveItems)
        {
            //Get the items attached
            foreach (Item item in items)
            {
                EquipableItem equip = item.GetComponent<EquipableItem>();
                if (equip)
                {
                    //Item is equipable, so try to equip it. Otherwise, dumpt it.
                    int equipSlot = m.equipment.CanSafelyEquip(equip);
                    if (equipSlot >= 0)
                    {
                        Item i = item.Instantiate();
                        i.Setup();
                        int itemSlot = m.inventory.Add(i);
                        m.equipment.Equip(itemSlot, equipSlot);
                    }
                    else
                    {
                        Debug.Log($"Could not safely equip {item.GetName()}, storing it in inventory");
                    }
                }
                else
                {
                    //Item is a consumable / usable thing, so let the monster keep it!
                    m.inventory.Add(item.Instantiate());
                }
            }
        }

        //Atttach abilities
        foreach (Ability ability in abilities)
        {
            m.abilities.AddAbilityInstantiate(ability.Instantiate());
        }

        m.AddEffectInstantiate(effects.ToArray());
    }
}
