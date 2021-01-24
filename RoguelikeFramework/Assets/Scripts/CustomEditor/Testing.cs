#if UNITY_EDITOR
//This whole class only compiles and runs in the editor.
//You definitely shouldn't just leave it in the build, but if you do, it'll be fine.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Testing : MonoBehaviour
{

    Player p;
    // Start is called before the first frame update
    void Start()
    {
        Map.singleton.allocateMap(100, 100);
        Map.singleton.PerformTesting();
        p = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    Path oldpath;

    // Update is called once per frame
    void Update()
    {

        if (oldpath != null)
        {
            foreach (Vector2Int t in oldpath)
            {
                Map.singleton.GetTile(t).StopHighlight();
            }
        }

        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int intPos = new Vector2Int((int) pos.x, (int) pos.y);
        Path path = Pathfinding.FindPath(p.location, intPos);
        Debug.Log($"Takes {path.Count()} steps to get there, cost is {path.Cost()}");

        foreach (Vector2Int t in path)
        {
            Map.singleton.GetTile(t).Highlight(Color.yellow);
        }

        Map.singleton.GetTile(path.destination).Highlight(Color.green);
        oldpath = path;
    }
}



#endif
