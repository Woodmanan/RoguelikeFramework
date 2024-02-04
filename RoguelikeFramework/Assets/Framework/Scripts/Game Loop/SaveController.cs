using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

using MonsterHandle = RogueHandle<Monster>;

public class SaveController : MonoBehaviour
{
    public string fileName;

    public bool trigger;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (trigger)
        {
            trigger = false;

            DemoSave();
        }
    }

    public void DemoSave()
    {
        RogueSaveSystem.BeginWriteSaveFile(fileName);
        RogueSaveSystem.Write(LevelLoader.singleton.seed);
        RogueSaveSystem.Write(GameController.singleton.turn);
        RogueDataStorage.SaveArenas();
        RogueSaveSystem.Write(LevelLoader.maps);

        RogueSaveSystem.Write(Player.player);


        RogueSaveSystem.CloseSaveFile();

        RogueSaveSystem.BeginReadSaveFile(fileName);

        RogueSaveSystem.CloseSaveFile(false);
    }
}
