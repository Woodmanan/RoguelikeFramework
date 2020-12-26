using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;



public class GameController : MonoBehaviour
{
    //Constant variables: change depending on runtime!
    public const long MONSTER_UPDATE_MS = 5;
    
    public int turn;

    public int energyPerTurn;

    public Player player;

    public List<Monster> monsters;
    
    // Start is called before the first frame update
    void Start()
    {
        turn = 0;
        StartCoroutine(TakeTurns());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator TakeTurns()
    {
        //1 Frame pause to set up LOS
        yield return null;
        LOS.GeneratePlayerLOS(player.location, player.visionRadius);
        
        //Main loop
        while (true)
        {
            turn++;
            player.AddEnergy(energyPerTurn);
            while (player.energy > 0)
            {
                player.TakeTurn();
                yield return null;
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < monsters.Count; i++)
            {
                //Taken too much time? Quit then! (Done before monster update to handle edge case on last call)
                if (watch.ElapsedMilliseconds > MONSTER_UPDATE_MS)
                {
                    watch.Stop();
                    yield return null;
                    watch.Restart();
                }

                Monster m = monsters[i];
                m.AddEnergy(energyPerTurn);
                m.TakeTurn();
            }

            //Finished calls for monster update, take turns for the players
            yield return null;
        }
    }

    public void AddMonster(Monster m)
    {
        monsters.Add(m);
    }
}
