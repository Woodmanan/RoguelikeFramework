using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionController : ActionController
{
    public override IEnumerator DetermineAction()
    {
        if (InputTracking.HasNextAction())
        {
            PlayerAction action = InputTracking.PopNextAction();
            switch (action)
            {
                //Handle Movement code
                case PlayerAction.MOVE_UP:
                    nextAction = new MoveAction(monster.location + Vector2Int.up);
                    break;
                case PlayerAction.MOVE_DOWN:
                    nextAction = new MoveAction(monster.location + Vector2Int.down);
                    break;
                case PlayerAction.MOVE_LEFT:
                    nextAction = new MoveAction(monster.location + Vector2Int.left);
                    break;
                case PlayerAction.MOVE_RIGHT:
                    nextAction = new MoveAction(monster.location + Vector2Int.right);
                    break;
                case PlayerAction.MOVE_UP_LEFT:
                    nextAction = new MoveAction(monster.location + new Vector2Int(-1, 1));
                    break;
                case PlayerAction.MOVE_UP_RIGHT:
                    nextAction = new MoveAction(monster.location + new Vector2Int(1, 1));
                    break;
                case PlayerAction.MOVE_DOWN_LEFT:
                    nextAction = new MoveAction(monster.location + new Vector2Int(-1, -1));
                    break;
                case PlayerAction.MOVE_DOWN_RIGHT:
                    nextAction = new MoveAction(monster.location + new Vector2Int(1, -1));
                    break;
                case PlayerAction.WAIT:
                    nextAction = new WaitAction();
                    break;
                case PlayerAction.DROP_ITEMS:
                    Debug.Log("Dropping items!");
                    UIController.singleton.OpenInventoryDrop();
                    yield return new WaitUntil(() => !UIController.WindowsOpen);
                    break;
                case PlayerAction.PICK_UP_ITEMS:
                    //Intelligently pick up items, opening dialouge box if needed.
                    PickupSmartDetection();

                    //Check if dialouge box is opened; if so, freeze until transaction is done
                    if (UIController.WindowsOpen)
                    {
                        yield return new WaitUntil(() => !UIController.WindowsOpen);
                    }
                    break;
                case PlayerAction.OPEN_INVENTORY:
                    UIController.singleton.OpenInventoryInspect();
                    yield return new WaitUntil(() => !UIController.WindowsOpen);
                    break;
                case PlayerAction.EQUIP:
                    UIController.singleton.OpenEquipmentInspect();
                    yield return new WaitUntil(() => !UIController.WindowsOpen);
                    break;
                case PlayerAction.UNEQUIP:
                    UIController.singleton.OpenEquipmentUnequip();
                    yield return new WaitUntil(() => !UIController.WindowsOpen);
                    break;
                case PlayerAction.APPLY:
                    UIController.singleton.OpenInventoryApply();
                    yield return new WaitUntil(() => !UIController.WindowsOpen);
                    break;
                case PlayerAction.CAST_SPELL:
                    UIController.singleton.OpenAbilities();
                    yield return new WaitUntil(() => !UIController.WindowsOpen);
                    break;
                case PlayerAction.FIRE:
                    nextAction = new RangedAttackAction();
                    break;
                case PlayerAction.ASCEND:
                    nextAction = new ChangeLevelAction(true);
                    break;
                case PlayerAction.DESCEND:
                    Stair stair = Map.current.GetTile(monster.location) as Stair;
                    if (stair && !stair.upStair)
                    {
                        nextAction = new ChangeLevelAction(false);
                    }
                    else
                    {
                        nextAction = new PathfindAction(Map.current.exits[0]);
                    }
                    break;

                //Handle potentially weird cases (Thanks, Nethack design philosophy!)
                case PlayerAction.ESCAPE_SCREEN:
                    //TODO: Open up the menu screen here
                    RogueUIPanel.ExitTopLevel(); //This really, really shouldn't do anything. Let it happen, though!
                    break;
                case PlayerAction.ACCEPT:
                case PlayerAction.NONE:
                    Debug.Log("Player read an input set to do nothing", this);
                    break;
                default:
                    Debug.LogError($"Player read an input that has no switch case: {action}");
                    break;
            }
        }
    }

    //Item pickup, but with a little logic for determining if a UI needs to get involved.
    private void PickupSmartDetection()
    {
        CustomTile tile = Map.current.GetTile(monster.location);
        Inventory onFloor = tile.GetComponent<Inventory>();
        switch (onFloor.Count)
        {
            case 0:
                return; //Exit early
            case 1:
                //Use the new pickup action system to just grab whatever is there.
                //If this breaks, the problem now lies in that file, instead of cluttering Player.cs
                nextAction = new PickupAction(0);
                break;
            default:
                //Open dialouge box
                UIController.singleton.OpenInventoryPickup();
                break;

        }
    }
}
