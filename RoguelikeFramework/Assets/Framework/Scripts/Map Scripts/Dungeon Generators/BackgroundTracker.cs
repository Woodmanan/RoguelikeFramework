using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundTracker : MonoBehaviour
{
    public int offset;
    public float realtimeSpeed;
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.player)
        {
            UpdatePosition();
        }
    }

    public void UpdatePosition()
    {
        if (Map.current)
        {
            Vector3 pos = new Vector3();
            pos.x = Map.current.width / 2;
            pos.y = Mathf.RoundToInt((Player.player[0].location.y / offset) +.5f) * offset;
            pos.z = 0;
            transform.position = pos;

            anim.speed = realtimeSpeed / offset;
        }
    }
}
