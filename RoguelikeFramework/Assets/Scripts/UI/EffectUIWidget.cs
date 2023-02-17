using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectUIWidget : MonoBehaviour, IDescribable
{
    Effect effect;

    public Image mainImage;
    public Image overlay;
    public TextMeshProUGUI text;

    public string GetDescription()
    {
        return effect.GetDescription();
    }

    public Sprite GetImage()
    {
        return effect.GetImage();
    }

    public string GetName(bool shorten = false)
    {
        return effect.GetName(shorten);
    }

    public void ShowEffect(Effect effect)
    {
        this.effect = effect;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mainImage.sprite = GetImage();
        text.text = effect.GetUISubtext();
        overlay.fillAmount = effect.GetUIFillPercent();
    }
}
