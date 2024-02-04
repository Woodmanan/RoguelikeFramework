using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Resources;


public class MonsterAI : ActionController
{
    public Query fleeQuery;
    public Query fightQuery;
    [HideInInspector] public bool isInBattle = false;

    public bool willExplore = true;
    public float interactionRange;
    public bool ranged = false;
    public int minRange = 0;

    public int intelligence = 2;
    int currentTries = 0;

    float loseDistance = 20;

    [NonSerialized]
    public RogueHandle<Monster> lastEnemy = RogueHandle<Monster>.Default;
    [NonSerialized]
    public RogueHandle<Monster> leader = RogueHandle<Monster>.Default;
    PriorityQueue<int> choices = new PriorityQueue<int>(20);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public bool debugFreezeMonster = false;
#else
    const bool debugFreezeMonster = false;
#endif

    //The main loop for monster AI! This assumes 
    public override IEnumerator DetermineAction()
    { 
        if (debugFreezeMonster)
        {
            nextAction = new WaitAction();
            yield break;
        }

        if (monster[0].view == null)
        {
            Debug.LogError("Monster did not have a view available! If this happened during real gameplay, we have a problem. Eating its turn to be safe.");
            monster[0].UpdateLOS();
            nextAction = new WaitAction();
            yield break;
        }
        monster[0].view.CollectEntities(Map.current, monster);

        List<RogueHandle<Monster>> enemies = monster[0].view.visibleEnemies;

        choices.Clear();

        //Backup leader - if yours dies, go up the credit chain
        while (leader.IsValid() && leader[0].IsDead())
        {
            leader = leader[0].credit;
        }

        if (lastEnemy && (monster[0].location.GameDistance(lastEnemy[0].location) > loseDistance || currentTries == 0))
        {
            lastEnemy = RogueHandle<Monster>.Default;
            currentTries = 0;
        }

        if (enemies.Count == 0)
        {
            //Standard behavior
            isInBattle = false;


            //1 - Take an existing interaction
            (InteractableTile tile, float interactableCost) = GetInteraction(false, interactionRange);
            choices.Enqueue(1, 1f - interactableCost);

            //2 - Chase someone who we don't see anymore
            if (lastEnemy && currentTries > 0)
            {
                choices.Enqueue(2, 1f - .8f);
            }

            if (!leader.IsValid())
            {
                //Wait for 120 turns on avg, then go explore
                if (willExplore && UnityEngine.Random.Range(0, 120) == 0)
                {
                    choices.Enqueue(3, 1f - .7f);
                }
            }
            else
            {
                //6 - Follow the leader
                if (monster[0].location.GameDistance(leader[0].location) > 4 || UnityEngine.Random.Range(0, 30) == 0)
                {
                    choices.Enqueue(6, 1f - 0.95f);
                }
            }

            //4 - Wait
            if (monster[0].energyPerStep == 0)
            {
                choices.Enqueue(4, 1f - 1.1f);
            }
            else
            {
                //Monster with lower health want to rest more - shouldn't be wandering around
                float restVal = Mathf.Min(.9f, (monster[0].baseStats[HEALTH] / monster[0].currentStats[MAX_HEALTH]));
                choices.Enqueue(4, restVal);
            }
            

            int finalChoice = choices.Dequeue();

            switch (finalChoice)
            {
                case 1:
                    nextAction = tile.GetAction();
                    break;
                case 2:
                    nextAction = new PathfindAction(lastEnemy[0].location);
                    currentTries--;
                    break;
                case 3:
                    Vector2Int rand = Map.current.GetRandomWalkableTile();
                    nextAction = new PathfindAction(rand);
                    break;
                case 4:
                    nextAction = new WaitAction();
                    break;
                case 6: //Follow the leader!
                    Vector2Int followPosition = Map.current.GetRandomWalkableTileInSight(leader, 2);
                    if (followPosition == new Vector2Int(-1, -1))
                    {
                        followPosition = Map.current.GetRandomWalkableTileInSight(leader, leader[0].visionRadius);
                    }
                    nextAction = new PathfindAction(followPosition);
                    break;
                default:
                    Debug.LogError($"Monster does not have non-combat action set for choice {finalChoice}", monster[0].unity);
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
            //4 - Wait

            float flee = fleeQuery.Evaluate(monster, null, null);
            float approach = fightQuery.Evaluate(monster, null, null);

            (int spellIndex, float spellValue) = (-1, -1);
            if (monster[0].abilities) (spellIndex, spellValue) = monster[0].abilities.GetBestAbility();

            (InteractableTile tile, float interactableCost) = GetInteraction(false, interactionRange);

            choices.Enqueue(0, 1f - flee);
            choices.Enqueue(1, 1f - approach);
            choices.Enqueue(2, 1f - spellValue);
            choices.Enqueue(3, 1f - interactableCost);

            if (monster[0].energyPerStep == 0)
            {
                choices.Enqueue(4, 1f - 1.1f);
            }

            int finalChoice = choices.Dequeue();
            switch (finalChoice)
            {
                case 0:
                    nextAction = new FleeAction();
                    break;
                case 1:
                    enemies = enemies.OrderBy(x => monster[0].location.GameDistance(x[0].location)).ToList();
                    int dist = Mathf.RoundToInt(monster[0].location.GameDistance(enemies[0][0].location) + .5f);
                    if (ranged)
                    {
                        if (dist <= minRange)
                        {
                            nextAction = new RangedAttackAction();
                        }
                        else
                        {
                            nextAction = new PathfindAction(enemies[0][0].location);
                        }
                    }
                    else
                    {
                        nextAction = new PathfindAction(enemies[0][0].location);
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
                case 4:
                    nextAction = new WaitAction();
                    break;
                default:
                    Debug.LogError($"Can't make choice {finalChoice}, so waiting instead");
                    nextAction = new WaitAction();
                    break;
            }
        }
    }

    public (InteractableTile, float) GetInteraction(bool isInCombat, float distanceCutoff)
    {
        List<InteractableTile> tiles = Map.current.interactables.FindAll(x => x.FilterByCombat(isInCombat) && Vector2Int.Distance(monster[0].location, x.location) <= distanceCutoff);
        List<(InteractableTile, float)> pairs = tiles.Select(x => (x, x.useQuery.Evaluate(monster, null, null)))
                                                     .OrderBy(x => Vector2Int.Distance(monster[0].location, x.Item1.location))
                                                     .OrderByDescending(x => x.Item2)
                                                     .ToList();
        if (pairs.Count == 0)
        {
            return (null, -1f);
        }
        return pairs[0];
    }

    public override IEnumerator DetermineTarget(Targeting targeting, BoolDelegate setValidityTo, Func<RogueHandle<Monster>, bool> TargetCheck = null)
    {
        if (targeting.BeginTargetting(monster[0].location, monster[0].view, TargetCheck) && targeting.range > 0)
        {
            List<RogueHandle<Monster>> targets;
            if ((targeting.options & TargetTags.RECOMMNEDS_ALLY_TARGET) > 0)
            {
                targets = monster[0].view.visibleFriends;
            }
            else
            {
                targets = monster[0].view.visibleEnemies;
            }

            //TODO: Make this use a new vector distance function, instead of just copying the targeting code.
            targets = targets.FindAll(x =>
            {
                return !(Mathf.Abs(x[0].location.x - monster[0].location.x) > targeting.range
                      || Mathf.Abs(x[0].location.y - monster[0].location.y) > targeting.range);
            });

            //TODO: Make weakest and strongest case on combat power, not just % health.
            switch (targeting.targetPriority)
            {
                case TargetPriority.NEAREST:
                    targets = targets.OrderBy(x => x[0].DistanceFrom(monster)).ToList();
                    break;
                case TargetPriority.FARTHEST:
                    targets = targets.OrderByDescending(x => x[0].DistanceFrom(monster)).ToList();
                    break;
                case TargetPriority.LOWEST_HEALTH:
                    targets = targets.OrderBy(x => x[0].baseStats[HEALTH] / x[0].currentStats[MAX_HEALTH]).ToList();
                    break;
                case TargetPriority.HIGHEST_HEALTH:
                    targets = targets.OrderByDescending(x => x[0].baseStats[HEALTH] / x[0].currentStats[MAX_HEALTH]).ToList();
                    break;
            }

            for (int i = 0; i < targeting.numPoints; i++)
            {
                int index = i;
                if ((targeting.options & TargetTags.RETARGETS_SAME_MONSTER) > 0)
                {
                    index = 0;
                }
                targeting.MoveTarget(targets[index][0].location);
                if (targeting.IsValid())
                {
                    if (!targeting.LockPoint())
                    {
                        Debug.LogError("Something has gone very wrong. Monster was unable to target correctly.");
                        yield return null; //Try to prevent a freeze from occuring, if the monster keeps trying and failing.
                    }
                }
                else
                {
                    Debug.Log("Monster could not shoot, and probably needs to pick something else to do!");
                    //monster.renderer.color = Color.red;
                    monster[0].energy -= 100;
                    break;
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
        UpdateRanged();
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

    public void SetToFollow(RogueHandle<Monster> target)
    {
        SetToFollow(target, intelligence);
    }

    public void SetToFollow(RogueHandle<Monster> target, int numTries)
    {
        lastEnemy = target;
        currentTries = numTries;
    }

    public void Clear()
    {
        lastEnemy = RogueHandle<Monster>.Default;
        leader = RogueHandle<Monster>.Default;
    }
}