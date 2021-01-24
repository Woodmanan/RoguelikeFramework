using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;

public struct EquipmentSlot
{
    SlotType type;
    Item equipped;
}

public class Monster : MonoBehaviour
{
    [HideInInspector] public int health;
    public int maxHealth;

    public int ac;
    public int ev;

    public float energy;

    public Vector2Int location;

    public int visionRadius;

    public int energyPerStep;

    private static readonly float monsterZPosition = -5f;

    //Empty Events
    public event Action RegenerateStats;
    public event Action OnTurnStartGlobal; //Filled
    public event Action OnTurnEndGlobal; //Filled
    public event Action OnTurnStartLocal; //Filled
    public event Action OnTurnEndLocal; //Filled
    public event Action OnMove; //Filled out!
    public event Action OnFullyHealed; // Filled out!
    public event Action OnDeath; //Filled


    //EntityEvent Events
    public event ActionRef<int> OnEnergyGained; //Filled out!
    public event ActionRef<int, int, int> OnAttacked; //Needs to be figured out.
    public event ActionRef<int> OnHealing; //Filled!
    public event ActionRef<Effect[]> OnApplyStatusEffects; //Filled!
    


    public List<Effect> effects;
    public List<Item> inventory;
    public List<EquipmentSlot> equipment;

    
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
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
        if (health >= maxHealth)
        {
            health = maxHealth;
            OnFullyHealed?.Invoke();
        }
    }

    public void Attack(int pierce, int accuracy, int damage)
    {
        OnAttacked?.Invoke(ref pierce, ref accuracy, ref damage);

        if (pierce < ac)
        {
            //TODO: Log break damage
            return;
        }

        if (accuracy < ev)
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
        LocalTurn();
        OnTurnEndLocal?.Invoke();
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            if (effects[i].ReadyToDelete)
            {
                effects.RemoveAt(i);
            }
        }
    }

    public virtual void LocalTurn()
    {
        energy -= 100;
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
}
