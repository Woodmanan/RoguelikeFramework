using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonsterAI : ActionController
{
    public Query fleeQuery;
    public Query approachQuery;
    //The main loop for monster AI! This assumes 
    public override IEnumerator DetermineAction()
    {
        if (monster.view == null)
        {
            Debug.LogError("Monster did not have a view available! If this happened during real gameplay, we have a problem. Eating its turn to be safe.");
            monster.UpdateLOS();
            nextAction = new WaitAction();
            yield break;
        }
        monster.view.CollectEntities(Map.current);

        List<Monster> enemies = monster.view.visibleMonsters.Where(x => (x.faction & monster.faction) == 0).ToList();

        if (enemies.Count == 0)
        {
            //Standard behavior

            //TODO: If someone is offering actions, look at their action and maybe take it

            //Else, try to go exploring!

            //Else, just wait?
            Debug.Log("Monster is just waiting!");
            nextAction = new WaitAction();
            yield break;
        }
        else
        {
            //We're majorly in combat!

            //Options
            //0 - Flee (Default)
            //1 - Fight
            //2 - Spell

            float flee = fleeQuery.Evaluate(monster, monster.view.visibleMonsters, null, null);
            float approach = approachQuery.Evaluate(monster, monster.view.visibleMonsters, null, null);
            (int spellIndex, float spellValue) = (-1, -1);
            if (monster.abilities) (spellIndex, spellValue) = monster.abilities.GetBestAbility();

            //TODO: Determine if any items would be a good fit
            List<int> bestChoice = new List<int> { 0 };
            float bestVal = flee;

            //Fighting check
            if (approach > bestVal)
            {
                bestChoice.Clear();
                bestChoice.Add(1);
                bestVal = approach;
            }
            if (approach == bestVal)
            {
                bestChoice.Add(1);
            }

            //Ability check
            if (spellValue > bestVal)
            {
                bestChoice.Clear();
                bestChoice.Add(2);
                bestVal = spellValue;
            }
            if (spellValue == bestVal)
            {
                bestChoice.Add(2);
            }

            int choice = bestChoice[Random.Range(0, bestChoice.Count)];
            switch (bestChoice[Random.Range(0, bestChoice.Count)])
            {
                case 0:
                    Debug.Log("Should flee!!");
                    nextAction = new FleeAction();
                    break;
                case 1:
                    Debug.Log("Should Attack!!");
                    enemies = enemies.OrderBy(x => Vector2Int.Distance(monster.location, x.location)).ToList();
                    nextAction = new PathfindAction(enemies[0].location);
                    break;
                case 2:
                    Debug.Log($"Should use spell {monster.abilities[spellIndex].displayName}");
                    nextAction = new WaitAction();
                    break;
                default:
                    Debug.LogError($"Can't make choice {choice}, so waiting instead");
                    nextAction = new WaitAction();
                    break;
            }
        }
    }
}
