using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class RogueLogDisplay : MonoBehaviour
{
    public TextMeshProUGUI messageBox;

    public LogPriority priorityToDisplay;

    public Scrollbar scrollBar;
    public ScrollRect ScrollView;
    public RectTransform contentBox;
    public GameObject messageObject;

    bool dirty = true;

    // Start is called before the first frame update
    void Start()
    {
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
        ScrollView.gameObject.SetActive(priorityToDisplay != LogPriority.NONE);
    }

    void UpdateMessageSize()
    {
        //Add messages
        /*while (displayedMessages.Count < RogueLog.singleton.storedMessageCount)
        {
            GameObject newMessage = Instantiate(messageObject, contentBox);
            newMessage.transform.parent.gameObject.SetActive(true);
            displayedMessages.Add(newMessage.GetComponentInChildren<TextMeshProUGUI>());
        }

        //Turn off (but don't reclaim) any extra messages.
        for (int i = RogueLog.singleton.storedMessageCount; i < displayedMessages.Count; i++)
        {
            displayedMessages[i].transform.parent.gameObject.SetActive(false);
        }*/

        RefreshMessageContent();
    }

    void RefreshMessageContent()
    {
        StringBuilder sb = new StringBuilder(100 * 40);
        int index = 0;
        foreach (RogueLogMessage log in RogueLog.singleton.GetMessagesWithPriority(priorityToDisplay))
        {
            if (index != 0)
            {
                sb.Append("\n");
            }
            sb.Append(log.message);
            if (log.count > 1)
            {
                sb.Append(" x");
                if (log.count <= 99)
                {
                    sb.Append(log.count);
                }
                else
                {
                    sb.Append("99+");
                }
            }
            index++;
        }

        messageBox.text = sb.ToString();

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
