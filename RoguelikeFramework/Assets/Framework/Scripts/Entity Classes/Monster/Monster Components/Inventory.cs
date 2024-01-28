using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class ItemStack
{
    public int id;
    public ItemType type;
    public int count;
    public List<Item> held;
    [HideInInspector] public int position;
    [HideInInspector] public int lastUpdated; //Used to find what items should float to the top

    
    //TODO: Articles for special weapons (a +5 sword vs the +5 sword of rebirth)
    public string GetName(bool withArticles = false)
    {
        if (count == 1)
        {
            if (withArticles)
            {
                return "a " + held[0].GetName();
            }
            else
            {
                return held[0].GetName();
            }
        }
        else
        {
            return $"{count} {held[0].GetName()}";
        }
    }
}

public class Inventory : MonoBehaviour
{
    //Regular variables
    public int capacity;
    [HideInInspector]
    public int available;

    public event ActionRef<ItemStack> itemsAdded;
    public event ActionRef<ItemStack> itemsRemoved;

    public List<Item> baseItems;

    Transform holder;

    private bool setup = false;

    private int updateCounterVal = 0;
    private int updateCounter
    {
        get
        {
            updateCounterVal++;
            return updateCounterVal;
        }
    }

    //Generated measure of how many items we're holding, useful for ground pickup
    public int Count
    {
        get { return capacity - available; }
    }

    [SerializeField]
    [HideInInspector]
    private ItemStack[] Items; //Wish this wasn't hidden, but it unfortunately must be. Unity serialization removes the nulls
    public ItemStack[] items
    {
        get { return Items; }
    }

    public IEnumerable<Item> AllHeld()
    {
        if (capacity - available == 0) yield break;
        for (int i = 0; i < capacity; i++)
        {
            if (Items[i] != null)
            {
                yield return Items[i].held[0];
            }
        }
    }

    public List<int> AllIndices()
    {
        return items.Where(x => x != null).Select(x => x.position).ToList();
    }
    

    public ItemStack this[int index]
    {
        get { return Items[index];  }
    }

    private Monster _monster;
    private Monster monster
    {
        get 
        { 
            if (!_monster)
            {
                _monster = GetComponent<Monster>();
            }
            return _monster;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!setup) Setup();
    }

