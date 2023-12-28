using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPlan : GameAction
{
    List<GameAction> actions = new List<GameAction>();

    public ActionPlan() { }

    //Constuctor for the action
    public ActionPlan(List<GameAction> actions)
    {
        this.actions.AddRange(actions);
    }

    public void AddAction(GameAction action)
    {
        actions.Add(action);
    }

    //Take all actions, and just run them in order. If someone gives up, we all give up.
    public override IEnumerator TakeAction()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            GameAction subAction = actions[i];
            yield return SubAction(subAction);

            if (!actions[i].successful)
            {
                yield return GameAction.Abort;
            }
        }
    }

    public override string GetDebugString()
    {
        string outputString = $"Action Plan with {actions.Count} actions:";
        foreach (GameAction action in actions)
        {
            outputString += $"\n\t{action.GetDebugString()}";
        }
        return outputString;
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
