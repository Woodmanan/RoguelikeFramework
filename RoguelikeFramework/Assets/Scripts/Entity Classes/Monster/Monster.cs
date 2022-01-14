using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using System.Text.RegularExpressions; //Oh god oh fuck
using System.Linq;



public class Monster : MonoBehaviour
{
    [Header("Setup Variables")]
    public StatBlock baseStats;
    public StatBlock stats;

    public ResourceList resources;

    //TODO: Abstract these out to another class!!!
    new public string name;
    public bool nameRequiresPluralVerbs; //Useful for the player!

    public Faction faction = Faction.STANDARD;


    [Header("Runtime Attributes")]
    public float energy;

    public Vector2Int location;

    public int visionRadius;

    public int energyPerStep;

    private static readonly float monsterZPosition = -5f;

    [HideInInspector] public Connections connections;
    [HideInInspector] public Connections other = null;

    [HideInInspector] public LOSData view;

    [HideInInspector] public List<Effect> effects;
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public Equipment equipment;
    [HideInInspector] public Abilities abilities;

    [HideInInspector] public ActionController controller;

    public GameAction currentAction;
    public CustomTile currentTile;
    
    // Start is called before the first frame update
    public virtual void Start()
    {
        inventory = GetComponent<Inventory>(); 
        equipment = GetComponent<Equipment>();
        abilities = GetComponent<Abilities>();
        controller = GetComponent<ActionController>();

        //TODO: Have starting equipment? Probably not a huge concern right now, though.
        stats = baseStats;

        foreach (Resource r in Enum.GetValues(typeof(Resource)))
        {
            resources[r] = stats.resources[r];
        }

        connections = new Connections(this);

        resources.health = stats.resources.health;

        connections.OnFullyHealed.BlendInvoke(other?.OnFullyHealed);
    }

    //Called right before the main loop, when the rest of the game has been set up.
    public void PostSetup()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Assert(Map.current.GetTile(location).currentlyStanding == null, "Generator placed two monsters together", this);
        #endif
        SetPosition(location);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Heal(int healthReturned)
    {
        connections.OnHealing.BlendInvoke(other?.OnHealing, ref healthReturned);

        if (healthReturned > 0) //Negative health healing is currently just ignored, for clarity's sake
        {
            resources.health += healthReturned;
        }
        if (resources.health >= stats.resources.health)
        {
            resources.health = stats.resources.health;
            connections.OnFullyHealed.BlendInvoke(other?.OnFullyHealed);
        }
    }

    public bool Attack(int pierce, int accuracy)
    {
        connections.OnAttacked.BlendInvoke(other?.OnAttacked, ref pierce, ref accuracy);

        if (accuracy < stats.ev)
        {
            //TODO: Log dodge
            return false;
        }

        if (pierce < stats.ac)
        {
            //TODO: Log break damage
            return false;
        }

        

        //TODO: Log Hit
        //TakeDamage(damage, DamageType.NONE);
        return true;
    }

    public void Damage(int damage, DamageType type, DamageSource source, string message = "{name} take%s{|s} {damage} damage")
    {
        connections.OnTakeDamage.BlendInvoke(other?.OnTakeDamage, ref damage, ref type, ref source);
        resources.health -= damage;

        //Loggingstuff
        string toPrint = FormatStringForName(message).Replace("{damage}", $"{damage}");
        Debug.Log($"Console print: {toPrint}");

        if (resources.health <= 0)
        {
            connections.OnDeath.BlendInvoke(other?.OnDeath);
            if (resources.health <= 0) //Check done for respawn mechanics to take effect
            {
                Die();
            }
        }
    }

    public void Damage(Monster dealer, int damage, DamageType type, DamageSource source, string message = "{name} take%s{|s} {damage} damage")
    {
        dealer.connections.OnDealDamage.BlendInvoke(dealer.other?.OnDealDamage, ref damage, ref type, ref source);
        Damage(damage, type, source, message);

    }

    public virtual void Die()
    {
        Debug.Log("Monster is dead!");

        //Clear tile, so other systems don't try to use a dead monster
        currentTile.currentlyStanding = null;

        //Clear inventory, if it exists
        if (inventory)
        {
            GameAction dropAll = new DropAction(inventory.AllIndices());
            dropAll.Setup(this);
            while (dropAll.action.MoveNext()) { }
        }
    }

    public bool IsDead()
    {
        //Oops, this must be <= 0, Sometimes people can overkill!
        return resources.health <= 0;
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
        return nameRequiresPluralVerbs ? "the " + name : name;
    }
    
    public void AddEnergy(int energy)
    {
        connections.OnEnergyGained.BlendInvoke(other?.OnEnergyGained, ref energy);
        this.energy += energy;
    }

    //TODO: Update this to be individual maps
    public virtual void UpdateLOS()
    {
        this.view = LOS.LosAt(Map.current, location, visionRadius);
    }

    public void StartTurn()
    {
        CallRegenerateStats();
        abilities?.CheckAvailability();
        connections.OnTurnStartLocal.BlendInvoke(other?.OnTurnStartLocal);
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
        stats = new StatBlock() + baseStats;
        connections.RegenerateStats.BlendInvoke(other?.RegenerateStats, ref stats);
    }

    public void SetAction(GameAction act)
    {
        if (currentAction != null)
        {
            Debug.LogError($"{this.name} had an action set, but it already had an action. Should this be allowed?", this);
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
                yield return currentAction.action.Current;
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
            e.Connect(this.connections);
            effects.Add(e);
        }
    }

    //Takes in status effects (uninstantiated by default) and adds them to the effects list.
    public void AddEffect(params StatusEffect[] effectsToAdd)
    {
        Effect[] instEffects = effectsToAdd.Select(x => x.Instantiate()).ToArray();
        AddEffect(instEffects);
    }

    //TODO: Add cost of moving from on spot to another
    public void SetPosition(Vector2Int newPosition)
    {
        if (currentTile) currentTile.currentlyStanding = null;
        location = newPosition;
        transform.position = new Vector3(location.x, location.y, monsterZPosition);
        currentTile = Map.current.GetTile(location);
        currentTile.SetMonster(this);
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

    public void GainResources(ResourceList resources)
    {
        connections.OnGainResources.BlendInvoke(other?.OnGainResources, ref resources);
        this.resources += resources;
    }

    public void LoseResources(ResourceList resources)
    {
        connections.OnLoseResources.BlendInvoke(other?.OnLoseResources, ref resources);
        this.resources -= resources;
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
}
