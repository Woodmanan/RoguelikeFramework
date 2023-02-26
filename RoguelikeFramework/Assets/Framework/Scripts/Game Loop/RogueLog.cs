using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueLog : MonoBehaviour
{
    private static RogueLog Singleton;
    public static RogueLog singleton
    {
        get
        {
            if (Singleton == null)
            {
                RogueLog extantController = FindObjectOfType<RogueLog>();
                if (extantController)
                {
                    Singleton = extantController;
                }
                else
                {
                    GameObject holder = new GameObject("Logging Controller");
                    Singleton = holder.AddComponent<RogueLog>();
                }
            }
            return Singleton;
        }

        set
        {
            Singleton = value;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        if (Singleton != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Log(string message)
    {
        //Generate a random screen coord around the player
        float maxRad = Mathf.Min(Screen.width, Screen.height) / 2;
        float r = maxRad * Mathf.Sqrt(RogueRNG.Linear(0.04f, .64f)); //Distribute  evenishly
        float theta = Random.value * Mathf.PI * 2;
        Vector2 pos = (new Vector2(Screen.width, Screen.height) / 2) + r * new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
        FloatingController.singleton.AddBasicMessage(message, pos);
    }

    public void LogAboveMonster(string message, Monster monster)
    {
        FloatingController.singleton.AddWorldMessage(message, monster.location + new Vector2(0f, 3f));
    }
}
