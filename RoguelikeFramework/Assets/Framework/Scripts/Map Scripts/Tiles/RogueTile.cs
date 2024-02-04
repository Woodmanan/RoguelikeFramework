using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Localization;
#if  UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Inventory))]
public class RogueTile : MonoBehaviour, IDescribable
{
    //Stuff that will change a lot, and should be visible
    [Header("Active gameplay elements")]
    public Visibility playerVisibility = Visibility.HIDDEN;
    public Visibility graphicsVisibility = Visibility.HIDDEN;
    private Visibility oldGraphicsVisibility = Visibility.VISIBLE;
    public bool graphicsDirty = true;
    private bool setup = false; 

    public Vector2Int location;

    //Stuff that will not change a lot, and should not be (too) visible
    [Header("Static elements")]
    public LocalizedString localName;
    public LocalizedString localDescription;
    public float movementCost;
    public bool blocksVision;
    public bool blocksProjectiles;
    public Color color = Color.white;
    public float minGreyAlpha;
    [NonSerialized]
    public RogueHandle<Monster> currentlyStanding = RogueHandle<Monster>.Default;
    [NonSerialized]
    public RogueHandle<Monster> ghostStanding = RogueHandle<Monster>.Default;

    //Floor visualization
    public Inventory inventory;
    private ItemVisiblity itemVis;

    private SpriteRenderer render;

    public event Action<RogueHandle<Monster>> MonsterEntered;

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
        PreSetup();
        inventory = GetComponent<Inventory>(); //May need to be changed to a get/set singleton system
        inventory.Setup();
        //Starts as on, so that Unity 
        render = GetComponent<SpriteRenderer>();
        render.enabled = true;

        //Set up initial visibility
        itemVis = GetComponent<ItemVisiblity>();
        itemVis.Setup();

        if (blocksVision)
        {
            render.sortingOrder = -location.y;
        }
        else
        {
            render.sortingOrder = -1000;
        }

