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
    [SerializeField] GameObject highlightPrefab;
    [SerializeField] HighlightBlock rangeHighlight;
    [SerializeField] RectTransform targetingIcon;
    HighlightBlock[,] highlights;

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
        if (current != t)
        {
            lastTarget = null;
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
        if (lastTarget == null && !t.recommendsPlayerTarget)
        {
            List<Monster> targets = Player.player.view.visibleMonsters;
            targets.Remove(Player.player);

            if ((t.tags & TargetTags.RECOMMNEDS_ALLY_TARGET) > 0)
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
            if (highlights != null)
            {
                if (highlights.GetLength(0) != current.length || highlights.GetLength(1) != current.length)
                {
                    foreach (HighlightBlock h in highlights)
                    {
                        Destroy(h.gameObject);
                    }
                    BuildArea();
                }
            }
            else
            {
                BuildArea();
            }
            for (int i = 0; i < current.length; i++)
            {
                for (int j = 0; j < current.length; j++)
                {
                    //Debug pattern
                    highlights[i, j].Hide();
                }
            }

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
        highlights = new HighlightBlock[current.length, current.length];
        for (int i = 0; i < current.length; i++)
        {
            for (int j = 0; j < current.length; j++)
            {
                GameObject newHighlight = Instantiate(highlightPrefab);
                highlights[i, j] = newHighlight.GetComponent<HighlightBlock>();
                highlights[i, j].Setup();

                RectTransform trans = (RectTransform)newHighlight.transform;
                trans.SetParent(transform);
                trans.anchoredPosition = new Vector2(i - current.length / 2, j - current.length / 2);
            }
        }
        RectTransform rect = (RectTransform)rangeHighlight.transform;
        rect.sizeDelta = new Vector2(current.range * 2 + 1, current.range * 2 + 1);
    }

    public void RedrawHighlights()
    {
        current.GenerateArea();
        for (int i = 0; i < current.length; i++)
        {
            for (int j = 0; j < current.length; j++)
            {
                if (current.area[i, j])
                {
                    highlights[i, j].Show();
                }
                else
                {
                    highlights[i, j].Hide();
                }
            }
        }

        Vector2Int offset = -current.origin + new Vector2Int(current.offset, current.offset);

        foreach (Vector2Int p in current.points)
        {
            Vector2Int spot = p + offset;
            highlights[spot.x, spot.y].Show(Color.green);
        }

        Vector2Int currentSpot = current.target + offset;
        targetingIcon.anchoredPosition = new Vector2(currentSpot.x - current.length / 2, currentSpot.y - current.length / 2);
        //highlights[currentSpot.x, currentSpot.y].Show(Color.blue);
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
                    if (current.affected.Contains(Player.player) && !current.recommendsPlayerTarget)
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