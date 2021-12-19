using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class InventoryScreen : RogueUIPanel
{
    [SerializeField] public Transform holdingPanel;
    [SerializeField] public GameObject itemPanelPrefab;
    [SerializeField] public GameObject itemHeaderPrefab;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI failureMessage;


    public Inventory examinedInventory;
    public ItemAction queuedAction;

    public bool[] selected;
    public List<ItemPanel> displayed = new List<ItemPanel>();

    private int queuedEquipmentIndex;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(Inventory inventoryToExamine, ItemAction action)
    {
        examinedInventory = inventoryToExamine;
        queuedAction = action;
    }

    public void Setup(Inventory inventoryToExamine, ItemAction action, int equipIndex)
    {
        queuedEquipmentIndex = equipIndex;
        Setup(inventoryToExamine, action);
    }

    public override void OnActivation()
    {
        //Clear old items
        displayed.Clear();
        for (int i = holdingPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(holdingPanel.GetChild(i).gameObject);
        }

        //Clear out empty items
        List<ItemStack> available = examinedInventory.items.ToList().FindAll(x => x != null);
        List<ItemStack> toDisplay = new List<ItemStack>();

        //Set up items to show, based on parameters
        switch (queuedAction)
        {
            case ItemAction.INSPECT:
                toDisplay = available;
                toDisplay.Sort(Inventory.ComparePlayer);
                title.text = "Inventory";
                break;
            case ItemAction.DROP:
                toDisplay = available;
                toDisplay.Sort(Inventory.ComparePlayer);
                selected = new bool[examinedInventory.capacity];
                title.text = "Drop which items?";
                break;
            case ItemAction.EQUIP:
                //TODO: Do this in a way that isn't dumb, and probably has item slots remember if they're equipabble
                //Filter out toDisplay into items that can be equipped in this slot
                EquipmentSlot slot = Player.player.equipment.equipmentSlots[queuedEquipmentIndex];
                title.text = $"Equip what to {slot.slotName}?";
                toDisplay = available.FindAll(x => x.held[0].CanEquip); //Pretty cheap
                toDisplay = toDisplay.FindAll(x => slot.type.Contains(x.held[0].equipable.primarySlot)); //Pretttttty expensive
                break;
            case ItemAction.PICK_UP:
                toDisplay = available;
                selected = new bool[examinedInventory.capacity];
                title.text = "Pick up which items?";
                break;
            case ItemAction.APPLY:
                title.text = "Apply which item?";
                toDisplay = available.FindAll(x => x.held[0].CanApply);
                break;
            default:
                Debug.LogError($"Inventory screen is not set up to handle {queuedAction} types. Yell at Woody about this.");
                break;
        }

        print($"How many items to display: {toDisplay.Count}");
        
        if (toDisplay.Count == 0)
        {
            failureMessage.gameObject.SetActive(true);
        }
        else
        {
            failureMessage.gameObject.SetActive(false);
        }

        //Sort the list into item types
        ItemType currentType = ItemType.NONE;

        for (int i = 0; i < toDisplay.Count; i++)
        {
            GameObject instance = Instantiate(itemPanelPrefab);
            ItemPanel current = instance.GetComponent<ItemPanel>();
            ItemStack stack = toDisplay[i];

            //Set up headers
            if (stack.type != currentType)
            {
                //Create a header!
                GameObject header = Instantiate(itemHeaderPrefab);
                header.GetComponent<ItemHeader>().Setup(stack.type);
                header.transform.SetParent(holdingPanel);
                currentType = stack.type;
            }

            //Set up item readout
            current.Setup(this, toDisplay[i].position);
            current.GenerateItemDescription();
            displayed.Add(current);

            instance.transform.SetParent(holdingPanel);
        }

    }

    public override void HandleInput(PlayerAction action, string inputString)
    {
        switch (queuedAction)
        {
            case ItemAction.INSPECT:
                //Break down input into item types
                foreach (char c in inputString.Where(c => char.IsLetter(c)))
                {
                    int index = Conversions.NumberingToInt(c);
                    if (index < examinedInventory.capacity && examinedInventory[index] != null)
                    {
                        //Display an item!
                        UIController.singleton.OpenItemInspect(examinedInventory, index);
                        break;
                    }
                }
                break;
            case ItemAction.EQUIP:
                //Break down input into item types
                foreach (char c in inputString.Where(c => char.IsLetter(c)))
                {
                    int index = Conversions.NumberingToInt(c);
                    if (index < examinedInventory.capacity && examinedInventory[index] != null && index >= 0)
                    {
                        //Equip an item!
                        Player.player.SetAction(new EquipAction(index, queuedEquipmentIndex));
                        ExitAllWindows();
                        break;
                    }
                }
                break;


            case ItemAction.PICK_UP:
            case ItemAction.DROP:
                if (action == PlayerAction.ACCEPT)
                {
                    //Splitting this up because of the new action system.
                    //TODO: Refactor this bit better

                    List<int> indices = new List<int>();
                    for (int i = 0; i < selected.Length; i++)
                    {
                        if (selected[i]) { indices.Add(i); }
                    }
                    GameAction act;
                    if (queuedAction == ItemAction.DROP)
                    {
                        act = new DropAction(indices);
                    }
                    else
                    {
                        act = new PickupAction(indices);
                    }
                    Player.player.SetAction(act);
                    ExitAllWindows();
                }
                else
                {
                    //Flip bits for selected items
                    foreach (char c in inputString.Where(c => char.IsLetter(c)))
                    {
                        //Attempt to flip the bit
                        int index = Conversions.NumberingToInt(c);
                        selected[index] = !selected[index];
                        for (int i = 0; i < displayed.Count; i++)
                        {
                            ItemPanel current = displayed[i];
                            if (current.index == index)
                            {
                                current.Select();
                                break;
                            }
                        }
                    }
                }
                break;

            case ItemAction.APPLY:
                foreach (char c in inputString.Where(c => char.IsLetter(c)))
                {
                    int index = Conversions.NumberingToInt(c);
                    if (index < examinedInventory.capacity && examinedInventory[index] != null && index >= 0)
                    {
                        //Equip an item!
                        Player.player.inventory.Apply(index);
                        ExitAllWindows();
                        break;
                    }
                }
                break;
        }
    }

    //Handles a given item being click
    public void Click(int index)
    {
        print($"Handling a click to {Conversions.IntToNumbering(index)}");
        print($"UI state is {queuedAction}");
        switch (queuedAction)
        {
            case ItemAction.DROP:
            case ItemAction.PICK_UP:
                selected[index] = !selected[index];
                for (int i = 0; i < displayed.Count; i++)
                {
                    ItemPanel current = displayed[i];
                    if (current.index == index)
                    {
                        current.Select();
                        break;
                    }
                }
                break;
            case ItemAction.INSPECT:
                print("OPENEING UI!");
                UIController.singleton.OpenItemInspect(examinedInventory, index);
                break;
            case ItemAction.EQUIP:
                //TODO: Do something something different if item is already equipped
                print($"Attaching item {index} to slot {queuedEquipmentIndex}");
                Player.player.SetAction(new EquipAction(index, queuedEquipmentIndex));
                ExitAllWindows(); //After equiping, just exit
                break;
            case ItemAction.APPLY:
                Player.player.inventory.Apply(index);
                ExitAllWindows();
                break;
        }
    }

    public override void OnDeactivation()
    {

    }

    public override void OnFocus()
    {

    }

    public override void OnDefocus()
    {

    }
}
