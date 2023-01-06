using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    [SerializeField] RectTransform effectsPanel;
    [SerializeField] TextMeshProUGUI equipEffectsText;
    [SerializeField] RectTransform equipEffectsPanel;
    [SerializeField] TextMeshProUGUI activateEffectsText;
    [SerializeField] RectTransform activateEffectsPanel;
    [SerializeField] private GameObject efffectDisplayPrefab;
    [SerializeField] TMP_Dropdown rarityDisplay;
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
        nameBox.text = $"{Conversions.IntToNumbering(inspecting.position)} - {inspecting.GetName()}";
        SpriteRenderer render = inspecting.held[0].GetComponent<SpriteRenderer>();
        image.sprite = render.sprite;
        image.color = render.color;

        DetermineDescription(inspecting.held[0]);
        DetermineAttributes(inspecting.held[0]);
        ShowConnectedEffects(inspecting.held[0].attachedEffects.Where(x => x.ShouldDisplay()).ToList());
        rarityDisplay.value = (int) inspecting.held[0].currentRarity;
        rarityDisplay.itemText.color = Item.GetRarityColor(inspecting.held[0].currentRarity);
    }

    public void DetermineDescription(Item item)
    {
        string text = "";
        if (item.localDescription.IsEmpty)
        {
            text = "Missing description! Sorry about that, please let Woody know.";
        }
        else
        {
            text = item.localDescription.GetLocalizedString();
        }
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
                foreach (Resources key in stats.Keys)
                {
                    int value = Mathf.RoundToInt(stats[key]);
                    equipText += $"{value} {key}\n";
                }
                text += equipText + "\n";
            }

            ShowAttachedEffects(item.equipable.addedEffects.Where(x => x.ShouldDisplay()).ToList());
        }
        else
        {
            ShowAttachedEffects(new List<Effect>());
        }

        if (item.activatable)
        {
            if ((item.activatable.activateType & ActivateType.Effect) > 0)
            {
                ShowActivatedEffects(item.activatable.activationEffects.Where(x => x.ShouldDisplay()).ToList());
            }
        }
        else
        {
            ShowActivatedEffects(new List<Effect>());
        }

        attributesBox.text = text;
    }

    public void ShowConnectedEffects(List<Effect> effects)
    {
        while (effectsPanel.childCount < effects.Count)
        {
            GameObject spawned = Instantiate(efffectDisplayPrefab);
            spawned.transform.parent = effectsPanel;
        }

        if (effectsPanel.childCount > effects.Count)
        {
            for (int i = effects.Count; i < effectsPanel.childCount; i++)
            {
                effectsPanel.GetChild(i).gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < effects.Count; i++)
        {
            effectsPanel.GetChild(i).gameObject.SetActive(true);
            effectsPanel.GetChild(i).GetComponent<EffectDisplay>().SetDisplay(effects[i]);
        }
    }

    public void ShowAttachedEffects(List<Effect> effects)
    {
        if (effects.Count == 0)
        {
            equipEffectsText.gameObject.SetActive(false);
            equipEffectsPanel.gameObject.SetActive(false);
            return;
        }
        else
        {
            equipEffectsText.gameObject.SetActive(true);
            equipEffectsPanel.gameObject.SetActive(true);
        }

        while (effectsPanel.childCount < effects.Count)
        {
            GameObject spawned = Instantiate(efffectDisplayPrefab);
            spawned.transform.parent = equipEffectsPanel;
        }

        if (equipEffectsPanel.childCount > effects.Count)
        {
            for (int i = effects.Count; i < equipEffectsPanel.childCount; i++)
            {
                equipEffectsPanel.GetChild(i).gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < effects.Count; i++)
        {
            equipEffectsPanel.GetChild(i).gameObject.SetActive(true);
            equipEffectsPanel.GetChild(i).GetComponent<EffectDisplay>().SetDisplay(effects[i]);
        }
    }

    public void ShowActivatedEffects(List<Effect> effects)
    {
        if (effects.Count == 0)
        {
            activateEffectsText.gameObject.SetActive(false);
            activateEffectsPanel.gameObject.SetActive(false);
            return;
        }
        else
        {
            activateEffectsText.gameObject.SetActive(true);
            activateEffectsPanel.gameObject.SetActive(true);
        }

        while (effectsPanel.childCount < effects.Count)
        {
            GameObject spawned = Instantiate(efffectDisplayPrefab);
            spawned.transform.parent = activateEffectsPanel;
        }

        if (activateEffectsPanel.childCount > effects.Count)
        {
            for (int i = effects.Count; i < activateEffectsPanel.childCount; i++)
            {
                activateEffectsPanel.GetChild(i).gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < effects.Count; i++)
        {
            activateEffectsPanel.GetChild(i).gameObject.SetActive(true);
            activateEffectsPanel.GetChild(i).GetComponent<EffectDisplay>().SetDisplay(effects[i]);
        }
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
