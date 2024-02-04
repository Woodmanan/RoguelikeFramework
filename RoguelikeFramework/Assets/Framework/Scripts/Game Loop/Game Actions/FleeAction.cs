using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FleeAction : GameAction
{
    //Constuctor for the action; must include caller!
    public FleeAction()
    {
        //Construct me! Assigns caller by default in the base class
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        while (true)
        {
            List<RogueHandle<Monster>> enemies = caller[0].view.visibleEnemies;
            if (enemies.Count == 0)
            {
                Debug.Log("Monster is fleeing without seeing anyone. Resting instead.");
                yield return SubAction(new RestAction());
                yield break;
            }

            float[,] fleeMap = Pathfinding.CreateFleeMap(enemies.Select(x => x[0].location).ToList());

            while (true)
            {
                Vector2Int next = nextSpot(caller[0].location, fleeMap);
                if (next == caller[0].location)
                {
                    Debug.Log($"{caller[0].GetLocalizedName()} has been cornered - stopping flee mode.");

                    enemies = caller[0].view.visibleEnemies;
                    if (enemies.Count == 0)
                    {
                        yield break;
                    }

                    RogueHandle<Monster> target = enemies[0];

                    yield return SubAction(new AttackAction(target));
                    yield break;
                }
                else
                {
                    yield return SubAction(new MoveAction(next));
                    yield return GameAction.StateCheck;
                }
            }
        }
    }

    Vector2Int nextSpot(Vector2Int current, float[,] fleeMap)
    {
        float currentCost = fleeMap[current.x, current.y];
        Vector2Int next = current;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                bool isCorner = (i * j) != 0;
                if (Map.space == MapSpace.Manhattan && isCorner)
                {
                    continue;
                }

                Vector2Int newCheck = current + new Vector2Int(i, j);

                if (newCheck.x < 0 || newCheck.x >= fleeMap.GetLength(0) || newCheck.y < 0 || newCheck.y >= fleeMap.GetLength(0))
                {
                    continue;
                }

                //Don't try to go through walls.
                if (Map.current.BlocksMovement(newCheck)) continue;

                float newCost = fleeMap[newCheck.x, newCheck.y];

                if (newCost < currentCost)
                {
                    currentCost = newCost;
                    next = newCheck;
                }
            }
        }

        return next;
    }

    public override string GetDebugString()
    {
        return "Flee Action";
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}