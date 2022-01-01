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
        Debug.Log("Ability is starting!");
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
}
