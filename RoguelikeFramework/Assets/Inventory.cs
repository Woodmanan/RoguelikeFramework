﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ItemStack
{
    public int id;
    public int count;
    public List<Item> held;
    [HideInInspector] public int position;
    [HideInInspector] public int lastUpdated; //Used to find what items should float to the top

    public string GetName()
    {
        if (count == 1)
        {
            return held[0].GetName();
        }
        else
        {
            return $"{count} {held[0].GetPlural()}";
        }
    }
}

public class Inventory : MonoBehaviour
{
    //Regular variables
    public int capacity;
    public int available;

    public event ActionRef<ItemStack> itemsAdded;
    public event ActionRef<ItemStack> itemsRemoved;
    private int updateCounter = 0;

    //Generated measure of how many items we're holding, useful for ground pickup
    public int count
    {
        get { return capacity - available; }
    }

    private ItemStack[] Items; //Wish this wasn't hidden, but it unfortunately must be. Unity serialization removes the nulls
    public ItemStack[] items
    {
        get { return Items; }
    }

    public List<Item> startingItems; //Easier to manage than a stack
    

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
        //Set up inventory
        available = capacity;
        Items = new ItemStack[capacity];

        //Add in starting items
        foreach (Item i in startingItems)
        {
            Add(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Add(Item item)
    {
        if (item == null) return;

        //Create a new stack, and push it through the stack system. Keeps everything
        //in one workflow, so there isn't any inconsistency.
        ItemStack newStack = new ItemStack();
        newStack.id = item.id;
        newStack.count = 1;
        newStack.held = new List<Item>();
        newStack.held.Add(item);

        Add(newStack);
    }

    public void AddStackNoMatch(ItemStack newStack)
    {
        if (available == 0)
        {
            Debug.Log("Can't add item to stack, no space"); //TODO: Add proper logging here
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
                return;
            }
        }

        Debug.LogError("Could not pick up item! No space was found, but space should have existed. (Available was not 0)", this);
    }

    public void Add(ItemStack stack)
    {
        updateCounter++;

        if (stack == null)
        {
            print("Stack add cancelled early.");
            return;
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
                    print("Added to another stack!");
                    Items[i].count += stack.count;
                    for (int j = 0; j < stack.count; j++)
                    {
                        Items[i].held.Add(stack.held[j]);
                    }
                    Items[i].lastUpdated = updateCounter;
                    return;
                }
            }
        }

        //No match found, add it into the first available slot
        AddStackNoMatch(stack);
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
        CustomTile tile = Map.singleton.GetTile(monster.location);
        for (int i = capacity - 1; i >= 0; i--)
        {
            FloorToMonster(i);
        }
    }

    public void FloorToMonster(int index)
    {
        Inventory onFloor = Map.singleton.GetTile(monster.location).inventory;

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
        Inventory onFloor = Map.singleton.GetTile(monster.location).inventory;

        ItemStack stack = Items[index];

        if (stack == null) return; //Quick cutout

        foreach (Item i in stack.held)
        {
            i.Drop();
            i.SetLocation(monster.location);
        }

        onFloor.Add(stack);
        RemoveAt(index);
    }
}
