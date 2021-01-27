using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemInspectionPanel : RogueUIPanel
{
    //Don't uncomment these variables! These are already declared in the base class,
    //and are listed here so you know they exist.

    //bool inFocus; - Tells you if this is the window that is currently focused

    private ItemStack inspecting; // The item that we are inspecting
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI attributes;
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
        }
    }

    /* Called every time this panel is activated by the controller */
    public override void OnActivation()
    {
        name.text = $"{Conversions.IntToNumbering(inspecting.position)} - {inspecting.GetName()}";
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