        RebuildGraphics(RogueHandle<Monster>.Default);
        this.enabled = false;
        setup = true;

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (movementCost == 0)
        {
            Debug.LogError($"{this.name} cannot have cost of 0! This breaks an important precondition of pathfinding.");
            Debug.LogError("This has temporarily been fixed to prevent a freeze, but will NOT be corrected in a build. FIX NOW.");
            movementCost = 1;
        }
        #endif
    }

    public string GetName(bool shorten = false)
    {
        return localName.GetLocalizedString(this);
    }

    public string GetDescription()
    {
        return localDescription.GetLocalizedString(this);
    }

    public Sprite GetImage()
    {
        return render.sprite;
    }

    public virtual void PreSetup()
    {

    }

    public void SetPlayerVisibility(Visibility newVisibility)
    {
        playerVisibility = newVisibility;
    }

    public void SetPlayerVisible()
    {
        playerVisibility |= Visibility.REVEALED | Visibility.VISIBLE;
    }

    public void ClearPlayerVisible()
    {
        playerVisibility &= ~Visibility.VISIBLE;
    }

    public void SetGraphicsVisibility(Visibility newVisibility)
    {
        graphicsVisibility = newVisibility;
        graphicsDirty = true;
    }

    public void SetGraphicsVisible()
    {
        graphicsVisibility |= Visibility.REVEALED | Visibility.VISIBLE;
        graphicsDirty = true;
    }

    public void ClearGraphicsVisible()
    {
        graphicsVisibility &= ~Visibility.VISIBLE;
        graphicsDirty = true;
    }    

    public bool isHidden { get { return graphicsVisibility == Visibility.HIDDEN; } }
    public bool isRevealed { get { return (graphicsVisibility & Visibility.REVEALED) > 0; } }
    public bool isVisible { get { return (graphicsVisibility & Visibility.VISIBLE) > 0; } }

    public bool isPlayerHidden { get { return playerVisibility == Visibility.HIDDEN; } }
    public bool isPlayerRevealed { get { return (playerVisibility & Visibility.REVEALED) > 0; } }
    public bool isPlayerVisible { get { return (playerVisibility & Visibility.VISIBLE) > 0; } }

    public void ClearMonster()
    {
        currentlyStanding = RogueHandle<Monster>.Default;
    }

    public bool IsOpen()
    {
        return !BlocksMovement() && !currentlyStanding.IsValid();
    }

    public void SetMonster(RogueHandle<Monster> m)
    {
        if (currentlyStanding != m && currentlyStanding.IsValid())
        {
            Debug.LogError($"Monster set itself as standing on tile ({location}), but {currentlyStanding.value.friendlyName} is there.", this);
        }
        currentlyStanding = m;
        MonsterEntered?.Invoke(m);
    }

    public void RebuildMapData()
    {
        map.blocksVision[location.x, location.y] = blocksVision;
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
        RebuildGraphics(RogueHandle<Monster>.Default);
    }

    public void RebuildIfDirty()
    {
        if (graphicsDirty)
        {
            RebuildGraphics(RogueHandle<Monster>.Default);
        }
    }

    public void RebuildGraphics(RogueHandle<Monster> graphicStanding)
    {
        graphicsDirty = false;
        if (graphicsVisibility == oldGraphicsVisibility) return;

        switch (graphicsVisibility)
        {
            case Visibility.HIDDEN:
                render.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
                break;
            case Visibility.REVEALED:
                float gray = color.grayscale / 2;
                render.color = new Color(gray, gray, gray, Mathf.Max(minGreyAlpha, color.a));
                ghostStanding = graphicStanding;
                break;
            case Visibility.VISIBLE:
            case (Visibility.VISIBLE | Visibility.REVEALED):
                render.color = color;
                if (ghostStanding.IsValid())
                {
                    //If a ghost is seen here, and it's the last time we saw it, make it disappear.
                    if (Mathf.Approximately(Vector2.Distance(transform.position, ghostStanding.value.unity.transform.position), 0))
                    {
                        ghostStanding.value.SetGraphicsVisibility(Visibility.HIDDEN);
                    }
                    ghostStanding = RogueHandle<Monster>.Default;
                }
                break;
            default:
                Debug.LogError("Visibility state was not handled.");
                break;
        }

        oldGraphicsVisibility = graphicsVisibility;

        //Let monsters know that this tile has switched
        //currentlyStanding?.SetGraphics((graphicsVisibility & Visibility.VISIBLE) > 0);
        if (graphicStanding.IsValid())
        {
            graphicStanding.value.SetGraphicsVisibilityOnTile(this);
        }
        

        itemVis.RebuildVisiblity(graphicsVisibility);
    }

    public virtual bool IsInteractable()
    {
        return false;
    }
    
    //Called at the end of map construction, once this tile is guarunteed to be in the map!
    public virtual void SetInMap(Map m)
    {

    }

    public virtual float GetMovementCost()
    {
        if (currentlyStanding.IsValid())
        {
            return movementCost * 5;
        }
        return movementCost;
    }

    //The expected EXTRA cost to move from this tile to a tile in direction
    //For most tiles, this is 0. If you're extra special fast, it can be negative. If it's a bad move, make it crazy high
    public virtual float CostToMoveIn(Vector2Int direction)
    {
        return 0f;
    }

    //Moves this tile, it's items, and it's monster to the new position. Convenience function for animating moving tiles
    public void AnimUpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
        AnimUpdateItemPosition(newPosition);
        AnimUpdateMonsterPosition(newPosition);
    }

    //Moves just the contents of this cell to the shown location
    public void AnimUpdateItemPosition(Vector3 newPosition)
    {
        if (inventory)
        {
            Vector3 inventoryPosition = newPosition;
            inventoryPosition.z = Item.itemZValue;
            foreach (Item i in inventory.AllHeld())
            {
                i.transform.position = inventoryPosition;
            }
        }
    }

    public void AnimUpdateMonsterPosition(Vector3 newPosition)
    {
        if (currentlyStanding.IsValid())
        {
            Vector3 monsterPosition = newPosition;
            monsterPosition.z = Monster.monsterZPosition;
            currentlyStanding.value.unity.transform.position = monsterPosition;
        }
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
