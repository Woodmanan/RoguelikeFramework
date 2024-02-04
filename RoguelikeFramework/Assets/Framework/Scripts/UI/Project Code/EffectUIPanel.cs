using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectUIPanel : MonoBehaviour
{
    [SerializeField] GameObject widgetPrefab;

    List<EffectUIWidget> widgets;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
            transform.GetChild(i).parent = null;
        }

        widgets = new List<EffectUIWidget>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.player)
        {
            while (Player.player[0].effects.Count > transform.childCount)
            {
                widgets.Add(Instantiate(widgetPrefab, transform).GetComponent<EffectUIWidget>());
            }

            int numActive = 0;

            for (int i = 0; i < Player.player[0].effects.Count; i++)
            {
                if (!Player.player[0].effects[i].ReadyToDelete && Player.player[0].effects[i].ShouldDisplay())
                {
                    widgets[numActive].ShowEffect(Player.player[0].effects[i]);
                    numActive++;
                }
            }

            for (int i = numActive; i < transform.childCount; i++)
            {
                widgets[i].Hide();
            }
        }
    }
}
