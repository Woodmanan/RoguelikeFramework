using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using System.Text.RegularExpressions; //Oh god oh fuck (so true)
using System.Linq;
using UnityEngine.Localization;

using static Resources;

public class Monster : MonoBehaviour, IDescribable
{
    [Header("Setup Variables")]
    public Stats baseStats;

    public Stats currentStats;

    public DamageType resistances;
    public DamageType weaknesses;
    public DamageType immunities;

    //TODO: Abstract these out to another class!!!
    [Header("Logging controls")]
    public LocalizedString localName;
    public LocalizedString localDescription;
    public string friendlyName;
    [Tooltip("Controls what kind of verbs are used. 'You cast' (true) vs 'It casts' (false)")]
    public bool singular;
    [Tooltip("Is this a generic insance of a monster? 'The goblin' (true) vs 'Grim Timothy' (false)")]
    public bool named;
    public bool nameRequiresPluralVerbs; //Useful for the player!

    public Faction faction = Faction.STANDARD;
    public int ID;
    public int minDepth;
    public int maxDepth;

    [SerializeReference]
    public List<Effect> baseEffects;

    [Header("Runtime Attributes")]
    public float energy;

    public Vector2Int location;

    public int visionRadius;

    public int energyPerStep;

    public static readonly float monsterZPosition = -5f;

    [HideInInspector] public Connections connections;
    [HideInInspector] private Connections other = null;

    [HideInInspector] public LOSData view;

    [HideInInspector] public List<Effect> effects;
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public Equipment equipment;
    [HideInInspector] public Abilities abilities;

    [HideInInspector] public ActionController controller;

    public SpriteRenderer renderer;
    public List<GameObject> FXObjects;

    public GameAction currentAction;
    public RogueTile currentTile;

    private bool setup = false;

    [HideInInspector] public bool willSwap;

    //public int XP;
    public int XPFromKill;
    public int level;

    private bool spriteDir;

    bool dead = false;

    // Start is called before the first frame update
    public virtual void Start()
    {
        Setup();
    }

    //This function should do ALL setup operations necessary to interact with this object and it's components.
    public void Setup()
    {
        if (setup) return;
        inventory = GetComponent<Inventory>();
        equipment = GetComponent<Equipment>();
        abilities = GetComponent<Abilities>();
        controller = GetComponent<ActionController>();

        renderer = GetComponent<SpriteRenderer>();

        connections = new Connections(this);
        baseStats[HEALTH] = baseStats[MAX_HEALTH];
        baseStats[MANA] = baseStats[MAX_MANA];
        baseStats[STAMINA] = baseStats[MAX_STAMINA];
        baseStats[XP] = 0;
        currentStats = baseStats.Copy();
        connections.OnFullyHealed.BlendInvoke(other?.OnFullyHealed);
        
        inventory?.Setup();
        equipment?.Setup();
        controller?.Setup();

        spriteDir = GetComponent<SpriteRenderer>().flipX;

        AddEffectInstantiate(baseEffects.ToArray());

        setup = true;
    }

    public Monster Instantiate()
    {
        Monster newMonster = Instantiate(gameObject).GetComponent<Monster>(); //Should be guaranteed to work, unless things are incredibly borked
        //newMonster.Setup();
        return newMonster;
    }

