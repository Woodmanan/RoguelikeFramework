using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;


public class InputTracking : MonoBehaviour
{
    //Public functions for accessing all of this
    public static Queue<PlayerAction> actions = new Queue<PlayerAction>();
    public static Queue<string> inputs = new Queue<string>();

    public static bool HasNextAction()
    {
        return actions.Count > 0;
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

    public static Tuple<PlayerAction, string> PopNextPair()
    {
        if (HasNextAction())
        {
            PlayerAction act = actions.Dequeue();
            string inp = inputs.Dequeue();
            return new Tuple<PlayerAction, string>(act, inp);
        }
        else
        {
            return new Tuple<PlayerAction, string>(PlayerAction.NONE, "");
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
        actions.Enqueue(act);
        inputs.Enqueue(Input.inputString);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Extensive check that we probably don't want in the built game
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (actions.Count != inputs.Count)
        {
            Debug.LogError("Actions and input queues got misaligned.");
        }
        #endif


        bool addedAction = false;

        //Add movements
        if (Left())
        {
            if (Up())
            {
                PushAction(PlayerAction.MOVE_UP_LEFT);
            }
            else if (Down())
            {
                PushAction(PlayerAction.MOVE_DOWN_LEFT);
            }
            else
            {
                PushAction(PlayerAction.MOVE_LEFT);
            }

            addedAction = true;
        }
        else if (Right())
        {
            if (Up())
            {
                PushAction(PlayerAction.MOVE_UP_RIGHT);
            }
            else if (Down())
            {
                PushAction(PlayerAction.MOVE_DOWN_RIGHT);
            }
            else
            {
                PushAction(PlayerAction.MOVE_RIGHT);
            }

            addedAction = true;
        }
        else if (Down())
        {
            PushAction(PlayerAction.MOVE_DOWN);
            addedAction = true;
        }
        else if (Up())
        {
            PushAction(PlayerAction.MOVE_UP);
            addedAction = true;
        }
        else if (UpLeft())
        {
            PushAction(PlayerAction.MOVE_UP_LEFT);
            addedAction = true;
        }
        else if (UpRight())
        {
            PushAction(PlayerAction.MOVE_UP_RIGHT);
            addedAction = true;
        }
        else if (DownLeft())
        {
            PushAction(PlayerAction.MOVE_DOWN_LEFT);
            addedAction = true;
        }
        else if (DownRight())
        {
            PushAction(PlayerAction.MOVE_DOWN_RIGHT);
            addedAction = true;
        }
        else if (Drop())
        {
            PushAction(PlayerAction.DROP_ITEMS);
            addedAction = true;
        }
        else if (PickUp())
        {
            PushAction(PlayerAction.PICK_UP_ITEMS);
            addedAction = true;
        }
        else if (OpenInventory())
        {
            PushAction(PlayerAction.OPEN_INVENTORY);
            addedAction = true;
        }
        else if (Equip())
        {
            PushAction(PlayerAction.EQUIP);
            addedAction = true;
        }
        else if (Unequip())
        {
            PushAction(PlayerAction.UNEQUIP);
            addedAction = false;
        }
        else if (Apply())
        {
            PushAction(PlayerAction.APPLY);
            addedAction = true;
        }
        else if (CastSpell())
        {
            PushAction(PlayerAction.CAST_SPELL);
            addedAction = true;
        }
        else if (Fire())
        {
            PushAction(PlayerAction.FIRE);
            addedAction = true;
        }
        else if (GoUp())
        {
            PushAction(PlayerAction.ASCEND);
            addedAction = true;
        }
        else if (GoDown())
        {
            PushAction(PlayerAction.DESCEND);
            addedAction = true;
        }
        else if (Escaping())
        {
            PushAction(PlayerAction.ESCAPE_SCREEN);
            addedAction = true;
        }
        else if (Accept())
        {
            PushAction(PlayerAction.ACCEPT);
            addedAction = true;
        }
        else if (Wait())
        {
            PushAction(PlayerAction.WAIT);
            addedAction = true;
        }
        else if (Input.inputString != "") //FINAL CHECK! Use this to add empty input to the buffer for character checks. (MUST BE LAST CHECK)
        {
            PushAction(PlayerAction.NONE);
            addedAction = false;
        }
    }

    //WASD has been removed here
    private bool Left()
    {
        return (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4));
    }

    private bool Right()
    {
        return (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6));
    }

    private bool Up()
    {
        return (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8));
    }

    private bool Down()
    {
        return (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2));
    }

    private bool UpLeft()
    {
        return (!UIController.WindowsOpen && (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Keypad7)));
    }

    private bool UpRight()
    {
        return (!UIController.WindowsOpen && (Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.Keypad9)));
    }

    private bool DownLeft()
    {
        return (!UIController.WindowsOpen && (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Keypad1)));
    }

    private bool DownRight()
    {
        return (!UIController.WindowsOpen && (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.Keypad3)));
    }

    private bool Drop()
    {
        return Input.GetKeyDown(KeyCode.D);
    }

    private bool PickUp()
    {
        return (Input.GetKeyDown(KeyCode.Comma) && !Input.GetKey(KeyCode.LeftShift)) || Input.GetKeyDown(KeyCode.G);
    }

    private bool OpenInventory()
    {
        return Input.GetKeyDown(KeyCode.I);
    }

    private bool Equip()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    //TODO: Revisit this sequence of inputs. As of right now PlayerAction.MOVE_UP_RIGHT is the one keyed to this.
    private bool Unequip()
    {
        return Input.GetKeyDown(KeyCode.R);
    }

    private bool Escaping()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    private bool Accept()
    {
        return Input.GetKeyDown(KeyCode.Return);
    }

    //Currently Q for convention; Apply is the backend stuff, these will probably just be quaffables
    private bool Apply()
    {
        return Input.GetKeyDown(KeyCode.Q); //It's Q, for 'Qiswhatyouhittoapply'
    }

    private bool CastSpell()
    {
        return Input.GetKeyDown(KeyCode.A);
    }

    private bool Fire()
    {
        return Input.GetKeyDown(KeyCode.F);
    }

    private bool Wait()
    {
        return Input.GetKeyDown(KeyCode.Period) && !Input.GetKey(KeyCode.LeftShift);
    }

    private bool GoUp()
    {
        return Input.GetKeyDown(KeyCode.Comma) && Input.GetKey(KeyCode.LeftShift);
    }

    private bool GoDown()
    {
        return Input.GetKeyDown(KeyCode.Period) && Input.GetKey(KeyCode.LeftShift);
    }

}
