using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Item : MonoBehaviour
{
    Vector2Int location;
    public bool held;
    private Monster heldBy;

    SpriteRenderer render;

    private static readonly float itemZValue = -7.0f;

    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<SpriteRenderer>();
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
        AddToFloor(heldBy.location);
    }

    private void AddToFloor(Vector2Int loc)
    {
        CustomTile tile = Map.singleton.GetTile(loc);
        this.location = loc;
        tile.itemsOnFloor.Add(this);
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
