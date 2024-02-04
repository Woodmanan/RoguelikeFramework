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
    private static RogueHandle<Monster> _player;
    public static RogueHandle<Monster> player
    {
        get
        {
            return _player;
        }
        set
        {
            _player = value;
        }
    }

    public LocalizedString speciesName;
    public Class chosenClass;

    //Special case, because it affects the world around it through the player's view.
    public override void UpdateLOS()
    {
        base.UpdateLOS();
        LOS.WritePlayerLOS();
        AnimationController.AddAnimationForMonster(new PlayerLOSAnimation(selfHandle), this);
    }

    public override int XPTillNextLevel()
    {
        return 15;
    }

    public override void GainXP(RogueHandle<Monster> source, float amount)
    {
        if (amount == 0)
        {
            RogueLog.singleton.LogTemplate("NoXP", null, priority: LogPriority.GENERIC);
        }

        while (amount > 0)
        {
            if (source[0].level < level)
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
        RogueLog.singleton.Log("You level up!", unity.gameObject, LogPriority.IMPORTANT);
    }

    protected override void Die(RogueHandle<Monster> killer)
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