using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static AbilityResources;
using UnityEngine.Localization;

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
 * 1. Costs
 * 2. Activation check
 * 3. Activation
 * 4. Ability-specific status effects
 * 5. Tie ins with all that goodness to the main system
 */


public class Ability : ScriptableObject
{
    public string friendlyName;

    public LocalizedString locName;
    public LocalizedString locDescription;

    public Sprite image;
    public Color color;

    //Public Resources
    public Targeting baseTargeting;
    public AbilityStats baseStats;
    [HideInInspector] public AbilityStats currentStats;

    public Query castQuery;

    [HideInInspector] public Targeting targeting;
    public Stats costs;
    
    [HideInInspector] public int currentCooldown
    {
        get
        {
            return Mathf.RoundToInt(baseStats[COOLDOWN]);
        }
        set
        {
            baseStats[COOLDOWN] = value;
        }
    }

    [HideInInspector] public bool castable = true;

    public AbilityTypes types;
    [SerializeReference] List<Effect> effects;
    List<Effect> attachedEffects = new List<Effect>();
    public Connections connections = null;

    public Ability Instantiate()
    {
        Ability copy = Instantiate(this);
        copy.Setup();
        return copy;
    }

    public void AddEffect(params Effect[] effects)
    {
        foreach (Effect e in effects)
        {
            e.Connect(connections);
            attachedEffects.Add(e);
        }
    }

    //Called by ability component to set up a newly acquired ability.
    public void Setup()
    {
        currentCooldown = 0;
        if (connections == null) connections = new Connections(this);
        AddEffect(effects.Select(x => x.Instantiate()).ToArray());
        RegenerateStats(null);
    }

    //TODO: Set this up in a nice way
    public void RegenerateStats(Monster m)
    {
        //I am losing my fucking mind
        targeting = baseTargeting.ShallowCopy();
        currentStats = baseStats.Copy();
        Ability ability = this;

        connections.OnRegenerateAbilityStats.BlendInvoke(m?.connections?.OnRegenerateAbilityStats, ref m, ref currentStats, ref ability);
        targeting.range += Mathf.RoundToInt(currentStats[RANGE_INCREASE]);
        targeting.radius += Mathf.RoundToInt(currentStats[RADIUS_INCREASE]);
    }

    public bool CheckAvailable(Monster caster)
    {
        bool canCast = true;
        if (currentCooldown != 0)
        {
            canCast = false;
        }
        if (canCast)
        {
            foreach (Resources r in costs.dictionary.Keys)
            {
                if (caster.currentStats[r] < costs[r])
                {
                    canCast = false;
                    break;
                }
            }
        }

        if (canCast)
        {
            canCast = OnCheckActivationSoft(caster);
        }

        Ability casting = this;

        //Link in status effects for this system
        caster.connections.OnCheckAvailability.BlendInvoke(connections.OnCheckAvailability, ref casting, ref canCast);

        if (canCast)
        {
            canCast = OnCheckActivationHard(caster);
        }

        castable = canCast;
        return canCast;
    }

    //Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    public virtual bool OnCheckActivationSoft(Monster caster)
    {
        return true;
    }

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    public virtual bool OnCheckActivationHard(Monster caster)
    {
        return true;
    }

    

    public void Cast(Monster caster)
    {
        //TODO: Call the OnCast modifier!
        OnCast(caster);

        baseStats[COOLDOWN] = Mathf.Max(0, baseStats[MAX_COOLDOWN] - baseStats[COOLDOWN_DECREASE]);
    }

    public virtual void OnCast(Monster caster)
    {
        Debug.Log("Ability did not override basic call", this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReduceCooldown()
    {
        currentCooldown--;
        if (currentCooldown < 0)
        {
            currentCooldown = 0;
        }
    }

    //Should be called at end of turn, to check and clean anything that's not needed anymore.
    public void Cleanup()
    {
        //Clean up old effect connections
        for (int i = attachedEffects.Count - 1; i >= 0; i--)
        {
            if (attachedEffects[i].ReadyToDelete) { attachedEffects.RemoveAt(i); }
        }
    }
}
