using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PipIcon : MonoBehaviour
{
    [SerializeField] Image iamge;
    [SerializeField] TextMeshProUGUI text;
    PipEffect effect;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupForEffect(PipEffect effect)
    {
        this.effect = effect;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        iamge.sprite = effect.GetImage();
        text.text = effect.stackCount.ToString();
    }

    public void UpdateCount(int delta)
    {
        text.text = effect.stackCount.ToString();
    }

}
