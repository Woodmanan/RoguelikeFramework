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
        StartCoroutine(BeginGame());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator BeginGame()
    {
        //1 Frame pause to set up LOS
        yield return null;
        LOS.GeneratePlayerLOS(player.location, player.visionRadius);

        //Monster setup, before the loop starts
        player.PostSetup();
        foreach (Monster m in monsters)
        {
            m.PostSetup();
        }

        //Move our camera onto the player for the first frame
        CameraTracking.singleton.JumpToPlayer();


        //Space for any init setup that needs to be done
        StartCoroutine(GameLoop());

        
    }

    IEnumerator GameLoop()
    {
        //Main loop
        while (true)
        {
            
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
                    if (turn.Current != GameAction.StateCheck)
                    {
                        yield return turn.Current;
                    }
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
                    //Set up local turn
                    m.StartTurn();

                    //Run the actual turn itself
                    IEnumerator turn = m.LocalTurn();
                    while (m.energy > 0 && turn.MoveNext())
                    {
                        if (turn.Current != GameAction.StateCheck)
                        {
                            watch.Stop();
                            yield return turn.Current;
                            watch.Restart();
                        }
                    }

                    //Turn is ended!
                    m.EndTurn();
                }
            }
            
            CallTurnEndGlobal();

            turn++;
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
