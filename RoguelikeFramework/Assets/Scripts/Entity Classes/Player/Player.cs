﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class Player : Monster
{
    private static Monster _player;
    public static Monster player
    {
        get
        {
            if (_player == null)
            {
                try
                {
                    _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
                }
                catch
                {
                    Debug.LogWarning("Effect chunk called on player before they could be found.");
                }
            }
            return _player;
        }
        set
        {
            _player = value;
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Setup();
        player = this;
        Player.player.connections.OnTurnStartLocal.AddListener(1000, OnTurnStart);
    }

    //Special case, because it affects the world around it through the player's view.
    public override void UpdateLOS()
    {
        view = LOS.GeneratePlayerLOS(Map.current, location, visionRadius);

    }

    public override int XPTillNextLevel()
    {
        baseStats.resources.xp = level;
        return level;
    }

    public override void OnLevelUp()
    {
        Debug.Log("Log: LEVEL UP!");
    }

    public override void Die()
    {
        Remove();
        if (resources.health <= 0)
        {
            Debug.Log("Game over!");
        }
    }

    public void OnTurnStart()
    {

    }
}