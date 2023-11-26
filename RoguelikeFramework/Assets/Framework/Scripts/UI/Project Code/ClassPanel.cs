using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClassPanel : RogueUIPanel
{
    //Don't uncomment these! These are already declared in the base class,
    //and are listed here so you know they exist.

    //bool inFocus; - Tells you if this is the window that is currently focused. Not too much otherwise.

    public Class current;

    public TextMeshProUGUI namePanel;

    public RectTransform abilityHolder;

    List<int> selected = new List<int>();

    public TextMeshProUGUI errorMessage;
    public Button button;

    // Start is called before the first frame update
    void Start()
    {
        this.canBeEscaped = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(Class classToGive)
    {
        selected.Clear();
        current = classToGive;
        namePanel.text = classToGive.name;
        for (int i = 0; i < current.abilities.Count; i++)
        {
            abilityHolder.GetChild(i).gameObject.SetActive(true);
            abilityHolder.GetChild(i).GetComponent<AbilitySelect>().Setup(this, i);
            selected.Add(i);
        }

        for (int j = current.abilities.Count; j < abilityHolder.childCount; j++)
        {
            abilityHolder.GetChild(j).gameObject.SetActive(false);
        }

        CheckValid();
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

    public void Click(int index, bool isSelected)
    {
        if (isSelected && !selected.Contains(index))
        {
            selected.Add(index);
        }
        else
        {
            selected.Remove(index);
        }

        CheckValid();
    }

    public void CheckValid()
    {
        if (selected.Count + Player.player.abilities.Count > Player.player.abilities.maxAbilities)
        {
            button.interactable = false;
            errorMessage.enabled = true;
        }
        else
        {
            button.interactable = true;
            errorMessage.enabled = false;
        }
    }

    public void Finish()
    {
        selected.Sort();
        foreach (int index in selected)
        {
            Player.player.abilities.AddAbilityInstantiate(current.abilities[index].Instantiate());
        }
        if (selected.Count > 0)
        {
            Player.player.AddEffectInstantiate(current.effects.ToArray());
        }

        ExitAllWindows();
    }
}
