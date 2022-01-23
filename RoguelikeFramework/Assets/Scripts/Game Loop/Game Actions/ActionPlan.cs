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
            actions[i].Setup(caller);
            while (actions[i].action.MoveNext())
            {
                yield return actions[i].action.Current;
            }

            if (!actions[i].successful)
            {
                yield return GameAction.Abort;
            }
        }
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
