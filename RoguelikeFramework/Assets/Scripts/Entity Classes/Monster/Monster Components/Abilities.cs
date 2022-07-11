using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//TODO: Come up with a better name for this?
public class Abilities : MonoBehaviour
{
    Monster connectedTo;
    List<Ability> abilities = new List<Ability>();

    public int Count
    {
        get { return abilities.Count; }
    }

    // Start is called before the first frame update
    void Start()
    {
        connectedTo = GetComponent<Monster>();
    }

    public void RegenerateAbilities()
    {
        foreach (Ability a in abilities)
        {
            a.Cleanup();
            a.RegenerateStats(connectedTo);
        }
    }

    public void CheckAvailability()
    {
        RegenerateAbilities();
        foreach (Ability a in abilities)
        {
            a.CheckAvailable(connectedTo);
        }
    }

    public Ability this[int index]
    {
        get { return abilities[index]; }
    }
    

    public void AddAbility(Ability abilityToAdd)
    {
        abilities.Add(abilityToAdd.Instantiate());
    }

    public void OnTurnEndGlobal()
    {
        foreach (Ability a in abilities)
        {
            a.ReduceCooldown();
            a.Cleanup();
        }
    }

    public (int, float) GetBestAbility()
    {
        List<int> bestIndex = new List<int> { -1 };
        float bestValue = -1;

        //Construct allies, sort by distance.
        List<Monster> allies = connectedTo.view.visibleMonsters.FindAll(x => x != connectedTo && !x.IsEnemy(connectedTo));
        List<int> allyDistances = allies.Select(x =>
        {
            return Mathf.Max(Mathf.Abs(x.location.x - connectedTo.location.x),
                             Mathf.Abs(x.location.y - connectedTo.location.y));
        }).ToList();


        List<Monster> enemies = connectedTo.view.visibleMonsters.FindAll(x=> x.IsEnemy(connectedTo));
        List<int> enemyDistances = enemies.Select(x =>
        {
            return Mathf.Max(Mathf.Abs(x.location.x - connectedTo.location.x),
                             Mathf.Abs(x.location.y - connectedTo.location.y));
        }).ToList();

        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i].castable)
            {
                if (abilities[i].targeting.targetingType != TargetType.SELF)
                {
                    //TODO: Rework this to account for both self targets, and for mixed types of abilities
                    if ((abilities[i].targeting.options & TargetTags.RECOMMNEDS_ALLY_TARGET) > 0)
                    {
                        if (allyDistances.Count == 0) continue; //Quit if no allies
                        else if (abilities[i].targeting.range == 0 && allyDistances[0] > abilities[i].targeting.radius) continue; //Quit if no allies in radius
                        else if (allyDistances[0] > abilities[i].targeting.range) continue; //Quit if no allies in range
                    }
                    else
                    {
                        if (enemyDistances.Count == 0)
                        {
                            continue;
                        }
                        else if (abilities[i].targeting.range == 0 && enemyDistances[0] > abilities[i].targeting.radius)
                        {
                            continue; //Quit if no allies in radius
                        }
                        else if (abilities[i].targeting.range != 0 && enemyDistances[0] > abilities[i].targeting.range)
                        {
                            continue; //Quit if no allies in range
                        }
                    }
                }

                float newVal = abilities[i].castQuery.Evaluate(connectedTo, connectedTo.view.visibleMonsters, abilities[i], null);
                if (newVal > bestValue)
                {
                    bestValue = newVal;
                    bestIndex.Clear();
                    bestIndex.Add(i);
                }
                else if (newVal == bestValue)
                {
                    bestIndex.Add(i);
                }
            }
        }

        return (bestIndex[UnityEngine.Random.Range(0, bestIndex.Count)], bestValue);
    }
}
