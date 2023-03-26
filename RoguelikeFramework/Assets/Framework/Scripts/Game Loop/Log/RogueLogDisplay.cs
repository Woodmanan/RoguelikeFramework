using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RogueLogDisplay : MonoBehaviour
{
    List<TextMeshProUGUI> displayedMessages;

    public LogPriority priorityToDisplay;

    public Scrollbar scrollBar;
    public ScrollRect ScrollView;
    public RectTransform contentBox;
    public GameObject messageObject;

    bool dirty = true;

    // Start is called before the first frame update
    void Start()
    {
        displayedMessages = new List<TextMeshProUGUI>();
        RogueLog.singleton.OnLogUpdated += SetDirty;
        UpdateMessageSize();
    }

    // Update is called once per frame
    void Update()
    {
        if (dirty)
        {
            RefreshMessageContent();
        }
    }

    void SetDirty()
    {
        dirty = true;
    }

    public void UpdatePriority(int newPriority)
    {
        priorityToDisplay = (LogPriority)newPriority;
        dirty = true;
        ScrollView.gameObject.SetActive(newPriority != 0);
    }

    void UpdateMessageSize()
    {
        //Add messages
        while (displayedMessages.Count < RogueLog.singleton.storedMessageCount)
        {
            GameObject newMessage = Instantiate(messageObject, contentBox);
            newMessage.gameObject.SetActive(true);
            displayedMessages.Add(newMessage.GetComponentInChildren<TextMeshProUGUI>());
        }

        //Turn off (but don't reclaim) any extra messages.
        for (int i = RogueLog.singleton.storedMessageCount; i < displayedMessages.Count; i++)
        {
            displayedMessages[i].gameObject.SetActive(false);
        }

        RefreshMessageContent();
    }

    void RefreshMessageContent()
    {
        int index = 0;
        foreach (RogueLogMessage log in RogueLog.singleton.GetMessagesAbovePriority(priorityToDisplay))
        {
            displayedMessages[index].text = log.message;
            if (log.count > 1)
            {
                if (log.count <= 99)
                {
                    displayedMessages[index].text += $" x{log.count}";
                }
                else
                {
                    displayedMessages[index].text += $" x99+";
                }
            }
            displayedMessages[index].gameObject.SetActive(true);
            index++;
        }

        for (int i = index; i < displayedMessages.Count; i++)
        {
            displayedMessages[i].gameObject.SetActive(false);
        }

        //Reset view on update!
        StartCoroutine(ResetScroll());
        dirty = false;
    }

    IEnumerator ResetScroll()
    {
        scrollBar.value = 0;
        yield return null;
        scrollBar.value = 0;
        yield return null;
        scrollBar.value = 0;
    }
}
