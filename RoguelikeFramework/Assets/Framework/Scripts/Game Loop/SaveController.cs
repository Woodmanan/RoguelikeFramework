using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            RogueSaveSystem.BeginWriteSaveFile(fileName);
            RogueSaveSystem.Write(GameController.singleton);
            RogueSaveSystem.Write(Map.current);
            RogueSaveSystem.CloseSaveFile();
        }
    }
}
