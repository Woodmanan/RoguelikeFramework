using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInspectionPanel : RogueUIPanel
{
    //Don't uncomment these variables! These are already declared in the base class,
    //and are listed here so you know they exist.

    //bool inFocus; - Tells you if this is the window that is currently focused

    private ItemStack inspecting; // The item that we are inspecting
    [SerializeField] private TextMeshProUGUI nameBox;
    [SerializeField] private TextMeshProUGUI descriptionBox;
    [SerializeField] private TextMeshProUGUI attributesBox;
    [SerializeField] private Image image;
    //[SerializeField] private TextMeshProUGUI quote TODO: Add quotes?


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(ItemStack toInspect)
    {
        inspecting = toInspect;

        Item represt = toInspect.held[0];
    }

    /*
     * One of the more important functions here. When in focus, this will be called
     * every frame with the stored input from InputTracking
     */
    public override void HandleInput(PlayerAction action, string inputString)
    {
        switch (action)
        {
            case PlayerAction.DROP_ITEMS:
                Player.player.inventory.Drop(inspecting.position);
                ExitAllWindows();
                break;
            case PlayerAction.EQUIP:
                EquipableItem toEquip = inspecting.held[0].equipable;
                if (toEquip != null && !toEquip.isEquipped)
                {
                    UIController.singleton.OpenEquipmentEquip(inspecting);
                }
                else
                {
                    //TODO: Console error of some sort?
                }
                break;
            case PlayerAction.UNEQUIP:
                EquipableItem equip = inspecting.held[0].equipable;
                if (equip != null && equip.isEquipped)
                {
                    Player.player.equipment.UnequipItem(inspecting.position); //Faster method, doesn't need a search
                    ExitAllWindows();
                }
                break;
        }
    }

    /* Called every time this panel is activated by the controller */
    public override void OnActivation()
    {
        Debug.Log($"Is inspecting null? {inspecting.GetName()}");
        nameBox.text = $"{Conversions.IntToNumbering(inspecting.position)} - {inspecting.GetName()}";
        SpriteRenderer render = inspecting.held[0].GetComponent<SpriteRenderer>();
        image.sprite = render.sprite;
        image.color = render.color;

        DetermineDescription(inspecting.held[0]);
        DetermineAttributes(inspecting.held[0]);
    }

    public void DetermineDescription(Item item)
    {
        string text = "";
        text = $"This is a clone of item {item.ID}, named {item.GetNameClean()}. It will have a description some day, but doesn't quite yet. For now this dynamic entry is proof that that could happen. Please be nice to it despite it's many flaws.";
        descriptionBox.text = text;
    }

    public void DetermineAttributes(Item item)
    {
        string text = "";

        if (item.CanMelee)
        {
            string meleeText = "";
            WeaponBlock block = item.melee.primary;
            meleeText += $"As a primary weapon, this weapon has <color=#2222CC>{block.accuracy} accuracy<#000000> and <#22CC22>{block.piercing} piercing<#000000> with a default {block.chanceToHit}% chance to hit.\n";
            meleeText += $"On a hit, it deals ";
            bool first = true;
            foreach (DamagePairing pair in block.damage)
            {
                meleeText += $"{(first ? "" : " + ")}{pair.damage.rolls}d{pair.damage.dice} {pair.type}";
                first = false;
            }
            meleeText += " damage.\n\n";

            block = item.melee.secondary;
            meleeText += $"As a secondary weapon, this weapon has <color=#2222CC>{block.accuracy} accuracy<#000000> and <#33AA33>{block.piercing} piercing<#000000> with a default {block.chanceToHit}% chance to hit.\n";
            meleeText += $"On a hit, it deals ";
            first = true;
            foreach (DamagePairing pair in block.damage)
            {
                meleeText += $"{(first ? "" : " + ")}{pair.damage.rolls}d{pair.damage.dice} {pair.type}";
                first = false;
            }
            meleeText += " damage.\n\n";

            text += meleeText;
        }

        if (item.equipable)
        {
            Dictionary<Resources, float> stats = item.equipable.addedStats.dictionary;
            if (stats.Count > 0)
            {
                string equipText = "When equipped, this item will add:\n";
                int i = 0;
                foreach (Resources key in stats.Keys)
                {
                    int value = Mathf.RoundToInt(stats[key]);
                    equipText += $"{value} {key.ToString()}\n";
                }
                text += equipText + "\n";
            }
            
            if (item.equipable.addedEffects.Count > 0)
            {
                string effectText = "This item will grant the following status effects on equip:\n";
                foreach (Effect e in item.equipable.addedEffects)
                {
                    effectText += $"{e.GetName()} - {e.GetDescription()}\n";
                }
                text += effectText + "\n";
            }
        }

        if (item.activatable)
        {
            if ((item.activatable.activateType & ActivateType.Effect) > 0)
            {
                string effectText = "Upon activation, this item will grant the following effects:\n";
                foreach (Effect e in item.activatable.activationEffects)
                {
                    effectText += $"{e.GetName()} - {e.GetDescription()}\n";
                }
                text += effectText;
            }
        }

        attributesBox.text = text;
    }
    
    /* Called every time this panel is deactived by the controller */
    public override void OnDeactivation()
    {

    }

    /* Called every time this panel is focused on. Use this to refresh values that might have changed */
    public override void OnFocus()
    {

    }

    /*
     * Called when this panel is no longer focused on (added something to the UI stack). I don't know 
     * what on earth this would ever get used for, but I'm leaving it just in case (Nethack design!)
     */
    public override void OnDefocus()
    {
        
    }
}
