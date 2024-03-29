﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIController : MonoBehaviour
{
    [SerializeField] private InventoryScreen inventory;
    [SerializeField] private EquipmentScreen equipment;
    [SerializeField] private AbilitiesScreen abilities;
    [SerializeField] private ItemInspectionPanel inspection;
    [SerializeField] private ConfirmationPanel confirm;
    [SerializeField] private ClassPanel classPanel;
    [SerializeField] private PersonalAttributePanel attributePanel;
    [SerializeField] private PausePanel pausePanel;
    [SerializeField] private CheatsPanel cheats;
    public static bool WindowsOpen
    {
        get { return RogueUIPanel.WindowsOpen; }
    }

    private static UIController Singleton;
    public static UIController singleton
    {
        get
        {
            if (!Singleton)
            {
                Singleton = GameObject.FindGameObjectWithTag("UIControl").GetComponent<UIController>();
            }
            return Singleton;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (WindowsOpen)
        {
            HandleInput();
        }
    }

    public void HandleInput()
    {
        (PlayerAction action, string inputString) = InputTracking.PopNextPair();
        switch (action)
        {
            case PlayerAction.ESCAPE_SCREEN: //Breaks free early, so panels themselves don't have to all try to handle this input.
                RogueUIPanel.AttemptExitTopLevel();
                break;
            default:
                RogueUIPanel.inFocus.HandleInput(action, inputString);
                break;
        }
    }

    public void OpenInventoryInspect()
    {
        inventory.Setup(Player.player[0].inventory, ItemAction.INSPECT);
        inventory.Activate();
    }

    public void OpenInventoryDrop()
    {
        inventory.Setup(Player.player[0].inventory, ItemAction.DROP);
        inventory.Activate();
    }

    public void OpenInventoryPickup()
    {
        RogueTile tile = Map.current.GetTile(Player.player[0].location);
        inventory.Setup(tile.inventory, ItemAction.PICK_UP);
        inventory.Activate();
    }

    public void OpenInventoryEquip(int index)
    {
        inventory.Setup(Player.player[0].inventory, ItemAction.EQUIP, index);
        inventory.Activate();
    }

    public void OpenEquipmentInspect()
    {
        equipment.Setup(Player.player[0].equipment, ItemAction.INSPECT, null);
        equipment.Activate();
    }

    public void OpenEquipmentEquip(int index) //Yeah, the naming scheme made this one kind of stupid
    {
        Monster player = Player.player; //Wow, this whole function is full of them
        equipment.Setup(player.equipment, ItemAction.EQUIP, player.inventory[index]);
        equipment.Activate();
    }

    public void OpenEquipmentEquip(ItemStack stack)
    {
        equipment.Setup(Player.player[0].equipment, ItemAction.EQUIP, stack);
        equipment.Activate();
    }

    public void OpenEquipmentUnequip()
    {
        equipment.Setup(Player.player[0].equipment, ItemAction.UNEQUIP, null);
        equipment.Activate();
    }

    public void OpenInventoryActivate()
    {
        inventory.Setup(Player.player[0].inventory, ItemAction.APPLY);
        inventory.Activate();
    }

    public void OpenInventorySelect(Predicate<ItemStack> filter, Action<List<int>> OnFound, int numItems = 52)
    {
        inventory.Setup(Player.player[0].inventory, ItemAction.SELECT, filter, OnFound, numItems);
        inventory.Activate();
    }

    public void OpenAbilities()
    {
        abilities.Setup(Player.player[0].abilities);
        abilities.Activate();
    }

    public void OpenClassPanel(Class classToGive)
    {
        classPanel.Setup(classToGive);
        classPanel.Activate();
    }

    public void OpenAttributePanel(Effect attribute, Effect backup)
    {
        attributePanel.Setup(attribute, backup);
        attributePanel.Activate();
    }

    public void OpenItemInspect(Inventory inventory, int index)
    {
        //Quick double check
        if (index >= inventory.capacity || index < 0)
        {
            Debug.LogError($"Can't open item with index {index}", inventory.gameObject);
            return;
        }

        //Actually open the item
        ItemStack stack = inventory.items[index];
        inspection.Setup(stack);
        inspection.Activate();
    }

    public void OpenCheats()
    {
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        cheats.Activate();
    #else
        Debug.Log("Console: Hey! You found a way to enable cheats. Why are you doing that?");
        Debug.Log("Console: If you like the game and want to improve it, you can message the developer with the details of your exploit.");
        Debug.Log("Console: If it's a new exploit, you'll get your name in the credits! (And have helped make the game better)");
    #endif
    }

    public void OpenPause()
    {
        pausePanel.Activate();
    }

    public void PassInput(PlayerAction action)
    {  
        if (action == PlayerAction.ESCAPE_SCREEN)
        {
            RogueUIPanel.ExitTopLevel();
        }
    }

    public void OpenConfirmation(String msg, BoolDelegate funcToCall)
    {
        confirm.Setup(msg, funcToCall);
        confirm.Activate();
    }
}
