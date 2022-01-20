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
            nextAction = new WaitAction();
            yield break;
        }
        else
        {
            //We're majorly in combat!
            //TODO: Make offered actions available to combat monsters for specific actions

            //Options
            //0 - Flee (Default)
            //1 - Fight
            //2 - Spell
            //3 - Some offered action

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
                    nextAction = new FleeAction();
                    break;
                case 1:
                    enemies = enemies.OrderBy(x => Vector2Int.Distance(monster.location, x.location)).ToList();
                    nextAction = new PathfindAction(enemies[0].location);
                    break;
                case 2:
                    nextAction = new AbilityAction(spellIndex);
                    break;
                default:
                    Debug.LogError($"Can't make choice {choice}, so waiting instead");
                    nextAction = new WaitAction();
                    break;
            }
        }
    }

    public override IEnumerator DetermineTarget(Targeting targeting, BoolDelegate setValidityTo)
    {
        if (targeting.BeginTargetting(monster.location, monster.view) && targeting.range > 0)
        {
            List<Monster> targets;
            if ((targeting.tags & TargetTags.RECOMMNEDS_ALLY_TARGET) > 0)
            {
                targets = monster.view.visibleMonsters.FindAll(x => !x.IsEnemy(monster) && x != monster);
            }
            else
            {
                targets = monster.view.visibleMonsters.FindAll(x => x.IsEnemy(monster));
            }

            //TODO: Make this use a new vector distance function, instead of just copying the targeting code.
            targets = targets.FindAll(x =>
            {
                return !(Mathf.Abs(x.location.x - monster.location.x) > targeting.range
                      || Mathf.Abs(x.location.y - monster.location.y) > targeting.range);
            });

            //TODO: Make weakest and strongest case on combat power, not just % health.
            switch (targeting.targetPriority)
            {
                case TargetPriority.NEAREST:
                    targets = targets.OrderBy(x => x.DistanceFrom(monster)).ToList();
                    break;
                case TargetPriority.FARTHEST:
                    targets = targets.OrderByDescending(x => x.DistanceFrom(monster)).ToList();
                    break;
                case TargetPriority.LOWEST_HEALTH:
                    targets = targets.OrderBy(x => ((float) x.resources.health) / x.stats.resources.health).ToList();
                    break;
                case TargetPriority.HIGHEST_HEALTH:
                    targets = targets.OrderByDescending(x => ((float)x.resources.health) / x.stats.resources.health).ToList();
                    break;
            }

            for (int i = 0; i < targeting.numPoints; i++)
            {
                int index = i;
                if ((targeting.tags & TargetTags.RETARGETS_SAME_MONSTER) > 0)
                {
                    index = 0;
                }
                targeting.MoveTarget(targets[index].location);
                if (!targeting.LockPoint())
                {
                    Debug.LogError("Something has gone very wrong. Monster was unable to target correctly.");
                    yield return null; //Try to prevent a freeze from occuring, if the monster keeps trying and failing.
                }
            }

            //We're done! Signal to outside systems that we finished correctly.
            setValidityTo(targeting.isFinished);
        }
        else
        {
            if (!targeting.LockPoint()) //UI thinks we don't even need it, so just skip this whole thing
            {
                Debug.LogError("Targeting that skips can NOT have more than one point! This is unecessary behaviour, and must be fixed immediately to maintain invariants.");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
                yield return null; //Prevents a full freeze, hopefully.
                setValidityTo(false);
            }
            else
            {
                setValidityTo(true);
            }
        }
    }
}
