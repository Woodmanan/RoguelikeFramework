using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonalAttributePanel : RogueUIPanel
{
    //Don't uncomment these! These are already declared in the base class,
    //and are listed here so you know they exist.

    //bool inFocus; - Tells you if this is the window that is currently focused. Not too much otherwise.

    Effect attribute;
    Effect backup;

    [SerializeField]
    EffectDisplay display;

    // Start is called before the first frame update
    void Start()
    {
        this.canBeEscaped = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(Effect attribute, Effect backup)
    {
        this.attribute = attribute;
        this.backup = backup;
        display.SetDisplay(attribute);
    }

    /*
     * One of the more important functions here. When in focus, this will be called
     * every frame with the stored input from InputTracking
     */
    public override void HandleInput(PlayerAction action, string inputString)
    {
       
    }

    /* Called every time this panel is activated by the controller */
    public override void OnActivation()
    {
        
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

    public void ChooseAttribute()
    {
        //Exit first, in case the attibute wants to open a window on its connection
        ExitAllWindows();
        Player.player[0].AddEffectInstantiate(attribute);
    }

    public void RejectAttribute()
    {
        ExitAllWindows();
        Player.player[0].AddEffectInstantiate(backup);
    }
}
