using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Steamworks")]
public class GrantAchievementSystem : DungeonSystem
{
    public string AchievementName;
    bool hasTriggered = false;
    public override void OnEnterLevel(Map m)
    {
        if (!hasTriggered)
        {
            if (SteamController.singleton)
            {
                SteamController.singleton.GiveAchievement(AchievementName);
                hasTriggered = true;
            }
        }
    }
}