    public void Setup()
    {
        if (setup) return;
        //Set up inventory
        available = capacity;
        Items = new ItemStack[capacity];

        RogueTile tile = GetComponent<RogueTile>();
        Monster monster = GetComponent<Monster>();
        
        if (tile)
        {
            holder = tile.transform.parent.parent.parent.GetComponent<Map>().itemContainer;
        }

        if (monster)
        {
            GameObject hold = new GameObject("Items");
            hold.transform.parent = transform;
            holder = hold.transform;
        }

        Debug.Assert(holder != null, "Inventory wasn't attached to monster or tile? Make sure to update inventory logic if this is intentional.", this);

        foreach (Item item in baseItems)
        {
            Item i = item.Instantiate();
            i.Setup();
            Add(i);
        }

        //TODO: REWORK THIS
        this.enabled = false; //This is really, really dumb. I know. Gives us back 15 fps, though
        setup = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int Add(Item item)
    {
        if (item == null) return -1;

        //Create a new stack, and push it through the stack system. Keeps everything
        //in one workflow, so there isn't any inconsistency.
        ItemStack newStack = new ItemStack();
        newStack.id = item.ID;
        newStack.count = 1;
        newStack.type = item.type;
        newStack.held = new List<Item>();
        newStack.held.Add(item);

        return Add(newStack);
    }

    public int AddStackNoMatch(ItemStack newStack)
    {
        if (available == 0)
        {
            Debug.Log("Console: Can't add item to stack, no space"); //TODO: Add proper logging here
        }

        //Add item in
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] == null)
            {
                itemsAdded?.Invoke(ref newStack);

                //Empty slot!
                newStack.position = i; //Looks stupid, I know. Helps with sorting and numbering later!
                newStack.lastUpdated = updateCounter;
                Items[i] = newStack;
                available--;

                //Move added items into our holder!
                foreach (Item item in newStack.held)
                {
                    item.transform.parent = holder;
                }

                return i;
            }
        }

        Debug.LogError("Could not pick up item! No space was found, but space should have existed. (Available was not 0)", this);
        return -1;
    }

    public void RemoveLastItemFromStack(int index)
    {
        ItemStack stack = items[index];
        if (stack != null)
        {
            int numItems = stack.held.Count;
            Destroy(stack.held[numItems - 1].gameObject);
            if (numItems <= 1)
            {
                items[index] = null;
            }
            else
            {
                stack.held.RemoveAt(numItems - 1);
                stack.count--;
                stack.lastUpdated = updateCounter;
            }
        }
    }

    public int Add(ItemStack stack)
    {
        if (stack == null)
        {
            print("Stack add cancelled early.");
            return -1;
        }

        //Look for a match
        if (stack.held[0].stackable)
        {
            for (int i = 0; i < capacity; i++)
            {
                if (Items[i] == null) continue;
                if (Items[i].id == stack.id)
                {
                    itemsAdded?.Invoke(ref stack);
                    Items[i].count += stack.count;
                    for (int j = 0; j < stack.count; j++)
                    {
                        Items[i].held.Add(stack.held[j]);
                        stack.held[j].transform.parent = holder;
                    }
                    Items[i].lastUpdated = updateCounter;
                    return i;
                }
            }
        }

        //No match found, add it into the first available slot
        return AddStackNoMatch(stack);
    }

    //VERY EXPENSIVE: Sorts items up to the top, try not to use this a lot
    public void Collapse()
    {
        Array.Sort<ItemStack>(Items, Compare);
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] != null)
            {
                Items[i].position = i;
            }
        }
    }

    public static int Compare(ItemStack one, ItemStack two)
    {
        if (one == null && two == null)
        {
            return 0;
        }
        else if (one == null)
        {
            return 1;
        }
        else if (two == null)
        {
            return -1;
        }
        else if (one.type == two.type)
        {
            return one.lastUpdated.CompareTo(two.lastUpdated);
        }
        else
        {
            return one.type.CompareTo(two.type);
        }
    }

    public static int ComparePlayer(ItemStack one, ItemStack two)
    {
        if (one == null && two == null)
        {
            return 0;
        }
        else if (one == null)
        {
            return 1;
        }
        else if (two == null)
        {
            return -1;
        }
        else if (one.type == two.type)
        {
            return one.position.CompareTo(two.position);
        }
        else
        {
            return one.type.CompareTo(two.type);
        }
    }

    public Inventory GetFloor()
    {
        if (monster == null)
        {
            return this;
        }
        return Map.current.GetTile(monster.location).GetComponent<Inventory>();
    }

    //Get stack index of an item, -1 if not found
    public int GetIndexOf(Item item)
    {
        for (int i = 0; i < capacity; i++)
        {
            if (items[i] != null)
            {
                //Check, with short circuit for a little speedup
                if (items[i].id == item.ID && items[i].held.Contains(item))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    //Convenience function
    public void Drop(int index)
    {
        MonsterToFloor(index);
    }

    public void PickUp(int index)
    {
        FloorToMonster(index);
    }

    public void RemoveAt(int index)
    {
        if (Items[index] != null)
        {
            ItemStack toRemove = Items[index];
            available++;
            Items[index] = null;

            itemsRemoved?.Invoke(ref toRemove);
        }
        else
        {
            Debug.LogError("Tried to remove at a null location, so op was cancelled", this);
        }
    }

    public void PickUpAll()
    {
        RogueTile tile = Map.current.GetTile(monster.location);
        for (int i = capacity - 1; i >= 0; i--)
        {
            FloorToMonster(i);
        }
    }

    public void FloorToMonster(int index)
    {
        Inventory onFloor = Map.current.GetTile(monster.location).inventory;

        ItemStack stack = onFloor[index];
        if (stack == null) return; //Quick cutout
        foreach (Item i in stack.held)
        {
            i.Pickup(monster);
        }
        Add(stack);
        onFloor.RemoveAt(index);
    }

    public void MonsterToFloor(int index)
    {
        Inventory onFloor = Map.current.GetTile(monster.location).inventory;

        ItemStack stack = Items[index];

        if (stack == null) return; //Quick cutout

        EquipableItem equip = stack.held[0].equipable;

        if (equip && equip.isEquipped)
        {
            //TODO: Figure out if we should abort the drop, or just unequip

            //For now, just unequip it
            equip.Unequip();
        }

        foreach (Item i in stack.held)
        {
            i.Drop();
            i.SetLocation(monster.location);
        }

        onFloor.Add(stack);
        RemoveAt(index);
    }

    public void ClearItems()
    {
        Items = new ItemStack[capacity];
        available = capacity;
    }
}
