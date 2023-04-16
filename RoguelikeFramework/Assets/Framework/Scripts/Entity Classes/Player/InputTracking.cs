using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

[Serializable]
public struct ActionLigature
{
    public PlayerAction inputOne;
    public PlayerAction inputTwo;
    public PlayerAction result;
}

public class InputTracking : MonoBehaviour
{
    //Public functions for accessing all of this
    public static Queue<PlayerAction> actions = new Queue<PlayerAction>();
    public static Queue<string> inputs = new Queue<string>();

    public static List<PlayerAction> frameActions = new List<PlayerAction>();

    public static List<PlayerAction> processingActions = new List<PlayerAction>();
    public static string processingInput = "";
    public static float lastInputAdded;

    //IMPORTANT! This is our input lag
    //Longer gives more flex for combined inputs, but slower single inputs.
    public static float maxCombineDelay = 0.04f;

    public static Dictionary<char, PlayerAction> baseActions = new Dictionary<char, PlayerAction>();
    public static List<ActionLigature> actionLigatures = new List<ActionLigature>();

    public static bool HasNextAction()
    {
        return actions.Count > 0;
    }

    public static void Clear()
    {
        actions.Clear();
        inputs.Clear();
    }

    public static PlayerAction PopNextAction()
    {
        if (HasNextAction())
        {
            inputs.Dequeue();
            return actions.Dequeue();
        }
        else
        {
            return PlayerAction.NONE;
        }
    }

    public static (PlayerAction, string) PopNextPair()
    {
        if (HasNextAction())
        {
            PlayerAction act = actions.Dequeue();
            string inp = inputs.Dequeue();
            return (act, inp);
        }
        else
        {
            return (PlayerAction.NONE, "");
        }
    }

    public static Tuple<PlayerAction, string> PeekNextPair()
    {
        if (HasNextAction())
        {
            PlayerAction act = actions.Peek();
            string inp = inputs.Peek();
            return new Tuple<PlayerAction, string>(act, inp);
        }
        else
        {
            return new Tuple<PlayerAction, string>(PlayerAction.NONE, "");
        }
    }

    public static PlayerAction PeekNextAction()
    {
        if (HasNextAction())
        {
            return actions.Peek();
        }
        else
        {
            return PlayerAction.NONE;
        }
    }

    public static void PushAction(PlayerAction act)
    {
        PushAction(act, Input.inputString);
    }

    public static void PushAction(PlayerAction act, string inputString)
    {
        actions.Enqueue(act);
        inputs.Enqueue(inputString);
    }

    public static bool ContainsEscape()
    {
        foreach (PlayerAction action in actions)
        {
            if (action == PlayerAction.ESCAPE_SCREEN)
            {
                return true;
            }
        }
        return false;
    }

    public static int NumOfUnmatchedActions(params PlayerAction[] matchedActions)
    {
        int count = 0;
        foreach (PlayerAction action in actions)
        { 
            bool matched = false;
            foreach (PlayerAction match in matchedActions)
            {
                if (action == match)
                {
                    matched = true;
                    break;
                }
            }

            if (!matched) count++;
        }

        return count;
    }

    //Check if there are inputs to read this frame
    public static bool HasAnyInputs()
    {
        return Input.anyKeyDown;
    }

    public static bool HasUnprocessedInput()
    {
        return processingInput.Length > 0 || processingActions.Count > 0;
    }

    public static void PushUnprocessedInput()
    {
        if (!HasUnprocessedInput()) return;
        foreach (PlayerAction action in processingActions)
        {
            PushAction(action, processingInput);
            processingInput = "";
        }
        processingActions.Clear();
    }

    public static void UpdateFrameActions()
    {
        frameActions.Clear();
        AddNormalActions();
        AddSpecialActions();
    }

    public static void AddNormalActions()
    {
        foreach (char c in Input.inputString)
        {
            PlayerAction outAction;
            if (baseActions.TryGetValue(c, out outAction))
            {
                frameActions.Add(outAction);
            }
        }
    }

