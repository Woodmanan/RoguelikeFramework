using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
#if  UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Inventory))]
public class CustomTile : MonoBehaviour
{
    //Stuff that will change a lot, and should be visible
    [Header("Active gameplay elements")]
    public bool isVisible = false;
    public bool isHidden = true;
    public bool dirty = true;
    private bool setup = false;

    public Vector2Int location;
    
    //Stuff that will not change a lot, and should not be (too) visible
    [Header("Static elements")] 
    public new string name;
    public string description;
    public float movementCost;
    public bool blocksVision;
    public Color color = Color.white;
    public Monster currentlyStanding;

    //Floor visualization
    public Inventory inventory;
    private ItemVisiblity itemVis;

    private SpriteRenderer render;

    public event Action<Monster> MonsterEntered;

    //Stuff used for convenience editor hacking, and should never be seen.
    
    /* If you see this and don't know what this is, ask me! It's super useful
     * for hacking up the editor, and making things easy. The #if's in this file
     * are used to make the sprite in the sprite renderer equal the sprite in this file,
     * so you can't forget to not change both. */
    #if UNITY_EDITOR
    private Color currentColor;
    #endif

    Map map;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup()
    {
        if (setup) return;
        inventory = GetComponent<Inventory>(); //May need to be changed to a get/set singleton system
        inventory.Setup();
        //Starts as on, so that Unity 
        render = GetComponent<SpriteRenderer>();
        render.enabled = false;

        //Set up initial visibility
        itemVis = GetComponent<ItemVisiblity>();
        itemVis.Setup();

        RebuildGraphics();
        this.enabled = false;
        setup = true;
    }

    public void Reveal()
    {
        isVisible = true;
        isHidden = false;
        dirty = true;
    }

    public void Clear()
    {
        isVisible = false;
        dirty = true;
    }

    public void ClearMonster()
    {
        currentlyStanding = null;
    }

    public void SetMonster(Monster m)
    {
        if (currentlyStanding != m && currentlyStanding != null)
        {
            Debug.LogError($"Monster set itself as standing on tile ({location}), but {currentlyStanding.displayName} is there.", this);
        }
        currentlyStanding = m;
        MonsterEntered?.Invoke(m);
    }

    public void RebuildMapData()
    {
        map.blocksVision[location.x, location.y] = blocksVision;
        map.moveCosts[location.x, location.y] = movementCost;
    }

    public void SetMap(Map map, Vector2Int location)
    {
        this.map = map;
        this.location = location;
    }

    public bool BlocksMovement()
    {
        return movementCost < 0;
    }

    public void Highlight(Color hcolor)
    {
        render.color = hcolor;
    }
    
    public void StopHighlight()
    {
        RebuildGraphics();
    }

    public void RebuildIfDirty()
    {
        if (dirty)
        {
            RebuildGraphics();
        }
    }

    public void RebuildGraphics()
    {
        if (isVisible)
        {
            render.color = color;
            if (render.enabled == false)
            {
                render.enabled = true;
            }
        }
        else
        {
            //TODO: Item coloring on tiles that are not visible anymore
            if (!isHidden)
            {
                render.enabled = true;
                float gray = color.grayscale / 2;
                render.color = new Color(gray, gray, gray);
            }
            else
            {
                if (render == null)
                {
                    Debug.LogError("Someone didn't do something right!", this);
                }
                render.enabled = false;
                render.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            }

        }

        itemVis.RebuildVisiblity(isVisible, isHidden);

        dirty = false;
    }
    
    //Called at the end of map construction, once this tile is guarunteed to be in the map!
    public virtual void SetInMap(Map m)
    {

    }


    //Editor only functions - For convenience
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (render == null)
        {
            render = GetComponent<SpriteRenderer>();
        }

        if (render.color != currentColor)
        {
            currentColor = render.color;
            color = render.color;
        }
        else if (color != currentColor)
        {
            currentColor = color;
            render.color = color;
        }
    }
    #endif
}
