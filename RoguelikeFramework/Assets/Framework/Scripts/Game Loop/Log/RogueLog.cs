using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct RogueLogMessage
{
    public string message;
    public GameObject source;
    public int count;
    public LogPriority priority;
    public LogDisplay display;
    public object context;
}

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

    bool usesFloatingMessages;
    bool stacksMessages;
    public int storedMessageCount = 50;

    LinkedList<RogueLogMessage> messages = new LinkedList<RogueLogMessage>();

    public Action OnLogUpdated;
    public ActionRef<RogueLogMessage> OnDisplayLogAdded;


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

    public void Log(string message, GameObject source = null, LogPriority priority = LogPriority.LOW, LogDisplay display = LogDisplay.NONE, object context = null)
    {
        RogueLogMessage LogMessage = new RogueLogMessage();
        LogMessage.message = message;
        LogMessage.source = source;
        LogMessage.priority = priority;
        LogMessage.display = display;
        LogMessage.context = context;
        LogMessage.count = 1;
        Log(LogMessage);
    }
    
    public void Log(RogueLogMessage message)
    {
        if (message.priority != LogPriority.NONE)
        {
            if (messages.Last != null && messages.Last.Value.message == message.message)
            {
                message.count += messages.Last.Value.count;
                messages.RemoveLast();
                messages.AddLast(message);
            }
            else
            {
                if (messages.Count == storedMessageCount)
                {
                    messages.RemoveFirst();
                }
                messages.AddLast(message);
            }
        }

        if (message.display != LogDisplay.NONE)
        {
            //Setup the floating message!
            OnDisplayLogAdded?.Invoke(ref message);
        }

        OnLogUpdated?.Invoke();

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        //Copy player logs over for maximum info
        Debug.Log($"Console: {message.message}");
        #endif
    }

    public IEnumerable<RogueLogMessage> GetMessagesAbovePriority(LogPriority priority)
    {
        foreach (RogueLogMessage message in messages)
        {
            if (message.priority >= priority)
            {
                yield return message;
            }
        }
        yield break;
    }
}
