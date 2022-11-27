using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    Monster player;
    Material mat;
    Image image;

    public Resources main;
    public Resources max;
    public Color fillColor;
    public bool alwaysShow;

    float currentFillAmount;
    // Start is called before the first frame update
    void Start()
    {
        image = transform.GetChild(0).GetComponent<Image>();
        mat = Instantiate<Material>(image.material);
        image.material = mat;
        mat.SetColor("_fillColor", fillColor);
        currentFillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Player.player;
        }

        if (player != null && mat != null)
        {
            if (player.currentStats[max] > 0 || alwaysShow)
            {
                if (!image.enabled)
                {
                    image.enabled = true;
                }
                float goalFillAmount = player.currentStats[main] / Mathf.Max(player.currentStats[max], 0.001f);
                currentFillAmount = Mathf.Lerp(currentFillAmount, goalFillAmount, .05f);
                if (Mathf.Abs(goalFillAmount - currentFillAmount) < .005)
                {
                    currentFillAmount = goalFillAmount;
                }
                mat.SetFloat("_fillAmount", currentFillAmount);
            }
            else
            {
                if (image.enabled)
                {
                    image.enabled = false;
                }
            }
        }
    }
}
