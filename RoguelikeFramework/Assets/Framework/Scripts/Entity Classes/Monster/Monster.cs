using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using System.Text.RegularExpressions; //Oh god oh fuck (so true)
using System.Linq;


using static Resources;

public class Monster : MonoBehaviour
{
    [Header("Setup Variables")]
    public Stats baseStats;
    public Stats currentStats;

    //TODO: Abstract these out to another class!!!
    public string displayName;
    public bool nameRequiresPluralVerbs; //Useful for the player!

    public Faction faction = Faction.STANDARD;
    public int ID;
    public int minDepth;
    public int maxDepth;


    [Header("Runtime Attributes")]
    public float energy;

    public Vector2Int location;

    public int visionRadius;

    public int energyPerStep;
    public Loadout loadout;

    public static readonly float monsterZPosition = -5f;

    [HideInInspector] public Connections connections;
    [HideInInspector] public Connections other = null;

    [HideInInspector] public LOSData view;

    [HideInInspector] public List<Effect> effects;
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public Equipment equipment;
    [HideInInspector] public Abilities abilities;

    [HideInInspector] public ActionController controller;

    public SpriteRenderer renderer;

    public GameAction currentAction;
    public RogueTile currentTile;

    private bool setup = false;

    [HideInInspector] public bool willSwap;

    //public int XP;
    public int XPFromKill;
    public int level;

    private bool spriteDir;

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

        loadout?.Apply(this);

        spriteDir = GetComponent<SpriteRenderer>().flipX;

        setup = true;
    }

    public Monster Instantiate()
    {
        Monster newMonster = Instantiate(gameObject).GetComponent<Monster>(); //Should be guaranteed to work, unless things are incredibly borked
        newMonster.Setup();
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

    public void Heal(int healthReturned, bool shouldLog = false)
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
            healthReturned -= (int) (currentStats[MAX_HEALTH] - currentStats[HEALTH]);
            baseStats[HEALTH] = currentStats[MAX_HEALTH];
            connections.OnFullyHealed.BlendInvoke(other?.OnFullyHealed);
        }

        if (shouldLog && healthReturned > 0)
        {
            Debug.Log($"Log: {GetFormattedName()} heals for {healthReturned}");
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


    private void Damage(int damage, DamageType type, DamageSource source, string message = "{name} take%s{|s} {damage} damage")
    {
        connections.OnTakeDamage.BlendInvoke(other?.OnTakeDamage, ref damage, ref type, ref source);
        baseStats[HEALTH] -= damage;

        //Loggingstuff
        string toPrint = FormatStringForName(message).Replace("{damage}", $"{damage}");
        //Debug.Log($"Console print: {toPrint}");

        if (baseStats[HEALTH] <= 0)
        {
            connections.OnDeath.BlendInvoke(other?.OnDeath);
            if (baseStats[HEALTH] <= 0) //Check done for respawn mechanics to take effect
            {
                Die();
            }
        }
    }

    public void Damage(Monster dealer, int damage, DamageType type, DamageSource source, string message = "{name} take%s{|s} {damage} damage")
    {
        if (dealer == null)
        {
            Debug.LogError("Dealer was null!");
        }
        dealer?.connections.OnDealDamage.BlendInvoke(dealer.other?.OnDealDamage, ref damage, ref type, ref source);

        Damage(damage, type, source, message);

        //Quick hacky fix - Make this always true!
        if (dealer != null)
        {
            Debug.Log($"{dealer.GetFormattedName()} deals {damage} {type}damage");
        }
        

        if (baseStats[HEALTH] <= 0)
        {
            dealer?.KillMonster(this, type, source);
        }
    }


    public virtual void Die()
    {
        Debug.Log("Monster is dead!");

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

        AnimationController.AddAnimation(new DeathAnimation(this));
    }

    public void KillMonster(Monster target, DamageType type, DamageSource source)
    {
        connections.OnKillMonster.BlendInvoke(other?.OnKillMonster, ref target, ref type, ref source);
        GainXP(target.XPFromKill);
    }

    public virtual void GainXP(int amount)
    {
        Debug.Log($"{DebugName()} has gained {amount} XP!");
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
        return baseStats[HEALTH] <= 0;
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
    }

    public string DebugName()
    {
        return $"{this.name} ({this.location})";
    }


    public void StartTurn()
    {
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

    public void SetAction(GameAction act)
    {
        if (currentAction != null)
        {
            Debug.LogError($"{this.displayName} had an action set, but it already had an action. Should this be allowed?", this);
        }
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
        if (view.visibleMonsters.Any(x => x.IsEnemy(this))) currentAction = null;
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
        Effect[] instEffects = effectsToAdd.Select(x => x.Instantiate()).ToArray();

        connections.OnApplyStatusEffects.BlendInvoke(other?.OnApplyStatusEffects, ref effectsToAdd);
        for (int i = 0; i < effectsToAdd.Length; i++)
        {
            Effect e = effectsToAdd[i];
            e.Connect(this.connections);
            effects.Add(e);
        }
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

    public void SetPositionSnap(Map map, Vector2Int newPosition)
    {
        SetPosition(map, newPosition);
        transform.position = new Vector3(newPosition.x, newPosition.y, monsterZPosition);
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
        this.baseStats += resources;
    }

    public void LoseResources(Stats resources)
    {
        connections.OnLoseResources.BlendInvoke(other?.OnLoseResources, ref resources);
        this.baseStats -= resources;
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
    }
}