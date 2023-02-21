using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VersionIndicator : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        #if UNITY_EDITOR
        text.text = $"Editor : {LevelLoader.singleton.seed}";
        #elif DEVELOPMENT_BUILD
        text.text = $"{Application.version} : {LevelLoader.singleton.seed}";
        #else
        gameObject.SetActive(false);
        #endif
    }
}
