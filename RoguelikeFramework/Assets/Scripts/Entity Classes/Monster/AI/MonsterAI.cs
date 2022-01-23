using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Priority_Queue;
public class IntNode : FastPriorityQueueNode
{
    public int value;

    public IntNode(int value)
    {
        this.value = value;
    }
}


public class MonsterAI : ActionController
{
    public Query fleeQuery;
    public Query approachQuery;

    public float interactionRange;
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

        FastPriorityQueue<IntNode> choices = new FastPriorityQueue<IntNode>(300);

        if (enemies.Count == 0)
        {
            //Standard behavior

            //1 - Take an existing interaction
            (InteractableTile tile, float interactableCost) = GetInteraction(false, interactionRange);
            choices.Enqueue(new IntNode(1), 1f - interactableCost);

            //Else, try to go exploring!

            //3 - Wait
            choices.Enqueue(new IntNode(3), 1f - .1f);

            switch (choices.First.value)
            {
                case 1:
                    nextAction = tile.GetAction();
                    break;
                case 3:
                    nextAction = new WaitAction();
                    break;
                default:
                    Debug.LogError($"Monster does not have action set for choice {choices.First.value}");
                    break;
            }
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
            (InteractableTile tile, float interactableCost) = GetInteraction(false, interactionRange);
            

            choices.Enqueue(new IntNode(0), 1f - flee);
            choices.Enqueue(new IntNode(1), 1f - approach);
            choices.Enqueue(new IntNode(2), 1f - spellValue);
            choices.Enqueue(new IntNode(3), 1f - interactableCost);


            switch (choices.First.value)
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
                case 3:
                    nextAction = tile.GetAction();
                    break;
                default:
                    Debug.LogError($"Can't make choice {choices.First.value}, so waiting instead");
                    nextAction = new WaitAction();
                    break;
            }
        }
    }

    public (InteractableTile, float) GetInteraction(bool isInCombat, float distanceCutoff)
    {
        List<InteractableTile> tiles = Map.current.interactables.FindAll(x => x.FilterByCombat(isInCombat) && Vector2Int.Distance(monster.location, x.location) <= distanceCutoff);
        List<(InteractableTile, float)> pairs = tiles.Select(x => (x, x.useQuery.Evaluate(monster, monster.view.visibleMonsters, null, null)))
                                                     .OrderBy(x => Vector2Int.Distance(monster.location, x.Item1.location))
                                                     .OrderByDescending(x => x.Item2)
                                                     .ToList();
        if (pairs.Count == 0)
        {
            return (null, -1f);
        }
        return pairs[0];
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
