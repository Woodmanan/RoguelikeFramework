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
        text.text = $"Editor Build\nSeed {LevelLoader.singleton.seed}";
        #elif DEVELOPMENT_BUILD
        text.text = $"Development Build v{Application.version}\nSeed {LevelLoader.singleton.seed}";
        #else
        gameObject.SetActive(false);
        #endif
    }
}
