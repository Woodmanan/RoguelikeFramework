using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Come up with a better name for this?
public class Abilities : MonoBehaviour
{
    Monster connectedTo;
    List<Ability> abilities = new List<Ability>();

    //TEMPORARY FIX
    public List<Ability> startingAbilities;

    public int Count
    {
        get { return abilities.Count; }
    }

    // Start is called before the first frame update
    void Start()
    {
        connectedTo = GetComponent<Monster>();
        foreach (Ability a in startingAbilities)
        {
            abilities.Add(a.Instantiate());
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

        for (int i = 0; i < abilities.Count; i++)
        {
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

        return (bestIndex[UnityEngine.Random.Range(0, bestIndex.Count)], bestValue);
    }
}
