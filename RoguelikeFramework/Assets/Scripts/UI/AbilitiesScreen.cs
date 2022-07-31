using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class AbilitiesScreen : RogueUIPanel
{
    [SerializeField] public Transform holdingPanel;
    [SerializeField] public GameObject panelPrefab;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI failureMessage;

    public Abilities examinedAbilities;

    public List<ClickButton> displayed = new List<ClickButton>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(Abilities abilities)
    {
        examinedAbilities = abilities;
    }

    /*
     * One of the more important functions here. When in focus, this will be called
     * every frame with the stored input from InputTracking
     */
    public override void HandleInput(PlayerAction action, string inputString)
    {
        //Break down input into item types
        foreach (char c in inputString.Where(c => char.IsLetter(c)))
        {
            int index = Conversions.NumberingToInt(c);
            if (index < examinedAbilities.Count && examinedAbilities[index].castable)
            {
                //Cast a spell
                Debug.Log($"Button {index} was pressed. Casting {examinedAbilities[index].displayName}!");
                Player.player.SetAction(new AbilityAction(index));
                ExitAllWindows();
                break;
            }
        }
    }

    /* Called every time this panel is activated by the controller */
    public override void OnActivation()
    {
        Debug.Log($"Displayed count is {displayed.Count}");
        Debug.Log($"Examined count is {examinedAbilities.Count}");

        for (int i = displayed.Count - 1; i >= examinedAbilities.Count; i--)
        {
            displayed[i].gameObject.SetActive(false);
        }

        for (int i = displayed.Count; i < examinedAbilities.Count; i++)
        {
            GameObject instance = Instantiate(panelPrefab);
            instance.transform.SetParent(holdingPanel);
            displayed.Add(instance.GetComponent<ClickButton>());
        }

        for (int i = 0; i < examinedAbilities.Count; i++)
        {
            displayed[i].gameObject.SetActive(true);
            displayed[i].Setup(Cast, i);
            displayed[i].SetDisplay(examinedAbilities[i].image, $"{Conversions.IntToNumbering(i)} - {examinedAbilities[i].displayName}", examinedAbilities[i].color);
            displayed[i].SetCooldown(examinedAbilities[i].currentCooldown, examinedAbilities[i].stats.cooldown);
            if (!examinedAbilities[i].castable)
            {
                displayed[i].Disable();
            }
        }

        failureMessage.gameObject.SetActive(examinedAbilities.Count == 0);
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

    public void Cast(int index)
    {
        if (displayed[index].clickable)
        {
            Debug.Log($"Button {index} was pressed. Casting {examinedAbilities[index].displayName}!");
            Player.player.SetAction(new AbilityAction(index));
            ExitAllWindows();
        }
    }
}
