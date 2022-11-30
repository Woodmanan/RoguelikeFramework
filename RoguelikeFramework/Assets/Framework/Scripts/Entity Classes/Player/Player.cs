using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using static Resources;

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

    [HideInInspector] public Effect personalAttribute;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Setup();
        player = this;
        if (level >= 5)
        {
            AddEffectInstantiate(personalAttribute);
        }
    }

    //Special case, because it affects the world around it through the player's view.
    public override void UpdateLOS()
    {
        view = LOS.GeneratePlayerLOS(Map.current, location, visionRadius);
        view.CollectEntities(Map.current);
        UpdateLOSPostCollection();
    }

    public override int XPTillNextLevel()
    {
        return 15;
    }

    public override void GainXP(Monster source, float amount)
    {
        while (amount > 0)
        {
            if (source.level < level)
            {
                Debug.Log("Console: That felt unrewarding.");
                return;
            }

            if (amount >= (XPTillNextLevel() - baseStats[XP]))
            {
                amount -= (XPTillNextLevel() - (int) baseStats[XP]);
                baseStats[XP] = 0;
                LevelUp();
                continue;
            }

            baseStats[XP] += amount;
            amount = 0;
        }
    }

    public override void OnLevelUp()
    {
        Debug.Log("Log: YOU LEVEL UP!");
        if (level == 5)
        {
            AddEffectInstantiate(personalAttribute);
        }
    }

    protected override void Die()
    {
        Remove();
        if (baseStats[HEALTH] <= 0)
        {
            Debug.Log("Game over!");
        }
    }
}