using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevelAction : GameAction
{
    bool up;

    //Constuctor for the action; must include caller!
    public ChangeLevelAction(bool up)
    {
        this.up = up;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        Debug.Log("Moving levels!");
        Stair stair = Map.current.GetTile(caller[0].location) as Stair;
        if (stair && !stair.locked)
        {
            if (stair.up ^ up)
            {
                yield break;
                /*bool keepGoing = false;
                UIController.singleton.OpenConfirmation($"<color=\"black\">Are you sure you want to go {(up ? "up" : "down")}?", (b) => keepGoing = b);
                yield return new WaitUntil(() => !UIController.WindowsOpen);
                if (!keepGoing) yield break;*/
            }
            LevelLoader.singleton.ConfirmConnection(stair.connection);
            int LevelToChange = stair.GetMatchingLevel();
            if (LevelToChange >= 0)
            {
                GameController.singleton.MoveToLevel(stair.GetMatchingLevel());
                caller[0].energy -= 100;
            }
            else
            {
                Debug.Log("Console: This gate is locked. Retrive the amulet of Yendor!");
            }


        }
        else
        {
            Debug.Log($"Console: You cannot go {(up ? "up" : "down")} here!");
        }
    }

    public override string GetDebugString()
    {
        return "Change Level Action";
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
