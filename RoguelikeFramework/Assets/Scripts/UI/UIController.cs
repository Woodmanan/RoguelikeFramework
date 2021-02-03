using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIController : MonoBehaviour
{
    [SerializeField] private InventoryScreen inventory;
    [SerializeField] private EquipmentScreen equipment;
    [SerializeField] private ItemInspectionPanel inspection;
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
        Tuple<PlayerAction, string> pair = InputTracking.PopNextPair();
        PlayerAction action = pair.Item1;
        string inputString = pair.Item2;
        switch (action)
        {
            case PlayerAction.ESCAPE_SCREEN: //Breaks free early, so panels themselves don't have to all try to handle this input.
                RogueUIPanel.ExitTopLevel();
                break;
            default:
                RogueUIPanel.inFocus.HandleInput(action, inputString);
                break;
        }
    }


    public void OpenInventoryInspect()
    {
        inventory.Setup(Player.player.inventory, ItemAction.INSPECT);
        inventory.Activate();
    }

    public void OpenInventoryDrop()
    {
        inventory.Setup(Player.player.inventory, ItemAction.DROP);
        inventory.Activate();
    }

    public void OpenInventoryPickup()
    {
        CustomTile tile = Map.singleton.GetTile(Player.player.location);
        inventory.Setup(tile.inventory, ItemAction.PICK_UP);
        inventory.Activate();
    }

    public void OpenInventoryEquip(int index)
    {
        inventory.Setup(Player.player.inventory, ItemAction.EQUIP, index);
        inventory.Activate();
    }

    public void OpenEquipmentInspect()
    {
        equipment.Setup(Player.player.equipment, ItemAction.INSPECT, null);
        equipment.Activate();
    }

    public void OpenEquipmentEquip(int index) //Yeah, the naming scheme made this one kind of stupid
    {
        Player player = Player.player; //Wow, this whole function is full of them
        equipment.Setup(player.equipment, ItemAction.EQUIP, player.inventory[index]);
        equipment.Activate();
    }

    public void OpenEquipmentEquip(ItemStack stack)
    {
        equipment.Setup(Player.player.equipment, ItemAction.EQUIP, stack);
        equipment.Activate();
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

    public void PassInput(PlayerAction action)
    {
        if (action == PlayerAction.ESCAPE_SCREEN)
        {
            RogueUIPanel.ExitTopLevel();
        }
    }
}
