using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public int energy;

    public Vector2Int location;
    
    // Start is called before the first frame update
    void Start()
    {
        energy = 0;
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
        energy -= 100;
    }
}
