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
    public Class chosenClass;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Setup();
        player = this;
    }

    //Special case, because it affects the world around it through the player's view.
    public override void UpdateLOS()
    {
        base.UpdateLOS();
        LOS.WritePlayerLOS();
        AnimationController.AddAnimationForMonster(new PlayerLOSAnimation(this), this);
    }

    public override int XPTillNextLevel()
    {
        return 15;
    }

    public override void GainXP(Monster source, float amount)
    {
        if (amount == 0)
        {
            RogueLog.singleton.LogTemplate("NoXP", null, priority: LogPriority.GENERIC);
        }

        while (amount > 0)
        {
            if (source.level < level)
            {
                RogueLog.singleton.LogTemplate("NoXP", null, priority: LogPriority.GENERIC);
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
            priority: LogPriority.GENERIC
            );

            baseStats[XP] += amount;
            amount = 0;
        }
    }

    public override void OnLevelUp()
    {
        RogueLog.singleton.Log("You level up!", this.gameObject, LogPriority.IMPORTANT);
    }

    protected override void Die(Monster killer)
    {
        base.Die(killer);
        if (baseStats[HEALTH] <= 0)
        {
            Remove();
            Debug.Log("Game over!");
            SceneManager.LoadScene("CharacterSelect");
        }
    }
}