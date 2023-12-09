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

    public List<string> autocompleteOptions;
}
#endif

public class CheatsPanel : RogueUIPanel
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    //Don't uncomment these! These are already declared in the base class,
    //and are listed here so you know they exist.

    [SerializeField] TMP_InputField input;

    string lastSubmission = "";

    string lastInput;

    public List<CheatInfo> cachedCheats;

    public List<CheatInfo> active;

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
        int totalActive = 0;
        suggestionsBox.text = "";

        bool finishedCommand = false;
        foreach (CheatInfo cheat in cachedCheats)
        {
            int length = Mathf.Min(input.text.Length, cheat.name.Length);
            if (input.text.Substring(0, length).Equals(cheat.name.Substring(0, length), System.StringComparison.OrdinalIgnoreCase))
            {
                suggestionsBox.text += "\n" + cheat.command;
                active[totalActive] = cheat;
                totalActive++;
            }
        }

        //Just the one option! Switch to using autocomplete options now.
        if (totalActive == 1 && active[0].autocompleteOptions != null)
        {
            finishedCommand = (active[0].name.Length <= input.text.Length);
            suggestionsBox.text = "";
            foreach (string s in active[0].autocompleteOptions)
            {
                string newInput = active[0].name + " " + s;
                int length = Mathf.Min(input.text.Length, newInput.Length);
                if (input.text.Substring(0, length).Equals(newInput.Substring(0, length), System.StringComparison.OrdinalIgnoreCase))
                {
                    suggestionsBox.text += "\n" + newInput;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && totalActive > 0)
        {
            if (!finishedCommand || (active[totalActive-1].parameters == null))
            {
                input.text = active[totalActive - 1].name;
                if (active[totalActive - 1].parameters.Length > 0)
                {
                    input.text += " ";
                }
                input.MoveToEndOfLine(false, false);
            }
            else
            {
                List<string> options = suggestionsBox.text.Split('\n').ToList();
                options.RemoveAt(0);
                if (options.Count > 0)
                {
                    string final = options[0];
                    foreach (string nextOption in options)
                    {
                        int length = Mathf.Min(final.Length, nextOption.Length);
                        if (length == 0) return;
                        for (int i = 0; i < length; i++)
                        {
                            if (nextOption[i] != final[i])
                            {
                                final = final.Substring(0, i);
                                break;
                            }
                        }
                    }
                    input.text = final;
                    input.MoveToEndOfLine(false, false);
                }
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
            case PlayerAction.MOVE_UP:
                if (!inputString.Any(x => "wk".Contains(x)))
                {
                    input.text = lastSubmission;
                }
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
        if (cachedCheats == null)
        {
            cachedCheats = new List<CheatInfo>();
            active = new List<CheatInfo>();
        }

        if (cachedCheats.Count == 0)
        {
            //MethodInfo[] methods = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).SelectMany(x => x.GetMethods().Where(y => y.IsPublic)).ToArray();
            MethodInfo[] methods = typeof(CheatsPanel).GetMethods();
            foreach (MethodInfo method in methods)
            {
                CheatAttribute cheatAttribute = method.GetCustomAttribute<CheatAttribute>();
                if (cheatAttribute != null)
                {
                    CheatInfo newCheat = new CheatInfo();
                    newCheat.method = method;
                    newCheat.name = method.Name;
                    newCheat.parameters = method.GetParameters();
                    string commandString = method.Name;
                    foreach (ParameterInfo param in newCheat.parameters)
                    {
                        commandString = commandString + $" [{param.ParameterType.Name}]";
                    }
                    newCheat.command = commandString;

                    //Find autocomplete?
                    if (cheatAttribute.hasAutoComplete)
                    {
                        //Do the thing!
                        newCheat.autocompleteOptions = (List<string>) typeof(CheatsPanel).GetMethod(method.Name + "_AutoComplete").Invoke(this, new object[0]);
                    }

                    cachedCheats.Add(newCheat);
                    active.Add(newCheat);
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
                lastSubmission = submission;
                return;
            }
        }

        Debug.Log($"Could not find cheat with the name {cheatSplit[0]}");
    }

    public void ProcessCheat(CheatInfo cheat, List<string> parameters)
    {
        /*if (cheat.parameters.Length > parameters.Count)
        {
            Debug.Log($"Incorrect number of parameters for {cheat.name}");
            return;
        }*/

        while (parameters.Count < cheat.parameters.Length)
        {
            Type t = cheat.parameters[parameters.Count].ParameterType;
            parameters.Add(GetDefaultOperator(t));
        }

        while (parameters.Count > cheat.parameters.Length)
        {
            parameters.RemoveAt(parameters.Count - 1);
        }

        List<object> methodParams = new List<object>();

        for (int i = 0; i < parameters.Count; i++)
        {
            var converter = TypeDescriptor.GetConverter(cheat.parameters[i].ParameterType);
            methodParams.Add(converter.ConvertFromString(parameters[i]));
        }

        cheat.method.Invoke(this, methodParams.ToArray());
    }

    public string GetDefaultOperator(Type t)
    {
        if (t.IsValueType)
        {
            return Activator.CreateInstance(t).ToString();
        }

        return "";
    }

    //CHEATS!
    [Cheat]
    public void KillPlayer()
    {
        Player.player.Damage(Player.player, 9999, DamageType.TRUE, DamageSource.EFFECT);
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
                monster.Damage(Player.player, 9999, DamageType.TRUE, DamageSource.MONSTER);
            }
        }
    }

    [Cheat]
    public void ClearAllAchievements()
    {
        SteamController.singleton?.ClearAllAchievements();
    }

    [Cheat]
    public void ClearAchievement(string name)
    {
        SteamController.singleton?.ClearAchievement(name);
    }

    [Cheat]
    public void GiveAchievement(string name)
    {
        SteamController.singleton?.GiveAchievement(name);
    }

    [Cheat]
    public void SetStat(string name, int value)
    {
        SteamController.singleton?.SetStat(name, value);
        SteamController.singleton?.StoreStats();
    }

    [Cheat]
    public void PrintSteamDiagnostics()
    {
        SteamController.singleton?.PrintDiagnostics();
    }

    [Cheat]
    public void ForceLoadAllLevels()
    {
        for (int i = 0; i < LevelLoader.singleton.generators.Count; i++)
        {
            LevelLoader.singleton.FastLoadLevel(i);
        }
    }

    [Cheat(true)]
    public void MoveToLevel(string levelName)
    {
        int index = LevelLoader.singleton.GetIndexOf(levelName);
        if (index >= 0)
        {
            LOS.lastCall.Deprint(Map.current);
            GameController.singleton.LoadMap(index);
            Player.player.transform.parent = Map.current.monsterContainer;
            Player.player.SetPositionSnap(Map.current.GetRandomWalkableTile());
            
            LOS.lastCall = null;
            CameraTracking.singleton.JumpToPlayer();
            LOS.GeneratePlayerLOS(Map.current, Player.player.location, Player.player.visionRadius);
        }
        else
        {
            RogueLog.singleton.Log($"Could not move to level {levelName}, as it does not exist.", priority: LogPriority.IMPORTANT, display: LogDisplay.ABILITY);
        }    
    }

    public List<string> MoveToLevel_AutoComplete()
    {
        List<string> options = new List<string>();
        foreach (DungeonGenerator generator in LevelLoader.singleton.generators)
        {
            options.Add(generator.name);
        }

        return options;
    }

    [Cheat]
    public void SetFriendly()
    {
        Player.player.faction = (Faction) (-1);
    }

    [Cheat]
    public void SetUnfriendly()
    {
        Player.player.faction = Faction.PLAYER;
    }

    [Cheat]
    public void LimitFrameRate(int framesPerSecond)
    {
        Application.targetFrameRate = framesPerSecond;
    }

    [Cheat]
    public void UnlockFrameRate()
    {
        Application.targetFrameRate = -1;
    }


    [Cheat(true)]
    public void SpawnItem(string name, string idString, ItemRarity rarity)
    {
        int id = -1;
        if (!int.TryParse(name.Substring(0, Mathf.Min(name.Length, 3)), out id))
        {
            if (!int.TryParse(idString.Substring(0, Mathf.Min(idString.Length, 3)), out id))
            {
                Debug.LogError("Neither input was an ID!");
                return;
            }
        }
        Debug.Log($"Generating item with id {id} and rarity {rarity}");
        Item toSpawn = ItemSpawner.singleton.GetItemByID(id);
        ItemSpawner.singleton.SpawnItem(toSpawn, Player.player.location, Map.current, rarity);
    }

    public List<string> SpawnItem_AutoComplete()
    {
        List<string> options = new List<string>();
        HashSet<int> items = new HashSet<int>();
        foreach (Branch branch in World.current.branches)
        {
            foreach (Item item in branch.lootPool.tree.GetItemsIn(branch.lootPool.tree.rect))
            {
                if (!items.Contains(item.ID))
                {
                    items.Add(item.ID);
                    options.Add($"{item.friendlyName} {item.ID.ToString("000")}");
                    options.Add($"{item.ID.ToString("000")} {item.friendlyName}");
                    //options.Add(item.gameObject.name.Substring(0, item.gameObject.name.Length - 7));
                }
            }
        }

        return options;
    }



    [Cheat(true)]
    public void AddActionBinding(PlayerAction action, char key)
    {
        PlayerPrefs.SetString($"InputOverride:{action}", key.ToString());
        InputTracking.GenerateActionsDictionary();
    }

    public List<string> AddActionBinding_AutoComplete()
    {
        List<string> options = new List<string>();
        foreach (PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
        {
            options.Add(action.ToString());
        }

        return options;
    }

    [Cheat]
    public void SetViBindings()
    {
        AddActionBinding(PlayerAction.MOVE_LEFT, 'h');
        AddActionBinding(PlayerAction.MOVE_RIGHT, 'l');
        AddActionBinding(PlayerAction.MOVE_UP, 'k');
        AddActionBinding(PlayerAction.MOVE_DOWN, 'j');
        AddActionBinding(PlayerAction.MOVE_UP_LEFT, 'y');
        AddActionBinding(PlayerAction.MOVE_UP_RIGHT, 'u');
        AddActionBinding(PlayerAction.MOVE_DOWN_LEFT, 'b');
        AddActionBinding(PlayerAction.MOVE_DOWN_RIGHT, 'n');
        AddActionBinding(PlayerAction.DROP_ITEMS, 'd');
        AddActionBinding(PlayerAction.APPLY, 'a');
    }

    [Cheat]
    public void ClearKeyBindings()
    {
        foreach (PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
        {
            PlayerPrefs.DeleteKey($"InputOverride:{action}");
        }
        InputTracking.GenerateActionsDictionary();
    }

    [Cheat]
    public void SetInputDelay(float delay)
    {
        InputTracking.maxCombineDelay = delay;
    }

    [Cheat(true)]
    public void AddDamage(DamageType type, float amount)
    {
        Player.player.Damage(Player.player, amount, type, DamageSource.ABILITY);
    }

    public List<string> AddDamage_AutoComplete()
    {
        List<string> options = new List<string>();
        foreach (String name in Enum.GetNames(typeof(DamageType)))
        {
            options.Add(name);
        }

        return options;
    }

#endif
}
