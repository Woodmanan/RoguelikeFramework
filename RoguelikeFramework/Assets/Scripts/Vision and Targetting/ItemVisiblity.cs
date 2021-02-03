using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class ItemVisiblity : MonoBehaviour
{
    private Item visible;
    Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
        inventory = GetComponent<Inventory>();
        inventory.itemsAdded += ItemIsAdded;
        inventory.itemsRemoved += ItemIsRemoved;
        if (inventory.startingItems.Count > 0)
        {
            visible = inventory.startingItems[0]; //TODO: Be smarter than just grabbing the first item (ie, most rare / valuable)
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RebuildVisiblity(bool isVisible, bool isHidden)
    {
        if (visible == null) return; //Cancel early for null, who cares
        if (isHidden)
        {
            visible.DisableSprite();
        }
        else
        {
            if (isVisible)
            {
                visible.SetFullColor();
            }
            else
            {
                visible.SetGrayscale();
            }
        }
    }
   
    public void ItemIsAdded(ref ItemStack stack)
    {
        visible?.DisableSprite();

        //Turn off all items - fixes problems with stacks rendering badly
        foreach (Item i in stack.held)
        {
            i.DisableSprite();
        }

        //Set new visible
        visible = stack.held[0];
        visible.EnableSprite();
    }

    public void ItemIsRemoved(ref ItemStack stack)
    {
        #if UNITY_EDITOR
        if (visible == null)
        {
            Debug.LogError("Item removed from inventory was null?");
            return;
        }
        #endif

        if (stack.held[0] == visible)
        {
            visible.DisableSprite();
            ItemStack newest = null;
            for (int i = 0; i < inventory.capacity; i++)
            {
                if (newest == null)
                {
                    newest = inventory[i];
                    continue;
                }

                //Can assume lowest is not null now
                if (inventory[i] != null)
                {
                    if (newest.lastUpdated < inventory[i].lastUpdated)
                    {
                        newest = inventory[i];
                    }
                }
            }

            if (newest != null)
            {
                visible = newest.held[0];
                visible.EnableSprite();
            }
            else
            {
                visible = null;
            }

        }
    }
}