    public static void AddSpecialActions()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            frameActions.Add(PlayerAction.AUTO_ATTACK);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            frameActions.Add(PlayerAction.ESCAPE_SCREEN);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            frameActions.Add(PlayerAction.ACCEPT);
        }

        //Directional arrow inputs
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            frameActions.Add(PlayerAction.MOVE_LEFT);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            frameActions.Add(PlayerAction.MOVE_RIGHT);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
        {
            frameActions.Add(PlayerAction.MOVE_UP);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            frameActions.Add(PlayerAction.MOVE_DOWN);
        }

        //Check numpad keys
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            frameActions.Add(PlayerAction.WAIT);
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            frameActions.Add(PlayerAction.MOVE_DOWN_LEFT);
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            frameActions.Add(PlayerAction.MOVE_DOWN_RIGHT);
        }
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            frameActions.Add(PlayerAction.MOVE_UP_LEFT);
        }
        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            frameActions.Add(PlayerAction.MOVE_UP_RIGHT);
        }


    }

    public static bool ProcessLigatures(List<PlayerAction> actions)
    {
        bool combinedAny = false;
        foreach (ActionLigature ligature in actionLigatures)
        {
            if (actions.Contains(ligature.inputOne) && actions.Contains(ligature.inputTwo))
            {
                actions.Remove(ligature.inputOne);
                actions.Remove(ligature.inputTwo);
                actions.Add(ligature.result);
                combinedAny = true;
            }
        }

        return combinedAny;
    }

    public static bool ProcessCombinedLigatures(List<PlayerAction> oldInputs, List<PlayerAction> newInputs)
    {
        bool combinedAny = false;
        foreach (ActionLigature ligature in actionLigatures)
        {
            if (oldInputs.Contains(ligature.inputOne))
            {
                if (newInputs.Contains(ligature.inputTwo))
                {
                    oldInputs.Remove(ligature.inputOne);
                    newInputs.Remove(ligature.inputTwo);
                    oldInputs.Add(ligature.result);
                    combinedAny = true;
                }
            }
            else if (oldInputs.Contains(ligature.inputTwo))
            {
                if (newInputs.Contains(ligature.inputOne))
                {
                    oldInputs.Remove(ligature.inputTwo);
                    newInputs.Remove(ligature.inputOne);
                    oldInputs.Add(ligature.result);
                    combinedAny = true;
                }
            }
        }

        return combinedAny;
    }

    public static void AddLigature(PlayerAction inputOne, PlayerAction inputTwo, PlayerAction result)
    {
        ActionLigature ligature = new ActionLigature();
        ligature.inputOne = inputOne;
        ligature.inputTwo = inputTwo;
        ligature.result = result;
        actionLigatures.Add(ligature);
    }

    public static void GenerateLigatures()
    {
        actionLigatures.Clear();

        //Diagonal ligatures
        AddLigature(PlayerAction.MOVE_LEFT, PlayerAction.MOVE_UP, PlayerAction.MOVE_UP_LEFT);
        AddLigature(PlayerAction.MOVE_RIGHT, PlayerAction.MOVE_UP, PlayerAction.MOVE_UP_RIGHT);
        AddLigature(PlayerAction.MOVE_LEFT, PlayerAction.MOVE_DOWN, PlayerAction.MOVE_DOWN_LEFT);
        AddLigature(PlayerAction.MOVE_RIGHT, PlayerAction.MOVE_DOWN, PlayerAction.MOVE_DOWN_RIGHT);
        AddLigature(PlayerAction.MOVE_RIGHT, PlayerAction.MOVE_LEFT, PlayerAction.WAIT);
    }

    public static void GenerateActionsDictionary()
    {
        baseActions.Clear();
        foreach (PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
        {
            string overrideKey = $"InputOverride:{action}";
            if (PlayerPrefs.HasKey(overrideKey))
            {
                string overrideValue = PlayerPrefs.GetString(overrideKey);
                if (overrideValue.Length == 0)
                {
                    Debug.LogError($"{action} had an unacceptable override!");
                    continue;
                }

                char inputKey = overrideValue.ToLower()[0];

                if (baseActions.ContainsKey(inputKey))
                {
                    Debug.LogError($"{inputKey} was bound to both {action} and {baseActions[inputKey]}");
                    continue;
                }

                baseActions.Add(inputKey, action);
            }
            else
            {
                char defaultInput = GetDefaultInput(action);
                if (defaultInput != '\0')
                {
                    if (baseActions.ContainsKey(defaultInput))
                    {
                        Debug.LogError($"{defaultInput} was bound to both {action} and {baseActions[defaultInput]}");
                        continue;
                    }
                    baseActions.Add(defaultInput, action);
                }
            }
        }
    }

    public static char GetDefaultInput(PlayerAction action)
    {
        switch (action)
        {
            case PlayerAction.NONE:
                return '\0';
            case PlayerAction.MOVE_LEFT:
                return 'a';
            case PlayerAction.MOVE_RIGHT:
                return 'd';
            case PlayerAction.MOVE_UP:
                return 'w';
            case PlayerAction.MOVE_DOWN:
                return 's';
            case PlayerAction.WAIT:
                return '.';
            case PlayerAction.PICK_UP_ITEMS:
                return 'g';
            case PlayerAction.DROP_ITEMS:
                return 'v';
            case PlayerAction.OPEN_INVENTORY:
                return 'i';
            case PlayerAction.APPLY:
                return 'q';
            case PlayerAction.EQUIP:
                return 'e';
            case PlayerAction.UNEQUIP:
                return 'r';
            case PlayerAction.CAST_SPELL:
                return 'z';
            case PlayerAction.FIRE:
                return 'f';
            case PlayerAction.ASCEND:
                return '<';
            case PlayerAction.DESCEND:
                return '>';
            case PlayerAction.AUTO_EXPLORE:
                return 'o';
            case PlayerAction.DEV_CHEAT:
                return '`';
            default:
                return '\0';
        }
    }

    public static void ProcessInput()
    {
        //Clear, and add from input string
        UpdateFrameActions();

        //Check if we have any self ligatures
        //We can early out if this is true, since we aren't combining any more.
        if (ProcessLigatures(frameActions))
        {
            //Push old inputs
            PushUnprocessedInput();

            //Add new inputs to processing queue, then flush.
            processingActions.AddRange(frameActions);
            processingInput = Input.inputString;
            PushUnprocessedInput();
            return;
        }

        //Push empty input to keep things moving
        if (frameActions.Count == 0)
        {
            frameActions.Add(PlayerAction.NONE);
        }

        if (HasUnprocessedInput())
        {
            //Check for combines!
            ProcessCombinedLigatures(processingActions, frameActions);

            PushUnprocessedInput();

            if (frameActions.Count > 0)
            {
                processingActions.AddRange(frameActions);
                processingInput = Input.inputString;
                lastInputAdded = Time.time;
            }
        }
        else
        {
            processingActions.AddRange(frameActions);
            processingInput = Input.inputString;
            lastInputAdded = Time.time;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateActionsDictionary();
        GenerateLigatures();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            ProcessInput();
        }
        
        if (HasUnprocessedInput() && Time.time - lastInputAdded > maxCombineDelay)
        {
            PushUnprocessedInput();
        }

        //Extensive check that we probably don't want in the built game
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (actions.Count != inputs.Count)
        {
            Debug.LogError("Actions and input queues got misaligned.");
        }
        #endif
    }

}