    //Called right before the main loop, when the rest of the game has been set up.
    public void PostSetup(Map map)
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        //Confirm that we got our own unique space
        Debug.Assert(map.GetTile(location).currentlyStanding == this || map.GetTile(location).currentlyStanding == null, "Generator placed two monsters together", this);
        #endif
        //Put us in that space, and build our initial LOS
        transform.position = new Vector3(location.x, location.y, monsterZPosition);
        SetPosition(map, location);
        UpdateLOS(map);
    }

    //Dummy function since compiler complains due to interface
    public string GetName(bool shorten)
    {
        return GetName(shorten, true);
    }

    public string GetName(bool shorten = false, bool definite = true)
    {
        if (shorten)
        {
            return localName.GetLocalizedString(this, currentStats.dictionary);
        }
        else
        {
            return LogFormatting.FormatNameForMonster(this, definite);
        }    
    }

    public string GetDescription()
    {
        return localDescription.GetLocalizedString(this, currentStats.dictionary);
    }

    public Sprite GetImage()
    {
        return renderer.sprite;
    }

    public void Heal(float healthReturned, bool shouldLog = false)
    {
        connections.OnHealing.BlendInvoke(other?.OnHealing, ref healthReturned);

        //Healing has no actual effect (and therefore no display) if you didn't actually heal
        if (baseStats[HEALTH] == currentStats[MAX_HEALTH])
        {
            return;
        }

        if (healthReturned > 0) //Negative health healing is currently just ignored, for clarity's sake
        {
            baseStats[HEALTH] += healthReturned;
        }
        if (baseStats[HEALTH] >= currentStats[MAX_HEALTH])
        {
            healthReturned -= (currentStats[MAX_HEALTH] - currentStats[HEALTH]);
            baseStats[HEALTH] = currentStats[MAX_HEALTH];
            connections.OnFullyHealed.BlendInvoke(other?.OnFullyHealed);
        }

        if (shouldLog && healthReturned > 0)
        {
            RogueLog.singleton.Log($"{GetName()} heals for {healthReturned}");
        }
    }

    public bool Attack(int pierce, int accuracy)
    {
        connections.OnAttacked.BlendInvoke(other?.OnAttacked, ref pierce, ref accuracy);

        if (accuracy < currentStats[EV])
        {
            //TODO: Log dodge
            return false;
        }

        if (pierce < currentStats[AC])
        {
            //TODO: Log break damage
            return false;
        }

        //TODO: Log Hit
        // 
        return true;
    }

    public void Damage(Monster dealer, float damage, DamageType type, DamageSource source)
    {
        float damageMod = 1f;
        if ((resistances & type) > 0)
        {
            damageMod = damageMod / 2;
        }
        if ((weaknesses & type) > 0)
        {
            damageMod = damageMod * 2;
        }
        if ((immunities & type) > 0)
        {
            damageMod = 0;
        }
        if ((type & DamageType.TRUE) > 0)
        {
            damageMod = Mathf.Max(damageMod, 1);
        }

        damage *= damageMod;

        #if UNITY_EDITOR
        if (dealer == null)
        {
            Debug.LogError("Dealer was null! Fix me you fool!!!");
        }
        #endif
        dealer?.connections.OnDealDamage.BlendInvoke(dealer.other?.OnDealDamage, ref damage, ref type, ref source);

        connections.OnTakeDamage.BlendInvoke(other?.OnTakeDamage, ref damage, ref type, ref source);
        baseStats[HEALTH] -= damage;

        //Quick hacky fix - Make this always true!
        if (dealer != null)
        {
            RogueLog.singleton.Log($"{dealer.GetFormattedName()} deals {Mathf.CeilToInt(damage)} {type} damage with {source}");
        }
        

        if (baseStats[HEALTH] <= 0 && !dead)
        {
            //Mark as dead temporarily, to prevent infinite loop
            dead = true;
            connections.OnDeath.BlendInvoke(other?.OnDeath);
            if (baseStats[HEALTH] <= 0) //Check done for respawn mechanics to take effect
            {
                Die();
                dealer?.KillMonster(this, type, source);
            }
            else
            {
                dead = false;
            }
        }
    }

    //Kills this monster, without damage
    public void DestroyMonster()
    {
        connections.OnDeath.BlendInvoke(other?.OnDeath);
        Die();
    }


    protected virtual void Die()
    {
        dead = true;
        RogueLog.singleton.LogTemplate("DeathString", new { monster = GetName(), singular = singular }, this.gameObject, priority: LogPriority.HIGH);

        foreach (Effect effect in effects)
        {
            effect.Disconnect();
        }

        //Clear tile, so other systems don't try to use a dead monster
        if (currentTile.currentlyStanding == this)
            currentTile.currentlyStanding = null;

        //Clear inventory, if it exists
        if (inventory)
        {
            GameAction dropAll = new DropAction(inventory.AllIndices());
            dropAll.Setup(this);
            while (dropAll.action.MoveNext()) { }
        }

        AnimationController.AddAnimationForObject(new DeathAnimation(this), this);
    }

    public void KillMonster(Monster target, DamageType type, DamageSource source)
    {
        connections.OnKillMonster.BlendInvoke(other?.OnKillMonster, ref target, ref type, ref source);
        GainXP(target, target.XPFromKill);
    }

    public virtual void GainXP(Monster source, float amount)
    {
        RogueLog.singleton.LogTemplate("XP",
            new { monster = GetName(), singular = singular, amount = Mathf.RoundToInt(amount) },
            priority: LogPriority.LOW
            );
        connections.OnGainXP.BlendInvoke(other?.OnGainXP, ref amount);
        baseStats[XP] += amount;
        if (baseStats[XP] >= currentStats[NEXT_LEVEL_XP])
        {
            baseStats[XP] -= currentStats[NEXT_LEVEL_XP];
            Debug.Log($"After leveling up with {XPTillNextLevel()} xp, monster now has {baseStats[XP]} xp leftover.");
            LevelUp();
        }

    }

    //TODO: Make this 
    public virtual int XPTillNextLevel()
    {
        baseStats[NEXT_LEVEL_XP] = int.MaxValue;
        return int.MaxValue;
    }

    public void LevelUp()
    {
        level++;
        connections.OnLevelUp.BlendInvoke(other?.OnLevelUp, ref level);
        baseStats[NEXT_LEVEL_XP] = XPTillNextLevel();
        OnLevelUp();
        abilities?.CheckAvailability();
    }

    public virtual void OnLevelUp()
    {
        Debug.Log($"{DebugName()} leveled up! This does nothing, yet.");
    }

    public virtual void Remove()
    {
        // Like dying but no drops
        Debug.Log("Monster Removed!");
        baseStats[HEALTH] = 0;
        currentTile.ClearMonster();
    }

    public bool IsDead()
    {
        //Oops, this must be <= 0, Sometimes people can overkill!
        return dead || baseStats[HEALTH] <= 0;
    }

    private string FormatStringForName(string msg)
    {
        //Handle the {name} formatter
        string newMessage = msg.Replace("{name}", GetFormattedName());

        //Handle the %s{singular form | plural} formatter
        MatchCollection matches = Regex.Matches(newMessage, "%s\\{([a-zA-Z\\w]*\\|[a-zA-Z\\w]*)\\}");
        foreach (Match m in matches)
        {
            for (int i = 1; i < m.Groups.Count; i++)
            {
                string[] val = m.Groups[i].Value.Split('|');
                string rep = nameRequiresPluralVerbs ? val[1] : val[0];
                newMessage = newMessage.Replace(m.Groups[0].Value, rep);
            }

        }

        //Capitalize first letter
        newMessage = char.ToUpper(newMessage[0]) + newMessage.Substring(1);

        //newMessage = newMessage.Replace("{s}", nameRequiresPluralVerbs ? "s" : "");
        return newMessage;
    }

    public string GetFormattedName()
    {
        string displayName = localName.GetLocalizedString();
        return nameRequiresPluralVerbs ? "the " + displayName : displayName;
    }

    public void AddEnergy(int energy)
    {
        connections.OnEnergyGained.BlendInvoke(other?.OnEnergyGained, ref energy);
        this.energy += energy;
    }

    public virtual void UpdateLOS()
    {
        UpdateLOS(Map.current);
    }

    public virtual void UpdateLOS(Map map)
    {
        this.view = LOS.LosAt(map, location, visionRadius);
        UpdateLOSPreCollection();
        view.CollectEntities(map);
        UpdateLOSPostCollection();
    }

    public void UpdateLOSPreCollection()
    {
        connections.OnGenerateLOSPreCollection.BlendInvoke(other?.OnGenerateLOSPreCollection, ref view);
    }

    public void UpdateLOSPostCollection()
    {
        connections.OnGenerateLOSPostCollection.BlendInvoke(other?.OnGenerateLOSPostCollection, ref view);
    }

    public string GetLocalizedName()
    {
        return localName.GetLocalizedString();
    }

    public string DebugName()
    {
        return $"{this.friendlyName} ({this.location})";
    }


    public void StartTurn()
    {
        UpdateLOS();
        CallRegenerateStats();
        abilities?.CheckAvailability();
        connections.OnTurnStartLocal.BlendInvoke(other?.OnTurnStartLocal);
        willSwap = false;
    }

    public void EndTurn()
    {
        connections.OnTurnEndLocal.BlendInvoke(other?.OnTurnEndLocal);
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            if (effects[i].ReadyToDelete)
            {
                effects.RemoveAt(i);
            }
        }
        CallRegenerateStats();
    }

    public void CallRegenerateStats()
    {
        currentStats = baseStats.Copy();
        connections.RegenerateStats.BlendInvoke(other?.RegenerateStats, ref currentStats);
    }

    public void AddBaseStat(Resources r, float value)
    {
        baseStats[r] += value;
        ResetStatsToMax();
    }

    public void AddBaseStats(Stats s)
    {
        baseStats &= s;
        ResetStatsToMax();
    }

    public void ResetStatsToMax()
    {
        baseStats[HEALTH] = Mathf.Min(baseStats[HEALTH], currentStats[MAX_HEALTH]);
        baseStats[MANA] = Mathf.Clamp(baseStats[MANA], 0, currentStats[MAX_MANA]);
        baseStats[STAMINA] = Mathf.Min(baseStats[STAMINA], currentStats[MAX_STAMINA]);
        baseStats[HEAT] = Mathf.Clamp(baseStats[HEAT], 0, currentStats[MAX_HEAT]);
    }

    public void SetAction(GameAction act)
    {
        if (currentAction != null)
        {
            Debug.LogError($"{friendlyName} had an action {act.GetType()} set, but it already had an action ({this.currentAction.GetType()}). Try SetActionOverride isntead!", this);
        }
        currentAction = act;
        currentAction.Setup(this);
    }

    //Currently identical, but split up to cover edge cases that might arise
    public void SetActionOverride(GameAction act)
    {
        currentAction = act;
        currentAction.Setup(this);
    }

    public virtual IEnumerator DetermineAction()
    {
        controller.ClearAction();

        while (controller.selection.MoveNext())
        {
            yield return controller.selection.Current;
        }

        if (controller.nextAction != null)
        {
            SetAction(controller.nextAction);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        else
        {
            if (this != Player.player)
            {
                Debug.LogError("Monster AI returned null! That should NEVER happen!");
            }
        }
#endif
    }

    //Takes the local turn
    public IEnumerator LocalTurn()
    {
        if (view.visibleMonsters.Any(x => x.IsEnemy(this)))
        {
            if (currentAction != null && currentAction.checksOnVisible && !currentAction.hasPreventedStop)
            {
                if (this == Player.player)
                {
                    UIController.singleton.OpenConfirmation("You see an enemy. Would you like to stop?", x => currentAction.stopsOnVisible = x);
                    yield return new WaitUntil(() => !UIController.WindowsOpen);
                    currentAction.hasPreventedStop = !currentAction.stopsOnVisible;
                }
                else
                {
                    currentAction.stopsOnVisible = true;
                }
            }
            if (currentAction != null && currentAction.stopsOnVisible && !currentAction.hasPreventedStop)
            {
                currentAction = null;
            }
        }
        while (energy > 0)
        {
            if (currentAction == null)
            {
                IEnumerator actionDecision = DetermineAction();
                while (actionDecision.MoveNext())
                {
                    yield return actionDecision.Current;
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                //Expensive and unnessecary check, done in Editor only
                if (currentAction == null && this != Player.player)
                {
                    Debug.LogError("A monster returned a null action. Please force all monster AI systems to always return an action.", this);
                    Debug.LogError("This error will NOT be caught in build, so please fix it now!!!!");
                    this.energy -= 10; //Breaks the game state, but prevents our coroutine from running forever
                }
#endif

                if (currentAction == null)
                {
                    yield return null;
                    continue;
                }
            }

            //Short circuits early!
            while (energy > 0 && currentAction.action.MoveNext())
            {
                if (currentAction.action.Current != GameAction.AbortAll)
                {
                    yield return currentAction.action.Current;
                }
                else
                {
                    currentAction.successful = false;
                    currentAction.finished = true;
                    break;
                }
            }

            if (currentAction.finished)
            {
                currentAction = null;
            }
        }
    }

    public void AddEffect(params Effect[] effectsToAdd)
    {
        connections.OnApplyStatusEffects.BlendInvoke(other?.OnApplyStatusEffects, ref effectsToAdd);
        for (int i = 0; i < effectsToAdd.Length; i++)
        {
            Effect e = effectsToAdd[i];
            int index = DetermineBestIndex(e);
            if (index >= 0)
            {
                e.Connect(this.connections);
                effects.Insert(index, e);
            }
        }
    }

    public void AddEffectInstantiate(params Effect[] effectsToAdd)
    {
        Effect[] instEffects = effectsToAdd.Select(x => x.Instantiate()).ToArray();

        AddEffect(instEffects);

    }

    public int DetermineBestIndex(Effect effect)
    {
        //TODO: do this a better way.
        int index = effects.Select(x => x.GetType().FullName).ToList().BinarySearch(effect.GetType().FullName);
        if (index < 0) index = ~index;
        bool shouldContinue = true;

        //Search backwards for matching candidate
        for (int i = index - 1; i >= 0; i++)
        {
            Effect otherEffect = effects[i];
            if (otherEffect.GetType() != effect.GetType())
            {
                break;
            }
            effect.OnStack(otherEffect, ref shouldContinue);
            if (!shouldContinue) return -1;
        }

        //Search forwards
        for (int i = index; i < effects.Count; i++)
        {
            Effect otherEffect = effects[i];
            if (otherEffect.GetType() != effect.GetType())
            {
                break;
            }
            effect.OnStack(otherEffect, ref shouldContinue);
            if (!shouldContinue) return -1;
        }

        return index;
    }

    public int Compare(Effect a, Effect b)
    {
        return a.GetType().FullName.CompareTo(b.GetType().FullName);
    }

    //TODO: Add cost of moving from on spot to another
    public void SetPosition(Vector2Int newPosition)
    {
        SetPosition(Map.current, newPosition);
    }

    public void SetPositionSnap(Vector2Int newPosition)
    {
        SetPositionSnap(Map.current, newPosition);
    }

    public void SetPosition(Map map, Vector2Int newPosition)
    {
        if (currentTile) currentTile.ClearMonster();
        
        // update location
        location = newPosition;

        //Old update code - handled by animation now?
        //transform.position = new Vector3(newPosition.x, newPosition.y, monsterZPosition);

        //Update sprite sorting
        renderer.sortingOrder = -location.y;

        currentTile = map.GetTile(location);
        currentTile.SetMonster(this);

        if (currentTile.isVisible)
        {
            SetGraphics(true);
        }
        else
        {
            SetGraphics(false);
        }
    }

    public void SetPositionNoGraphicsUpdate(Vector2Int newPosition)
    {
        SetPositionNoGraphicsUpdate(Map.current, newPosition);
    }

    public void SetPositionNoGraphicsUpdate(Map map, Vector2Int newPosition)
    {
        bool rendering = renderer.enabled;
        SetPosition(map, newPosition);
        SetGraphics(rendering);
    }

    public void SetPositionSnap(Map map, Vector2Int newPosition)
    {
        SetPosition(map, newPosition);
        transform.position = new Vector3(newPosition.x, newPosition.y, monsterZPosition);
        //UpdateLOS();
    }

    //Function to activate event call of Global turn start
    public void OnTurnStartGlobalCall()
    {
        connections.OnTurnStartGlobal.BlendInvoke(other?.OnTurnStartGlobal);
    }

    //Same purpose as above
    public void OnTurnEndGlobalCall()
    {
        abilities?.OnTurnEndGlobal();
        connections.OnTurnEndGlobal.BlendInvoke(other?.OnTurnEndGlobal);
    }

    public void GainResources(Stats resources)
    {
        connections.OnGainResources.BlendInvoke(other?.OnGainResources, ref resources);
        this.baseStats &= resources;
    }

    public void LoseResources(Stats resources)
    {
        connections.OnLoseResources.BlendInvoke(other?.OnLoseResources, ref resources);
        this.baseStats ^= resources;
    }


    /************************************
     *         Inventory code
     ***********************************/
    public void DropItem(int index)
    {
        inventory.MonsterToFloor(index);
    }

    public void PickUp(int OnGroundIdx)
    {
        inventory.FloorToMonster(OnGroundIdx);
    }

    private void AddItemToInventory(Item i)
    {
        inventory.Add(i);
    }

    //Checks faction flags for matches. If none, return true!
    public bool IsEnemy(Monster other)
    {
        return (faction & other.faction) == 0;
    }

    public float DistanceFrom(Monster other)
    {
        return Vector2Int.Distance(location, other.location);
    }

    public void SetGraphics(bool state)
    {
        renderer.enabled = state;
        foreach (GameObject g in FXObjects)
        {
            g.SetActive(state);
        }
    }

    public void UpdatePositionToLocation(Vector2Int location)
    {
        transform.position = new Vector3(location.x, location.y, monsterZPosition);
        SetGraphics(Map.current.GetTile(location).isVisible);
    }

    public void AddConnection(Connections toAdd)
    {
        if (other != null)
        {
            RemoveConnection(other);
        }
        toAdd.monster = this;
        other = toAdd;
    }

    public void RemoveConnection(Connections toRemove)
    {
        if (toRemove != null)
        {
            toRemove.monster = null;
            other = null;
        }
    }

    public T GetEffect<T>() where T : Effect
    {
        foreach (Effect e in effects)
        {
            T cast = e as T;
            if (cast != null) return cast;
        }

        return null;
    }

    public float GetEstimatedStrength()
    {
        float strength = 0;
        //Benefit of all of these being ~equal value
        strength += (currentStats[MAX_HEALTH] * (currentStats[AC] + currentStats[EV] + currentStats[MR])) / 10;

        if (equipment)
        {
            foreach (EquipmentSlot slot in equipment.equipmentSlots)
            {
                if (slot.active)
                {
                    Item i = slot.equipped.held[0];
                    if (i.melee || i.ranged)
                    {
                        Weapon weapon = (i.melee != null) ? i.melee as Weapon : i.ranged as Weapon;
                        if (slot.type.Contains(EquipSlotType.PRIMARY_HAND))
                        {
                            foreach (DamagePairing pair in weapon.primary.damage)
                            {
                                strength += (pair.damage.dice * pair.damage.rolls / 2) * weapon.primary.accuracy * weapon.primary.accuracy;
                            }
                        }
                        else
                        {
                            foreach (DamagePairing pair in weapon.secondary.damage)
                            {
                                strength += (pair.damage.dice * pair.damage.rolls / 2) * weapon.secondary.accuracy * weapon.secondary.piercing;
                            }
                        }
                    }
                }
                else if (slot.CanAttackUnarmed)
                {
                    foreach (DamagePairing pair in slot.unarmedAttack.damage)
                    {
                        strength += (pair.damage.dice * pair.damage.rolls / 2) * slot.unarmedAttack.accuracy * slot.unarmedAttack.piercing;
                    }
                }
            }
        }

        //Casting is a general indicator of strength
        //More abilities is also more powerful
        if (abilities)
        {
            strength += abilities.Count * currentStats[MAX_MANA] / 3;
        }


        return strength;
    }
}