﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static Resources;
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

public class Ability : ScriptableObject, IDescribable
{
    public string friendlyName;

    [SerializeField]
    public LocalizedString locName;

    [SerializeField]
    public LocalizedString locDescription;

    public Sprite image;
    public Color color;

    //Public Resources
    public Targeting baseTargeting;
    [SerializeReference]
    public List<TargetingAnimation> animations;

    [ResourceGroup(ResourceType.Ability)]
    public Stats baseStats;
    [HideInInspector] public Stats currentStats;

    public float EnergyCost = 100;

    public Query castQuery;

    [HideInInspector] public Targeting targeting;
    [ResourceGroup(ResourceType.Monster)]
    public Stats costs;

    protected bool dirty = true;
    
    [HideInInspector] public int currentCooldown
    {
        get
        {
            return Mathf.RoundToInt(baseStats[CURRENT_COOLDOWN]);
        }
        set
        {
            baseStats[CURRENT_COOLDOWN] = value;
        }
    }

    [HideInInspector] public bool castable = true;

    public AbilityTypes types;
    [SerializeReference] List<Effect> effects;
    [SerializeField] List<Effect> attachedEffects = new List<Effect>();
    [System.NonSerialized]
    public Connections connections = null;
    [HideInInspector] public RogueHandle<Monster> credit;

    public Ability Instantiate()
    {
        Ability copy = Instantiate(this);
        copy.Setup();
        return copy;
    }

    public virtual string GetName(bool shorten = false)
    {
        if (locName.IsEmpty)
        {
            if (friendlyName.Length == 0)
                return $"Missing Name ({name})";
            else
                return $"Missing Name ({friendlyName})";
        }
        return locName.GetLocalizedString(this, currentStats.dictionary);
    }

    public virtual string GetDescription()
    {
        return locDescription.GetLocalizedString(this, currentStats.dictionary);
    }

    public virtual Sprite GetImage()
    {
        return image;
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
        OnSetup();

        //RegenerateStats(null);
    }

    //TODO: Set this up in a nice way
    public void RegenerateStats(RogueHandle<Monster> m)
    {
        //I am losing my fucking mind
        targeting = baseTargeting.ShallowCopy();
        currentStats = baseStats.Copy();
        Ability ability = this;

        OnRegenerateStats(m);

        connections.OnRegenerateAbilityStats.BlendInvoke(m[0].connections?.OnRegenerateAbilityStats, ref m, ref currentStats, ref ability);
        targeting.range += Mathf.RoundToInt(currentStats[RANGE]);
        targeting.radius += Mathf.RoundToInt(currentStats[RADIUS]);
    }

    public virtual void OnRegenerateStats(RogueHandle<Monster> caster)
    {

    }

    public virtual void OnSetup() { }

    public virtual bool IsValidTarget(RogueHandle<Monster> target)
    {
        return true;
    }

    public bool CheckAvailable(RogueHandle<Monster> caster)
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
                if (caster.value.currentStats[r] < costs[r])
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
        connections.monster = caster;
        caster[0].connections.OnCheckAvailability.BlendInvoke(connections.OnCheckAvailability, ref casting, ref canCast);
        connections.monster = RogueHandle<Monster>.Default;

        if (canCast)
        {
            canCast = OnCheckActivationHard(caster);
        }

        castable = canCast;
        return canCast;
    }

    //Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    public virtual bool OnCheckActivationSoft(RogueHandle<Monster> caster)
    {
        return true;
    }

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    public virtual bool OnCheckActivationHard(RogueHandle<Monster> caster)
    {
        return true;
    }

    

    public IEnumerator Cast(RogueHandle<Monster> caster)
    {
        if (credit == null)
        {
            credit = caster;
        }

        IEnumerator castingRoutine = OnCast(caster);

        while (castingRoutine.MoveNext())
        {
            yield return castingRoutine.Current;
        }
    }

    public virtual IEnumerator OnCast(RogueHandle<Monster> caster)
    {
        Debug.Log("Ability did not override basic call", this);
        yield break;
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

    new public void SetDirty()
    {
        dirty = true;
    }

    public bool IsDirty()
    {
        return dirty;
    }

    public void ClearDirty()
    {
        dirty = false;
    }
}
