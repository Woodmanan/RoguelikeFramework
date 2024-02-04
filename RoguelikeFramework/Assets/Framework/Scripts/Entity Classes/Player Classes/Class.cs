using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Class", menuName = "New Class", order = 2)]
public class Class : ScriptableObject
{
    public string friendlyName;

    public DamageType classDamage;

    public List<Item> items;

    //Should be replaced by a book at some point!
    public List<Ability> abilities;

    [SerializeReference]
    public List<Effect> effects;

    public void Apply(RogueHandle<Monster> m, bool giveItems = true)
    {
        //Attempt setup, in case the monster hasn't been configured yet.
        m.value.Setup();

        if (giveItems)
        {
            //Get the items attached
            foreach (Item item in items)
            {
                EquipableItem equip = item.GetComponent<EquipableItem>();
                if (equip)
                {
                    //Item is equipable, so try to equip it. Otherwise, dumpt it.
                    int equipSlot = m.value.equipment.CanSafelyEquip(equip);
                    if (equipSlot >= 0)
                    {
                        Item i = item.Instantiate();
                        i.Setup();
                        int itemSlot = m.value.inventory.Add(i);
                        m.value.equipment.Equip(itemSlot, equipSlot);
                    }
                    else
                    {
                        Debug.Log($"Could not safely equip {item.GetName()}, storing it in inventory");
                    }
                }
                else
                {
                    //Item is a consumable / usable thing, so let the monster keep it!
                    m.value.inventory.Add(item.Instantiate());
                }
            }
        }

        //Atttach abilities
        foreach (Ability ability in abilities)
        {
            m.value.abilities.AddAbilityInstantiate(ability.Instantiate());
        }

        m.value.AddEffectInstantiate(effects.ToArray());
    }

    //Assumes both classes are instantiated!
    public virtual void CombineWith(Class other)
    {
        other.items.AddRange(items);
        other.abilities.AddRange(abilities);
        other.effects.AddRange(effects);
        other.classDamage |= classDamage;
    }
}
