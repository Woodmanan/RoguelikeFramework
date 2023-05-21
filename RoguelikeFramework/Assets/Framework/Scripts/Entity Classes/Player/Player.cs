using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using static Resources;
using UnityEngine.Localization;

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

    public LocalizedString speciesName;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Setup();
        Debug.LogError($"Player has player tag? {tags.HasTag("Player")}");
        player = this;
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
        if (amount == 0)
        {
            RogueLog.singleton.LogTemplate("NoXP", null, priority: LogPriority.LOW);
        }

        while (amount > 0)
        {
            if (source.level < level)
            {
                RogueLog.singleton.LogTemplate("NoXP", null, priority: LogPriority.LOW);
                return;
            }

            if (amount >= (XPTillNextLevel() - baseStats[XP]))
            {
                amount -= (XPTillNextLevel() - (int) baseStats[XP]);
                baseStats[XP] = 0;
                LevelUp();
                continue;
            }

            RogueLog.singleton.LogTemplate("XP",
            new { monster = GetName(), singular = singular, amount = Mathf.RoundToInt(amount) },
            priority: LogPriority.LOW
            );

            baseStats[XP] += amount;
            amount = 0;
        }
    }

    public override void OnLevelUp()
    {
        RogueLog.singleton.Log("You level up!", this.gameObject, LogPriority.HIGH, display: LogDisplay.STANDARD);
    }

    protected override void Die()
    {
        Remove();
        if (baseStats[HEALTH] <= 0)
        {
            Debug.Log("Game over!");
            SceneManager.LoadScene("CharacterSelect");
        }
    }
}