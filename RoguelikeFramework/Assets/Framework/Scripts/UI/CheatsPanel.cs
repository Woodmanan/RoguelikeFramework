using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using System;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public struct CheatInfo
{
    public MethodInfo method;
    public ParameterInfo[] parameters;
    public string command;
    public string name;
}
#endif

public class CheatsPanel : RogueUIPanel
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    //Don't uncomment these! These are already declared in the base class,
    //and are listed here so you know they exist.

    [SerializeField] TMP_InputField input;

    string lastInput;

    public List<CheatInfo> cachedCheats;

    public TextMeshProUGUI suggestionsBox;

    //bool inFocus; - Tells you if this is the window that is currently focused. Not too much otherwise.

    // Start is called before the first frame update
    void Start()
    {
        input.onSubmit.AddListener(Submit);
        CacheCheats();
    }

    // Update is called once per frame
    void Update()
    {
        suggestionsBox.text = "";
        foreach (CheatInfo cheat in cachedCheats)
        {
            int length = Mathf.Min(input.text.Length, cheat.name.Length);
            if (input.text.Substring(0, length).Equals(cheat.name.Substring(0, length), System.StringComparison.OrdinalIgnoreCase))
            {
                suggestionsBox.text += "\n" + cheat.command;
            }
        }
    }

    /*
     * One of the more important functions here. When in focus, this will be called
     * every frame with the stored input from InputTracking
     */
    public override void HandleInput(PlayerAction action, string inputString)
    {
       switch(action)
        {
            case PlayerAction.DEV_CHEAT:
                lastInput = input.text.Remove(input.text.Length - 1);
                ExitAllWindows();
                break;
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
        EventSystem.current.SetSelectedGameObject(input.gameObject, null);
        input.ActivateInputField();
        if (lastInput != null)
        {
            input.text = lastInput;
            input.stringPosition = lastInput.Length;
        }
    }

    /*
     * Called when this panel is no longer focused on (added something to the UI stack). I don't know 
     * what on earth this would ever get used for, but I'm leaving it just in case (Nethack design!)
     */
    public override void OnDefocus()
    {
        
    }

    public void CacheCheats()
    {
        if (cachedCheats == null) cachedCheats = new List<CheatInfo>();

        if (cachedCheats.Count == 0)
        {
            //MethodInfo[] methods = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).SelectMany(x => x.GetMethods().Where(y => y.IsPublic)).ToArray();
            MethodInfo[] methods = typeof(CheatsPanel).GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute<CheatAttribute>() != null)
                {
                    CheatInfo newCheat = new CheatInfo();
                    Debug.Log("Found cheat named " + method.Name);
                    newCheat.method = method;
                    newCheat.name = method.Name;
                    newCheat.parameters = method.GetParameters();
                    string commandString = method.Name;
                    foreach (ParameterInfo param in newCheat.parameters)
                    {
                        commandString = commandString + $" [{param.ParameterType.Name}]";
                    }
                    newCheat.command = commandString;
                    cachedCheats.Add(newCheat);
                }
            }
        }
    }

    public void Submit(string submission)
    {
        lastInput = "";
        ExitAllWindows();

        List<string> cheatSplit = submission.Split(' ').ToList();
        foreach (CheatInfo cheat in cachedCheats)
        {
            if (cheat.name.Equals(cheatSplit[0], System.StringComparison.OrdinalIgnoreCase))
            {
                cheatSplit.RemoveAt(0);
                ProcessCheat(cheat, cheatSplit);
                return;
            }
        }

        Debug.Log($"Could not find cheat with the name {cheatSplit[0]}");
    }

    public void ProcessCheat(CheatInfo cheat, List<string> parameters)
    {
        if (cheat.parameters.Length != parameters.Count)
        {
            Debug.Log($"Incorrect number of parameters for {cheat.name}");
            return;
        }

        List<object> methodParams = new List<object>();

        for (int i = 0; i < parameters.Count; i++)
        {
            var converter = TypeDescriptor.GetConverter(cheat.parameters[i].ParameterType);
            methodParams.Add(converter.ConvertFromString(parameters[i]));
        }

        cheat.method.Invoke(this, methodParams.ToArray());
    }

    //CHEATS!
    [Cheat]
    public void KillPlayer()
    {
        Player.player.Damage(Player.player, int.MaxValue, DamageType.NONE, DamageSource.EFFECT, "You cheat die");
    }

    [Cheat]
    public void GiveHealth(int health)
    {
        Player.player.Heal(health);
    }

    [Cheat]
    public void SetLevel(int level)
    {
        Player.player.level = level;
    }

    [Cheat]
    public void ShowMap()
    {
        Map map = Map.current;
        for (int y = 0; y < map.height; y++)
        {
            for (int x = 0; x < map.width; x++)
            {
                map.GetTile(x, y).isVisible = true;
                map.GetTile(x, y).isHidden = false;
                map.GetTile(x, y).dirty = true;
            }
        }
    }

    [Cheat]
    public void KillAllMonsters()
    {
        foreach (Monster monster in Map.current.monsters)
        {
            if (monster != Player.player)
            {
                monster.Damage(Player.player, 1000, DamageType.NONE, DamageSource.MONSTER, "{name} dies to a cheat.");
            }
        }
    }

    [Cheat]
    public void ClearAllAchievements()
    {
        SteamController.singleton.ClearAllAchievements();
    }

    [Cheat]
    public void ClearAchievement(string name)
    {
        SteamController.singleton.ClearAchievement(name);
    }

    [Cheat]
    public void GiveAchievement(string name)
    {
        SteamController.singleton.GiveAchievement(name);
    }

    [Cheat]
    public void SetStat(string name, int value)
    {
        SteamController.singleton.SetStat(name, value);
        SteamController.singleton.StoreStats();
    }

    [Cheat]
    public void PrintSteamDiagnostics()
    {
        SteamController.singleton.PrintDiagnostics();
    }
    #endif
}
