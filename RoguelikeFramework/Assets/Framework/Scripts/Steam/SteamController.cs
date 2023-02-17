using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

//A true singleton! If it does not exist, make it. If it does exist, there is NO reason to ever drop it.
public class SteamController : MonoBehaviour
{
    private static SteamController Singleton;
    public static SteamController singleton
    {
        get
        {
            if (Singleton == null)
            {
                SteamController extantController = GameObject.Find("Steam Controller")?.GetComponent<SteamController>();
                if (extantController)
                {
                    Singleton = extantController;
                }
                else
                {
                    GameObject holder = new GameObject("Steam Controller");
                    Singleton = holder.AddComponent<SteamController>();
                    DontDestroyOnLoad(holder);
                }
            }

            //Try to establish a conneciton, if we need one.
            if (!Singleton.EstablishConnection())
            {
                Debug.LogError("Could not establish steam connection through singleton call.");
            }

            return Singleton;
        }

        set
        {
            Singleton = value;
            DontDestroyOnLoad(Singleton.gameObject);
        }
    }

    #if UNITY_EDITOR
    [SerializeField]
    bool DevSkipConnection = false;
    #endif

    private bool connected = false;

    public bool EstablishConnection()
    {
        if (!connected)
        {
            try
            {
                Steamworks.SteamClient.Init(2191640);
            }
            catch (System.Exception e)
            {
                // Something went wrong - it's one of these:
                //
                //     Steam is closed?
                //     Can't find steam_api dll?
                //     Don't have permission to play app?
                //
                Debug.LogError("Uh oh something went wrong. No steam for u cuz " + e.Message);
                return false;
            }

            Debug.Log("Established steam connection!");
            connected = true;
        }

        return connected;
    }


    // Start is called before the first frame update
    void Start()
    {
        if (Singleton == null)
        {
            singleton = this;
        }

        if (Singleton != this)
        {
            Destroy(this.gameObject);
            return;
        }

        if (this.name != "Steam Controller")
        {
            Debug.LogError("Sorry, you have to name it 'Steam Controller'");
            this.name = "Steam Controller";
        }

        #if UNITY_EDITOR
        if (DevSkipConnection)
        {
            connected = true;
        }
        #endif

        if (!EstablishConnection())
        {
            Debug.LogError("Could not establish connection on startup.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (connected)
        {
            Steamworks.SteamClient.RunCallbacks();
        }
    }

    private void OnDestroy()
    {
        if (connected)
        {
            Debug.Log("Shutting down steam connection");
            Steamworks.SteamClient.Shutdown();
        }
    }

    public void GiveAchievement(string name)
    {
        var achievement = new Steamworks.Data.Achievement(name);
        achievement.Trigger();
    }

    public void ClearAchievement(string name)
    {
        var achievement = new Steamworks.Data.Achievement(name);
        achievement.Clear();
    }

    public bool HasUnlockedAchievement(string name)
    {
        var achievement = new Steamworks.Data.Achievement(name);
        return achievement.State;
    }

    //Dangerous button. Only ref SHOULD be the cheat component.
    public void ClearAllAchievements()
    {
        foreach (var achievement in Steamworks.SteamUserStats.Achievements)
        {
            achievement.Clear();
        }
    }

    public void PrintDiagnostics()
    {
        Debug.Log($"Steam is connected, controller report: {connected}");
        Debug.Log($"Steam currently logged on: {Steamworks.SteamClient.IsLoggedOn}");
        Debug.Log($"Steam user ID: {Steamworks.SteamClient.Name} : {Steamworks.SteamClient.SteamId}");
    }
}
