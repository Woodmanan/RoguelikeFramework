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
            CallTurnStartGlobal();

            //Player turn sequence
            player.AddEnergy(energyPerTurn);
            while (player.energy > 0)
            {
                //Set up local turn
                player.StartTurn();
                
                //Run the actual turn itself
                IEnumerator turn = player.LocalTurn();
                while (player.energy > 0 && turn.MoveNext())
                {
                    yield return turn.Current;
                }

                //Turn is ended!
                player.EndTurn();
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
                while (m.energy > 0)
                {
                    //Run the actual turn itself
                    IEnumerator turn = m.LocalTurn();
                    while (m.energy > 0 && turn.MoveNext())
                    {
                        watch.Stop();
                        yield return turn.Current;
                        watch.Restart();
                    }

                    //Turn is ended!
                    m.EndTurn();
                }
            }
            
            CallTurnEndGlobal();

            //Finished calls for monster update, take turns for the players
            //Leaving this call for posterity in case we ever need it, but I think everything is now handled much better
            //by the existing system. Taking this out improves our speed for when the player takes extremely long actions
            //like resting!
            //yield return null;
        }
    }

    public void CallTurnStartGlobal()
    {
        player.OnTurnStartGlobalCall();
        foreach (Monster m in monsters)
        {
            m.OnTurnStartGlobalCall();
        }
    }

    public void CallTurnEndGlobal()
    {
        player.OnTurnEndGlobalCall();
        foreach (Monster m in monsters)
        {
            m.OnTurnEndGlobalCall();
        }
    }

    public void AddMonster(Monster m)
    {
        monsters.Add(m);
    }
}
