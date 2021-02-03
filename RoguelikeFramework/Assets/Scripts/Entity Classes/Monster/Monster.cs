using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;



public class Monster : MonoBehaviour
{
    [Header("Setup Variables")]
    public StatBlock baseStats;
    public StatBlock stats;


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
    public event Action<StatBlock> RegenerateStats;

    //EntityEvent Events
    public event ActionRef<int> OnEnergyGained; //Filled out!
    public event ActionRef<int, int, int> OnAttacked; //Filled out, TODO: Rework this
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

    public void Heal(int healthReturned)
    {
        OnHealing?.Invoke(ref healthReturned);
        
        health += healthReturned;
        if (health >= stats.maxHealth)
        {
            health = stats.maxHealth;
            OnFullyHealed?.Invoke();
        }
    }

    public void Attack(int pierce, int accuracy, int damage)
    {
        OnAttacked?.Invoke(ref pierce, ref accuracy, ref damage);

        if (pierce < stats.ac)
        {
            //TODO: Log break damage
            return;
        }

        if (accuracy < stats.ev)
        {
            //TODO: Log dodge
        }

        //TODO: Log Hit
        TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
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
    
    public void AddEnergy(int energy)
    {
        OnEnergyGained?.Invoke(ref energy);
        this.energy += energy;
    }

    public void TakeTurn()
    {
        OnTurnStartLocal?.Invoke();
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
        location = newPosition;
        transform.position = new Vector3(location.x, location.y, monsterZPosition);
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
        location += offset;
        transform.position = new Vector3(location.x, location.y, monsterZPosition);
        energy -= (energyPerStep * Map.singleton.MovementCostAt(location));
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
