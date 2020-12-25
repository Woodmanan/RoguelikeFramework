#if UNITY_EDITOR
//This whole class only compiles and runs in the editor.
//You definitely shouldn't just leave it in the build, but if you do, it'll be fine.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Testing : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Map.singleton.allocateMap(100, 100);
        Map.singleton.PerformTesting();
    }

    // Update is called once per frame
    void Update()
    {

    }
}



#endif
