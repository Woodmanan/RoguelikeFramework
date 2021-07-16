using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using System.Text.RegularExpressions; //Oh god oh fuck



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

    [HideInInspector] public Coroutine turnRoutine;

    //Empty Events
    public event Action OnTurnStartGlobal; //Filled
    public event Action OnTurnEndGlobal; //Filled
    public event Action OnTurnStartLocal; //Filled
    public event Action OnTurnEndLocal; //Filled
    public event Action OnMove; //Filled out!
    public event Action OnFullyHealed; // Filled out!
    public event Action OnDeath; //Filled

    //Special statblock event
    public event ActionRef<StatBlock> RegenerateStats; //TODO: Review when this should happen? Lots of weird problems with this one

    //EntityEvent Events
    public event ActionRef<int> OnEnergyGained; //Filled out!
    public event ActionRef<int, int> OnAttacked; //Filled out, TODO: Rework this
    public event ActionRef<int, DamageType> OnTakeDamage;
    public event ActionRef<int> OnHealing; //Filled!
    public event ActionRef<Effect[]> OnApplyStatusEffects; //Filled!
    


    public List<Effect> effects;
    public Inventory inventory;
    public Equipment equipment;
    
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
        OnHealing?.Invoke(ref healthReturned);

        if (healthReturned > 0) //Negative health healing is currently just ignored, for clarity's sake
        {
            health += healthReturned;
        }
        if (health >= stats.maxHealth)
        {
            health = stats.maxHealth;
            OnFullyHealed?.Invoke();
        }
    }

    public bool Attack(int pierce, int accuracy)
    {
        OnAttacked?.Invoke(ref pierce, ref accuracy);

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
        OnTakeDamage?.Invoke(ref damage, ref type);
        health -= damage;

        //Loggingstuff
        string toPrint = FormatStringForName(message).Replace("{damage}", $"{damage}");
        Debug.Log($"Console print: {toPrint}");

        if (health <= 0)
        {
            OnDeath?.Invoke();
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
        OnEnergyGained?.Invoke(ref energy);
        this.energy += energy;
    }

    public void StartTurn()
    {
        CallRegenerateStats();
        OnTurnStartLocal?.Invoke();
    }

    public void TakeTurn()
    {
        turnRoutine = StartCoroutine(LocalTurn());
    }

    public void EndTurn()
    {
        OnTurnEndLocal?.Invoke();
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
        RegenerateStats?.Invoke(ref stats);
    }

    //Takes the local turn
    public virtual IEnumerator LocalTurn()
    {
        energy -= 100;

        //Here so the compiler doesn't complain
        if (false)
        {
            yield return null;
        }

        EndTurn();
    }



    public void AddEffect(params Effect[] effectsToAdd)
    {
        OnApplyStatusEffects?.Invoke(ref effectsToAdd);
        for (int i = 0; i < effectsToAdd.Length; i++)
        {
            Effect e = effectsToAdd[i];
            e.Connect(this);
            effects.Add(e);
        }
    }

    //TODO: Add cost of moving from on spot to another
    public void SetPosition(Vector2Int newPosition)
    {
        Map.singleton.GetTile(location).currentlyStanding = null;
        location = newPosition;
        transform.position = new Vector3(location.x, location.y, monsterZPosition);
        Map.singleton.GetTile(location).currentlyStanding = this;
    }

    public void MoveUnit(Direction dir)
    {
        switch (dir)
        {
            case Direction.NORTH:
                MoveStep(Vector2Int.up);
                break;
            case Direction.SOUTH:
                MoveStep(Vector2Int.down);
                break;
            case Direction.EAST:
                MoveStep(Vector2Int.right);
                break;
            case Direction.WEST:
                MoveStep(Vector2Int.left);
                break;
            case Direction.NORTH_EAST:
                MoveStep(new Vector2Int(1, 1));
                break;
            case Direction.NORTH_WEST:
                MoveStep(new Vector2Int(-1, 1));
                break;
            case Direction.SOUTH_EAST:
                MoveStep(new Vector2Int(1, -1));
                break;
            case Direction.SOUTH_WEST:
                MoveStep(new Vector2Int(-1, -1));
                break;
        }
    }

    public Vector2Int GetUnitStep(Direction dir)
    {
        switch (dir)
        {
            case Direction.NORTH:
                return location + Vector2Int.up;
            case Direction.SOUTH:
                return location + Vector2Int.down;
            case Direction.EAST:
                return location + Vector2Int.right;
            case Direction.WEST:
                return location + Vector2Int.left;
            case Direction.NORTH_EAST:
                return location + new Vector2Int(1, 1);
            case Direction.NORTH_WEST:
                return location + new Vector2Int(-1, 1);
            case Direction.SOUTH_EAST:
                return location + new Vector2Int(1, -1);
            case Direction.SOUTH_WEST:
                return location + new Vector2Int(-1, -1);
            default:
                Debug.LogError("Player attempted to get direction with no actual direction");
                return location;
        }
    }

    private void MoveStep(Vector2Int offset)
    {
        Map.singleton.GetTile(location).ClearMonster();
        location += offset;
        transform.position = new Vector3(location.x, location.y, monsterZPosition);
        energy -= (energyPerStep * Map.singleton.MovementCostAt(location));
        Map.singleton.GetTile(location).SetMonster(this);
        OnMove?.Invoke();
    }

    //Function to activate event call of Global turn start
    public void OnTurnStartGlobalCall()
    {
        OnTurnStartGlobal?.Invoke();
    }

    //Same purpose as above
    public void OnTurnEndGlobalCall()
    {
        OnTurnEndGlobal?.Invoke();
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
