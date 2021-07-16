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

            player.AddEnergy(energyPerTurn);
            while (player.energy > 0)
            {
                //Set up local turn
                player.StartTurn();
                float currentEnergy = player.energy;

                //Hold until an action is taken
                while (player.energy == currentEnergy)
                {
                    player.TakeTurn();

                    //Freeze for player turn to finis, should that be necessary
                    if (player.turnRoutine != null)
                    {
                        yield return player.turnRoutine;
                    }

                    if (player.energy == currentEnergy)
                    {
                        yield return null;
                    }
                    else
                    {
                        break;
                    }
                }

                player.EndTurn();

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
                while (m.energy > 0)
                {
                    m.TakeTurn();

                    //Wait for monsters who need more frames for their turn
                    if (m.turnRoutine != null)
                    {
                        watch.Stop();
                        yield return m.turnRoutine;
                        watch.Restart();
                    }

                    m.EndTurn();
                }
            }
            
            CallTurnEndGlobal();

            //Finished calls for monster update, take turns for the players
            yield return null;
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
