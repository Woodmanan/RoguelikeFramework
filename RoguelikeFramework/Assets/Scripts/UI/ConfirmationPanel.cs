using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConfirmationPanel : RogueUIPanel
{
    BoolDelegate current;
    //Don't uncomment these! These are already declared in the base class,
    //and are listed here so you know they exist.

    //bool inFocus; - Tells you if this is the window that is currently focused. Not too much otherwise.

    [SerializeField] TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        this.canBeEscaped = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(string message, BoolDelegate del)
    {
        text.text = message;
        current = del;
    }

    /*
     * One of the more important functions here. When in focus, this will be called
     * every frame with the stored input from InputTracking
     */
    public override void HandleInput(PlayerAction action, string inputString)
    {
        if (inputString.Contains("n") || inputString.Contains("y"))
        {
            //TODO: console log
            Debug.Log("Log should print: uppercase Y or N only, please.");
        }    
        if (inputString.Contains("N"))//|| inputString.Contains("n"))
        {
            Deny();
        }
        else if (inputString.Contains("Y"))
        {
            Confirm();
        }
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

    public void Confirm()
    {
        current(true);
        ExitTopLevel();
    }

    public void Deny()
    {
        current(false);
        ExitTopLevel();
    }
}
