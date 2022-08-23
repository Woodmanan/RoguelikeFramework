using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************
 * World
 **************************
 * 
 * Top level class that defines how the dungeon is structured
 * Contains branch information, connection info, and the level structures themselves
 * Currently NOT tied to the level loader - leaving those separate just for better 
 * corrections down the line
 */

public class World
{
    public List<Map> levels;
    public List<Branch> branches = new List<Branch>();
    public List<Connection> connections;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
