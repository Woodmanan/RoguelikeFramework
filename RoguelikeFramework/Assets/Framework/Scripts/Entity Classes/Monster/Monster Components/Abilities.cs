using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//TODO: Come up with a better name for this?
public class Abilities : MonoBehaviour
{
    public int maxAbilities = 10;
    RogueHandle<Monster> connectedTo;
    public List<Ability> baseAbilities;
    List<Ability> abilities = new List<Ability>();

    public int Count
    {
        get { return abilities.Count; }
    }

    private void Awake()
    {
        connectedTo = GetComponent<UnityMonster>().monsterHandle;
        foreach (Ability ability in baseAbilities)
        {
            AddAbilityInstantiate(ability);
        }
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

        //Force no casting on abilites lower than your level
        for (int i = 0; i < abilities.Count; i++)
        {
            if ((i+1) > connectedTo[0].level && HasAbility(i))
            {
                abilities[i].castable = false;
            }
        }
    }

    public Ability this[int index]
    {
        get { return abilities[index]; }
    }

    public bool HasAbility(int index)
    {
        return index >= 0 && index < abilities.Count && abilities[index] != null;
    }


    public void AddAbility(Ability abilityToAdd)
    {
        if (abilities.Count < maxAbilities)
        {
            abilities.Add(abilityToAdd);
        }
        else
        {
            Debug.Log("Console: You can't learn that ability - max abilities reached");
        }
    }

    public void AddAbilityInstantiate(Ability abilityToAdd)
    {
        AddAbility(abilityToAdd.Instantiate());
    }

    public void RemoveAbility(Ability abilityToRemove)
    {
        int index = abilities.IndexOf(abilityToRemove);
        if (index >= 0)
        {
            abilities.RemoveAt(index);
            foreach (Ability ability in abilities)
            {
                ability.SetDirty();
            }
        }
    }

    public void RemoveAllAbilities()
    {
        abilities.Clear();
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
        List<RogueHandle<Monster>> allies = connectedTo[0].view.visibleFriends;
        List<int> allyDistances = allies.Select(x =>
        {
            return Mathf.Max(Mathf.Abs(x[0].location.x - connectedTo[0].location.x),
                             Mathf.Abs(x[0].location.y - connectedTo[0].location.y));
        }).ToList();


        List<RogueHandle<Monster>> enemies = connectedTo[0].view.visibleEnemies;
        List<int> enemyDistances = enemies.Select(x =>
        {
            return Mathf.Max(Mathf.Abs(x[0].location.x - connectedTo[0].location.x),
                             Mathf.Abs(x[0].location.y - connectedTo[0].location.y));
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

                float newVal = abilities[i].castQuery.Evaluate(connectedTo, abilities[i], null);
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

    public IEnumerable<Ability> GetAbilitiesAsEnumerable()
    {
        return abilities.AsEnumerable();
    }
}
