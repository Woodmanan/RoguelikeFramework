using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Item : MonoBehaviour
{
    int id;
    Vector2Int location;
    public bool held;
    private Monster heldBy;

    private SpriteRenderer Render;
    public SpriteRenderer render
    {
        get 
        {
            if (Render)
            {
                return Render;
            }
            else
            {
                Render = GetComponent<SpriteRenderer>();
                return Render;
            }
        }

        set
        {
            Render = value;
        }
    }

    private static readonly float itemZValue = -7.0f;

    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pickup(Monster m)
    {
        heldBy = m;
        DisableSprite();
    }

    public void Drop()
    {
        heldBy = null;
    }

    public void SetLocation(Vector2Int loc)
    {
        this.location = loc;
    }

    public void EnableSprite()
    {
        render.enabled = true;
        transform.position = new Vector3(location.x, location.y, itemZValue);
    }

    public void DisableSprite()
    {
        render.enabled = false;
    }

    public virtual void Apply()
    {

    }

    public virtual void Equip()
    {

    }

    public virtual void RegenerateStats()
    {

    }


}
