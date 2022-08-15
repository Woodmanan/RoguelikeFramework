using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueUIPanel : MonoBehaviour
{
    public static List<RogueUIPanel> panels = new List<RogueUIPanel>();
    public static RogueUIPanel inFocus;


    [HideInInspector] public bool isInFocus;
    [HideInInspector] public bool canBeEscaped = true;

    public static bool WindowsOpen
    {
        get { return panels.Count > 0; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void HandleInput(PlayerAction action, string inputString) { }

    public static void AttemptExitTopLevel()
    {
        if (panels.Count == 0)
        {
            //Made this not an error, just in case of weird jankiness
            Debug.Log("Can't deactivate top level UI when there are no panels open.");
        }
        else
        {
            RogueUIPanel toRemove = panels[panels.Count - 1];
            if (toRemove.canBeEscaped)
            {
                ExitTopLevel();
            }
        }
    }

    public static void ExitTopLevel()
    {
        if (panels.Count == 0)
        {
            //Made this not an error, just in case of weird jankiness
            Debug.Log("Can't deactivate top level UI when there are no panels open.");
        }
        else
        {
            RogueUIPanel toRemove = panels[panels.Count - 1];
            toRemove.Deactivate();
            panels.RemoveAt(panels.Count - 1);
            if (panels.Count != 0)
            {
                inFocus = panels[panels.Count - 1];
                inFocus.Focus();
            }
        }
    }

    public static void ExitAllWindows()
    {
        int windowsToClose = panels.Count; //Done in case someone tries to do something funky
        for (int i = 0; i < windowsToClose; i++)
        {
            ExitTopLevel();
        }
        if (panels.Count != 0)
        {
            //Either someone is breaking the rules and opening new windows, OR, this function doesn't work correctly
            Debug.LogError("Error: Panel is opening new panels upon closing! (Detected with ExitAllWindows())");
        }
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        panels.Add(this);
        if (inFocus)
        {
            inFocus.Defocus();
        }
        inFocus = this;
        Focus();
        OnActivation();
    }

    public virtual void OnActivation()
    {

    }

    public void Deactivate()
    {
        Defocus();
        OnDeactivation();
        gameObject.SetActive(false);
    }

    public virtual void OnDeactivation()
    {

    }

    public void Focus()
    {
        transform.SetAsLastSibling(); //FUCKING MAGIC RIGHT HERE
        isInFocus = true;
        OnFocus();
    }

    public void Defocus()
    {
        isInFocus = false;
        OnDefocus();
    }

    public virtual void OnFocus() { }

    public virtual void OnDefocus() { }

}
