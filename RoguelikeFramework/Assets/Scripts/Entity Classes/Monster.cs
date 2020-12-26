using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public float energy;

    public Vector2Int location;

    public int visionRadius;

    public int energyPerStep;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void AddEnergy(int energy)
    {
        this.energy += energy;
    }

    public void TakeTurn()
    {
        Debug.Log("Monster taking turn", this);
        energy -= 100;
    }

    //TODO: Add cost of moving from on spot to another
    public void Move(Vector2Int newPosition)
    {
        location = newPosition;
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

        energy -= (energyPerStep * Map.singleton.MovementCostAt(location));
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
                break;
            case Direction.SOUTH_EAST:
                return location + new Vector2Int(1, -1);
                break;
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
        transform.position = new Vector3(location.x, location.y, -5);
    }
}
