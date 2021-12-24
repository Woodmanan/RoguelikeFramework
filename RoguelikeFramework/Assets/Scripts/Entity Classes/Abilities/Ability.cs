using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*********** The Ability Class **************
 * 
 * This class is designed to cover the range of possibilities for what
 * an activated ability can do. In gneral, this consists of having values,
 * determining if those values allow activation, and activation of some effect.
 * 
 * Very important to this class is the use of status effect modifiers. These 
 * allow the ability to interface with the status effect system, letting us 
 * create some really interesting effects without a whole lot of effort. This
 * is being written before this integration actually exists, so there's a good
 * chance that there exists some weirdness around making this work.
 */

/* Things that abilities need
 * 
 * 1. Costs
 * 2. Activation check
 * 3. Activation
 * 4. Ability-specific status effects
 * 5. Tie ins with all that goodness to the main system
 */

public class Ability : ScriptableObject
{
    //Public Resources
    public AbilityBlock info;
    [HideInInspector] public int currentCooldown = 0;

    //Called by ability component to set up a newly acquired ability.
    public void Setup()
    {
        currentCooldown = 0;
    }

    public bool CheckActivation(Monster caster)
    {
        bool canCast = true;
        if (currentCooldown != 0)
        {
            canCast = false;
        }
        if (canCast)
        {
            foreach (Resource r in Enum.GetValues(typeof(Resource)))
            {
                if (caster.resources[r] < info.costs[r])
                {
                    canCast = false;
                    break;
                }
            }
        }

        if (canCast)
        {
            canCast = OnCheckActivation(caster);
        }

        //TODO: Call the OnAbilityCheck modifier!


        return canCast;
    }

    public virtual bool OnCheckActivation(Monster caster)
    {
        return true;
    }

    public void Cast()
    {
        //TODO: Call the OnCast modifier!
        OnCast();
    }

    public virtual void OnCast()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
