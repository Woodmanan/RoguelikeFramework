using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TargetingPanel : RogueUIPanel
{
    //Don't uncomment these! These are already declared in the base class,
    //and are listed here so you know they exist.

    //bool inFocus; - Tells you if this is the window that is currently focused. Not too much otherwise.

    public Targeting current;

    [SerializeField] RectTransform targetingIcon;
    [SerializeField] Color highlight;
    [SerializeField] Color blockedHighlight;
    [SerializeField] Sprite pointLocked;
    SpriteGrid grid;

    public Monster lastTarget;

    BoolDelegate returnCall;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool Setup(Targeting t, BoolDelegate endResult)
    {
        //Establish grid if it doesn't exist
        if (!grid)
        {
            grid = (new GameObject("Targeting Grid")).AddComponent<SpriteGrid>();
            grid.transform.parent = transform;
        }

        current = t;
        returnCall = endResult;
        Vector2Int startLocation = Player.player.location;

        //Perform setup and correctness check for last time
        if (lastTarget != null)
        {
            int dist = Mathf.Max(Mathf.Abs(lastTarget.location.x - startLocation.x), Mathf.Abs(lastTarget.location.y - startLocation.y));
            if (dist > t.range || !Player.player.view.visibleMonsters.Contains(lastTarget))
            {
                lastTarget = null;
            }
            else
            {
                startLocation = lastTarget.location;
            }
        }

        //If this is now true, attempt to determine the best spot
        if (lastTarget == null && !t.options.HasFlag(TargetTags.RECOMMENDS_SELF_TARGET))
        {
            List<Monster> targets = Player.player.view.visibleMonsters;
            targets.Remove(Player.player);

            if ((t.options & TargetTags.RECOMMNEDS_ALLY_TARGET) > 0)
            {
                Monster target = targets.Where(x => !x.IsEnemy(Player.player))
                                 .OrderBy(x => Mathf.Max(Mathf.Abs(x.location.x - startLocation.x), Mathf.Abs(x.location.y - startLocation.y)))
                                 .FirstOrDefault();

                if (target != null && Mathf.Max(Mathf.Abs(target.location.x - startLocation.x), Mathf.Abs(target.location.y - startLocation.y)) <= t.range)
                {
                    startLocation = target.location;
                }
            }
            else
            {
                Monster target = targets.Where(x => x.IsEnemy(Player.player))
                                 .OrderBy(x => Mathf.Max(Mathf.Abs(x.location.x - startLocation.x), Mathf.Abs(x.location.y - startLocation.y)))
                                 .FirstOrDefault();

                if (target != null && Mathf.Max(Mathf.Abs(target.location.x - startLocation.x), Mathf.Abs(target.location.y - startLocation.y)) <= t.range)
                {
                    startLocation = target.location;
                }
            }
        }


        //current = t.Initialize();
        if (current.BeginTargetting(Player.player.location, LOS.lastCall))
        {
            if (grid != null)
            {
                if (grid.width != current.length || grid.height != current.length)
                {
                    BuildArea();
                }
            }
            else
            {
                BuildArea();
            }

            grid.SetCenter(Player.player.location);

            current.MoveTarget(startLocation);
            return true;
        }
        else
        {
            if (!current.LockPoint()) //UI thinks we don't even need it, so just skip this whole thing
            {
                Debug.LogError("Targeting item that skips can NOT have more than one point! This is unecessary behaviour, and must be fixed immediately to maintain invariants.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            return false;
        }
    }

    public void BuildArea()
    {
        grid.Build(current.length, current.length, 3, 64);
        grid.AddColor(0, highlight);
        grid.AddColor(1, blockedHighlight);
        grid.AddSprite(2, pointLocked);
    }

    public void RedrawHighlights()
    {
        grid.ClearAll();
        current.GenerateArea();
        for (int i = 0; i < current.length; i++)
        {
            for (int j = 0; j < current.length; j++)
            {
                if (current.area[i, j])
                {
                    grid.SetSprite(i, j, 0);
                }
                else
                {
                    grid.ClearSprite(i, j);
                }
            }
        }

        Vector2Int offset = -current.origin + new Vector2Int(current.offset, current.offset);

        if (!current.isValid)
        {
            grid.SetSprite(current.target.x + offset.x, current.target.y + offset.y, 1);
        }

        foreach (Vector2Int p in current.points)
        {
            Vector2Int spot = p + offset;
            grid.SetSprite(spot.x, spot.y, 2);
        }

        Vector2Int currentSpot = current.target + offset;
        targetingIcon.anchoredPosition = new Vector2(currentSpot.x - current.length / 2, currentSpot.y - current.length / 2);
        //highlights[currentSpot.x, currentSpot.y].Show(Color.blue);

        grid.Apply();
    }

    /*
     * One of the more important functions here. When in focus, this will be called
     * every frame with the stored input from InputTracking
     */
    public override void HandleInput(PlayerAction action, string inputString)
    {
        foreach (char c in inputString)
        {
            switch (c)
            {
                case 'y':
                case 'Y':
                    current.MoveTargetOffset(Vector2Int.up + Vector2Int.left);
                    break;
                case 'u':
                case 'U':
                    current.MoveTargetOffset(Vector2Int.up + Vector2Int.right);
                    break;
                case 'b':
                case 'B':
                    current.MoveTargetOffset(Vector2Int.down + Vector2Int.left);
                    break;
                case 'n':
                case 'N':
                    current.MoveTargetOffset(Vector2Int.down + Vector2Int.right);
                    break;
            }
        }
        switch (action)
        {
            case PlayerAction.MOVE_LEFT:
                current.MoveTargetOffset(Vector2Int.left);
                break;
            case PlayerAction.MOVE_RIGHT:
                current.MoveTargetOffset(Vector2Int.right);
                break;
            case PlayerAction.MOVE_UP:
                current.MoveTargetOffset(Vector2Int.up);
                break;
            case PlayerAction.MOVE_DOWN:
                current.MoveTargetOffset(Vector2Int.down);
                break;
            /* CURRENT DON'T WORK
            case PlayerAction.MOVE_UP_LEFT:
                current.MoveTarget(Vector2Int.up + Vector2Int.left);
                break;
            case PlayerAction.MOVE_UP_RIGHT:
                current.MoveTarget(Vector2Int.up + Vector2Int.right);
                break;
            case PlayerAction.MOVE_DOWN_LEFT:
                current.MoveTarget(Vector2Int.down + Vector2Int.left);
                break;
            case PlayerAction.MOVE_DOWN_RIGHT:
                current.MoveTarget(Vector2Int.down + Vector2Int.right);
                break;
            */
            case PlayerAction.FIRE:
            case PlayerAction.ACCEPT:
                if (current.LockPoint())
                {
                    //We're done!
                    current.GenerateArea();
                    if (current.affected.Contains(Player.player) && !current.options.HasFlag(TargetTags.RECOMMENDS_SELF_TARGET))
                    {
                        UIController.singleton.OpenConfirmation("<color=\"black\">Are you sure you want to target yourself?", (b) => ReturnConfirmed(b)); //DELEGATE MAGICCCC
                        break;
                    }

                    CustomTile tile = Map.current.GetTile(current.points[0]);
                    lastTarget = tile.currentlyStanding;

                    ReturnConfirmed(true);
                }
                else
                {
                    //Do something else?
                }
                break;


        }
        RedrawHighlights();
    }

    void ReturnConfirmed(bool value)
    {
        returnCall(value);
        CustomTile tile = Map.current.GetTile(current.points[0]);
        lastTarget = tile.currentlyStanding;
        ExitAllWindows();
    }

    /* Called every time this panel is activated by the controller */
    public override void OnActivation()
    {

    }

    /* Called every time this panel is deactived by the controller */
    public override void OnDeactivation()
    {

    }

    /* Called every time this panel is focused on. Use this to refresh values that might have changed */
    public override void OnFocus()
    {

    }

    /*
     * Called when this panel is no longer focused on (added something to the UI stack). I don't know 
     * what on earth this would ever get used for, but I'm leaving it just in case (Nethack design!)
     */
    public override void OnDefocus()
    {

    }
}