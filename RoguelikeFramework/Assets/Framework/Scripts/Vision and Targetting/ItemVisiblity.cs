using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class ItemVisiblity : MonoBehaviour
{
    [SerializeField] private Item visible;
    private RogueTile tile;
    Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Setup()
    {
        inventory = GetComponent<Inventory>();
        tile = GetComponent<RogueTile>();
        inventory.itemsAdded += ItemIsAdded;
        inventory.itemsRemoved += ItemIsRemoved;
        this.enabled = false;
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
            visible.EnableSprite();
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

        stack.held[0].SetLocation(tile.location);

        //Set new visible
        visible = stack.held[0];
        if (tile.isVisible)
        {
            visible.EnableSprite();
        }
    }

    public void ItemIsRemoved(ref ItemStack stack)
    {
        Debug.Log("Item removal called.");
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
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
