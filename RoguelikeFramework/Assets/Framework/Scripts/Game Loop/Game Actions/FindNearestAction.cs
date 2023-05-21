using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindNearestAction : GameAction
{
    //Constuctor for the action; must include caller!
    List<Vector2Int> goals;
    public FindNearestAction(List<Vector2Int> goals)
    {
        //Construct me! Assigns caller by default in the base class
        this.goals = goals;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        if (goals.Count == 0)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (caller != Player.player)
            {
                Debug.LogError("Monster should never take movement action with NO goals!");
                caller.energy -= 10;
            }
            #endif
            yield break;
        }

        Path path = Pathfinding.CreateDjikstraWithAstar(caller.location, goals);

        if (path.Cost() < 0)
        {
            Debug.LogWarning("Monster cannot find path to location from here! Aborting.");
            yield break;
        }

        while (path.Count() > 0)
        {
            Vector2Int next = path.Pop();
            MoveAction act = new MoveAction(next);

            caller.UpdateLOS();

            if (caller.view.visibleEnemies.Count > 0)
            {
                Debug.Log($"Monster came into sight, so don't auto move!");
                yield break;
            }

            act.Setup(caller);
            while (act.action.MoveNext())
            {
                yield return act.action.Current;
            }

            //yield return new WaitForSeconds(.05f);

            yield return GameAction.StateCheck;
        }
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}