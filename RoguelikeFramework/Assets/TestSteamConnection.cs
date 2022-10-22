﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSteamConnection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
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
            Debug.LogError("Uh oh something went wrong no steam for u cuz " + e.Message);
            return;
        }

        //Got here, so steam is initialized! Attach some callbacks.
        Steamworks.Dispatch.OnException = (e) =>
        {
            Debug.LogError(e.Message);
            Debug.LogError(e.StackTrace);
        };

        var mySteamId = Steamworks.SteamClient.SteamId;
        Debug.Log("My steam id is " + mySteamId);
    }

    // Update is called once per frame
    void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    private void OnDestroy()
    {
        Debug.Log("Tearing down!");
        Steamworks.SteamClient.Shutdown();
    }
}
