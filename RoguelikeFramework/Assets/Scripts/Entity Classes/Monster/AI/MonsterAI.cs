using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Priority_Queue;
using static Resources;

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
    public Query fightQuery;
    [HideInInspector] public bool isInBattle = false;

    public float interactionRange;
    public bool ranged = false;
    public int minRange = 0;

    public int intelligence = 2;
    int currentTries = 0;

    float loseDistance = 20;

    public Monster lastEnemy;

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

        if (lastEnemy && (monster.location.GameDistance(lastEnemy.location) > loseDistance || currentTries == 0))
        {
            lastEnemy = null;
            currentTries = 0;
        }

        if (enemies.Count == 0)
        {
            //Standard behavior
            isInBattle = false;


            //1 - Take an existing interaction
            (InteractableTile tile, float interactableCost) = GetInteraction(false, interactionRange);
            choices.Enqueue(new IntNode(1), 1f - interactableCost);

            //2 - Chase someone who we don't see anymore
            if (lastEnemy && currentTries > 0)
            {
                choices.Enqueue(new IntNode(2), 1f - .8f);
            }

            //Wait for 60 turns on avg, then go explore
            if (UnityEngine.Random.Range(0, 60) == 0)
            {
                choices.Enqueue(new IntNode(3), 1f - .7f);
            }


            //4 - Wait
            choices.Enqueue(new IntNode(4), 1f - .1f);

            //5 - Heal up
            choices.Enqueue(new IntNode(5), (monster.baseStats[HEALTH] / monster.currentStats[MAX_HEALTH]));

            switch (choices.First.value)
            {
                case 1:
                    nextAction = tile.GetAction();
                    break;
                case 2:
                    nextAction = new PathfindAction(lastEnemy.location);
                    currentTries--;
                    break;
                case 3:
                    Vector2Int rand = Map.current.GetRandomWalkableTile();
                    nextAction = new PathfindAction(rand);
                    break;
                case 4:
                    nextAction = new WaitAction();
                    break;
                case 5:
                    nextAction = new MonsterRest();
                    break;
                default:
                    Debug.LogError($"Monster does not have non-combat action set for choice {choices.First.value}", monster);
                    break;
            }
            yield break;
        }
        else
        {
            //We're majorly in combat!
            isInBattle = true;
            //TODO: Make offered actions available to combat monsters for specific actions

            //Options
            //0 - Flee (Default)
            //1 - Fight
            //2 - Spell
            //3 - Some offered action

            float flee = fleeQuery.Evaluate(monster, monster.view.visibleMonsters, null, null);
            float approach = fightQuery.Evaluate(monster, monster.view.visibleMonsters, null, null);

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
                    enemies = enemies.OrderBy(x => monster.location.GameDistance(x.location)).ToList();
                    int dist = Mathf.RoundToInt(monster.location.GameDistance(enemies[0].location) + .5f);
                    if (ranged)
                    {
                        if (dist <= minRange)
                        {
                            nextAction = new RangedAttackAction();
                        }
                        else
                        {
                            nextAction = new PathfindAction(enemies[0].location);
                        }
                    }
                    else
                    {
                        nextAction = new PathfindAction(enemies[0].location);
                    }

                    lastEnemy = enemies[0];
                    currentTries = intelligence;
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
            if ((targeting.options & TargetTags.RECOMMNEDS_ALLY_TARGET) > 0)
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
                    targets = targets.OrderBy(x => x.baseStats[HEALTH] / x.currentStats[MAX_HEALTH]).ToList();
                    break;
                case TargetPriority.HIGHEST_HEALTH:
                    targets = targets.OrderByDescending(x => x.baseStats[HEALTH] / x.currentStats[MAX_HEALTH]).ToList();
                    break;
            }

            for (int i = 0; i < targeting.numPoints; i++)
            {
                int index = i;
                if ((targeting.options & TargetTags.RETARGETS_SAME_MONSTER) > 0)
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

    public override void Setup()
    {
        GetComponent<Equipment>().OnEquipmentAdded += UpdateRanged;
    }

    void UpdateRanged()
    {
        List<EquipmentSlot> slots = GetComponent<Equipment>().equipmentSlots.FindAll(x => x.active && x.equipped.held[0].type == ItemType.RANGED_WEAPON);
        ranged = slots.Count > 0;
        if (ranged)
        {
            minRange = slots.Min(x => x.equipped.held[0].ranged.targeting.range);
        }
    }
}