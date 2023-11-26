using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideInRelease : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        #if (!UNITY_EDITOR && !DEVELOPMENT_BUILD)
        gameObject.SetActive(false);
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
