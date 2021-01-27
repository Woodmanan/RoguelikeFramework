using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class InventoryScreen : RogueUIPanel
{
    [SerializeField] public Transform holdingPanel;
    [SerializeField] public GameObject itemPanelPrefab;
    [SerializeField] private TextMeshProUGUI title;


    public Inventory examinedInventory;
    public ItemAction queuedAction;

    public bool[] selected;
    public List<ItemPanel> displayed = new List<ItemPanel>();



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
                title.text = "Inventory";
                break;
            case ItemAction.DROP:
                toDisplay = available;
                selected = new bool[examinedInventory.capacity];
                title.text = "Drop which items?";
                break;
            case ItemAction.PICK_UP:
                toDisplay = available;
                selected = new bool[examinedInventory.capacity];
                title.text = "Pick up which items?";
                break;
            default:
                Debug.LogError($"Inventory screen is not set up to handle {queuedAction} types. Yell at Woody about this.");
                break;
        }

        print($"How many items to display: {toDisplay.Count}");

        for (int i = 0; i < toDisplay.Count; i++)
        {
            GameObject instance = Instantiate(itemPanelPrefab);
            ItemPanel current = instance.GetComponent<ItemPanel>();
            current.Setup(this, toDisplay[i].position);
            current.GenerateItemDescription();
            displayed.Add(current);

            instance.transform.parent = holdingPanel;
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

            case ItemAction.PICK_UP:
            case ItemAction.DROP:
                if (action == PlayerAction.ACCEPT)
                {
                    //Move the selected items!
                    for (int i = 0; i < selected.Length; i++)
                    {
                        if (selected[i])
                        {
                            switch (queuedAction)
                            {
                                case ItemAction.DROP:
                                    examinedInventory.Drop(i);
                                    break;
                                case ItemAction.PICK_UP:
                                    Player.player.inventory.PickUp(i);
                                    break;
                                default:
                                    Debug.LogError("How did you even do this!? The case inside of a nested switch does not match the original case.");
                                    break;
                            }
                        }
                    }

                    //Get us back to the main screen!
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
