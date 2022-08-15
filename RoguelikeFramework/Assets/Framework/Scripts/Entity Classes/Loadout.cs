using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Loadout", menuName = "New Loadout", order = 2)]
public class Loadout : ScriptableObject
{
    public List<Item> items;
    public int maxItems;

    public List<Ability> abilities;
    public int maxAbilties;

    public int minDepth;
    public int maxDepth;


    public void Apply(Monster m)
    {
        //Ensure that I'm the ONLY one allowed to get applied.
        m.loadout = null;
        //Attempt setup, in case the monster hasn't been configured yet.
        m.Setup();

        //Get the items attached
        items = items.OrderBy(x => UnityEngine.Random.Range(int.MinValue, int.MaxValue)).ToList();
        int numAttached = 0;
        foreach (Item item in items)
        {
            if (numAttached >= maxItems) break;

            EquipableItem equip = item.GetComponent<EquipableItem>();
            if (equip)
            {
                //Item is equipable, so try to equip it. Otherwise, dumpt it.
                int equipSlot = m.equipment.CanSafelyEquip(equip);
                if (equipSlot >= 0)
                {
                    Item i = item.Instantiate();
                    int itemSlot = m.inventory.Add(i);
                    m.equipment.Equip(itemSlot, equipSlot);
                    numAttached++;
                }
            }
            else
            {
                //Item is a consumable / usable thing, so let the monster keep it!
                m.inventory.Add(item.Instantiate());
                numAttached++;
            }
            
        }

        numAttached = 0;
        //Do the abilities
        abilities = abilities.OrderBy(x => UnityEngine.Random.Range(int.MinValue, int.MaxValue)).ToList();
        foreach (Ability ability in abilities)
        {
            if (numAttached >= maxAbilties) break;

            m.abilities.AddAbility(ability.Instantiate());
            numAttached++;
        }
    }
}
