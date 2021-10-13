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

    //TODO: Abstract these out to another class!!!
    new public string name;
    public bool nameRequiresPluralVerbs; //Useful for the player!


    [Header("Runtime Attributes")]
    public int health;
    public float energy;

    public Vector2Int location;

    public int visionRadius;

    public int energyPerStep;

    private static readonly float monsterZPosition = -5f;

    //Empty Events
    public OrderedEvent OnTurnStartGlobal = new OrderedEvent(); //Filled
    public OrderedEvent OnTurnEndGlobal = new OrderedEvent(); //Filled
    public OrderedEvent OnTurnStartLocal = new OrderedEvent(); //Filled
    public OrderedEvent OnTurnEndLocal = new OrderedEvent(); //Filled
    public OrderedEvent OnMove = new OrderedEvent(); //Filled out!
    public OrderedEvent OnFullyHealed = new OrderedEvent(); // Filled out!
    public OrderedEvent OnDeath = new OrderedEvent(); //Filled

    //Special statblock event
    public OrderedEvent<StatBlock> RegenerateStats = new OrderedEvent<StatBlock>(); //TODO: Review when this should happen? Lots of weird problems with this one

    //EntityEvent Events
    public OrderedEvent<int> OnEnergyGained = new OrderedEvent<int>(); //Filled out!
    public OrderedEvent<int, int> OnAttacked = new OrderedEvent<int, int>(); //Filled out, TODO: Rework this
    public OrderedEvent<int, DamageType> OnTakeDamage = new OrderedEvent<int, DamageType>();
    public OrderedEvent<int> OnHealing = new OrderedEvent<int>(); //Filled!
    public OrderedEvent<Effect[]> OnApplyStatusEffects = new OrderedEvent<Effect[]>(); //Filled!

    [HideInInspector] public LOSData view;

    public List<Effect> effects;
    public Inventory inventory;
    public Equipment equipment;

    public GameAction currentAction;
    
    // Start is called before the first frame update
    public virtual void Start()
    {
        inventory = GetComponent<Inventory>(); //May need to be set up with Get/Set to avoid null references during Start()!
        equipment = GetComponent<Equipment>();

        //TODO: Have starting equipment? Probably not a huge concern right now, though.
        stats = baseStats;
        health = stats.maxHealth;
        if (OnFullyHealed != null)
        {
            OnFullyHealed.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //TODO: Actually call this! The generator should call this function once monsters have been placed
    public void AfterInitialSetup()
    {
        Map.singleton.GetTile(location).SetMonster(this);
    }

    public void Heal(int healthReturned)
    {
        OnHealing.Invoke(ref healthReturned);

        if (healthReturned > 0) //Negative health healing is currently just ignored, for clarity's sake
        {
            health += healthReturned;
        }
        if (health >= stats.maxHealth)
        {
            health = stats.maxHealth;
            OnFullyHealed.Invoke();
        }
    }

    public bool Attack(int pierce, int accuracy)
    {
        OnAttacked.Invoke(ref pierce, ref accuracy);

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

    public void TakeDamage(int damage, DamageType type, string message = "{name} take%s{|s} {damage} damage")
    {
        OnTakeDamage.Invoke(ref damage, ref type);
        health -= damage;

        //Loggingstuff
        string toPrint = FormatStringForName(message).Replace("{damage}", $"{damage}");
        Debug.Log($"Console print: {toPrint}");

        if (health <= 0)
        {
            OnDeath.Invoke();
            if (health <= 0) //Check done for respawn mechanics to take effect
            {
                Die();
            }
        }
    }

    public virtual void Die()
    {

    }

    private string FormatStringForName(string msg)
    {
        //Handle the {name} formatter
        string newMessage = msg.Replace("{name}", GetFormattedName());

        //Handle the %s{singular form | plural} formatter
        MatchCollection matches = Regex.Matches(newMessage, "%s\\{([a-zA-Z\\w]*\\|[a-zA-Z\\w]*)\\}");
        foreach (Match m in matches)
        {
            Debug.Log($"{m.Groups.Count} matches found");
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

    //Automatically grabs
    public void Attack(Monster target)
    {
        //TODO: Make this more effecient of a lookup
        //List<EquipmentSlot> slots = equipment.equipmentSlots.FindAll(x => x.type.Contains(EquipSlotType.PRIMARY_HAND) || x.type.Contains(EquipSlotType.SECONDARY_HAND));
        //slots = slots.FindAll(x => x.active && x.equipped.held[0].type == ItemType.WEAPON);

        //Correction: see if we have any weapons actively equipped (Doesn't need to be in your hands, theoretically
        List<EquipmentSlot> slots = equipment.equipmentSlots.FindAll(x => x.active && x.equipped.held[0].type == ItemType.WEAPON);

        //Do we have any weapons equipped?
        if (slots.Count > 0)
        {
            //TODO: Sort the list
            List<MeleeWeapon> weapons = new List<MeleeWeapon>();
            foreach (EquipmentSlot s in slots)
            {
                MeleeWeapon weapon = s.equipped.held[0].GetComponent<MeleeWeapon>();
                if (weapon)
                {
                    if (!weapons.Contains(weapon))
                    {
                        weapons.Add(weapon);
                    }
                }
                else
                {
                    Debug.Log("Player somehow equipped something that isn't a weapon.");
                }
            }

            foreach (MeleeWeapon w in weapons)
            {
                w.Use(this, target);
            }
        }
    }
    
    public void AddEnergy(int energy)
    {
        OnEnergyGained.Invoke(ref energy);
        this.energy += energy;
    }

    public virtual void UpdateLOS()
    {
        this.view = LOS.LosAt(location, visionRadius);
    }

    public void StartTurn()
    {
        CallRegenerateStats();
        OnTurnStartLocal.Invoke();
    }

    public void EndTurn()
    {
        OnTurnEndLocal.Invoke();
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
        RegenerateStats.Invoke(ref stats);
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
        SetAction(new WaitAction());

        if (false)
        {
            yield return null;
        }
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
                    Debug.LogError("This error will NOT be caught in build, so please fix it now.");
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
        OnApplyStatusEffects.Invoke(ref effectsToAdd);
        for (int i = 0; i < effectsToAdd.Length; i++)
        {
            Effect e = effectsToAdd[i];
            e.Connect(this);
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
        Map.singleton.GetTile(location).currentlyStanding = null;
        location = newPosition;
        transform.position = new Vector3(location.x, location.y, monsterZPosition);
        Map.singleton.GetTile(location).currentlyStanding = this;
    }

    //Function to activate event call of Global turn start
    public void OnTurnStartGlobalCall()
    {
        OnTurnStartGlobal.Invoke();
    }

    //Same purpose as above
    public void OnTurnEndGlobalCall()
    {
        OnTurnEndGlobal.Invoke();
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
}
