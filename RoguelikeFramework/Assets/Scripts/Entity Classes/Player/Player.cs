using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : Monster
{

    //UI Stuff!
    [SerializeField] UIController uiControls;

    private static Player _player;
    public static Player player
    {
        get
        {
            if (_player == null)
            {
                _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            }
            return _player;
        }
        set
        {
            _player = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        player = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override IEnumerator LocalTurn()
    {
        if (InputTracking.HasNextAction())
        {
            PlayerAction action = InputTracking.PopNextAction();
            switch (action)
            {
                //Handle Movement code
                case PlayerAction.MOVE_UP:
                    AttemptMovement(Direction.NORTH);
                    break;
                case PlayerAction.MOVE_DOWN:
                    AttemptMovement(Direction.SOUTH);
                    break;
                case PlayerAction.MOVE_LEFT:
                    AttemptMovement(Direction.WEST);
                    break;
                case PlayerAction.MOVE_RIGHT:
                    AttemptMovement(Direction.EAST);
                    break;
                case PlayerAction.MOVE_UP_LEFT:
                    AttemptMovement(Direction.NORTH_WEST);
                    break;
                case PlayerAction.MOVE_UP_RIGHT:
                    AttemptMovement(Direction.NORTH_EAST);
                    break;
                case PlayerAction.MOVE_DOWN_LEFT:
                    AttemptMovement(Direction.SOUTH_WEST);
                    break;
                case PlayerAction.MOVE_DOWN_RIGHT:
                    AttemptMovement(Direction.SOUTH_EAST);
                    break;
                case PlayerAction.DROP_ITEMS:
                    //TODO: Open a dialogue box for dropping
                    Debug.Log("Dropping items!");
                    uiControls.OpenInventoryDrop();
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
                    uiControls.OpenInventoryInspect();
                    yield return new WaitUntil(() => !UIController.WindowsOpen);
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

        EndTurn();

        LOS.GeneratePlayerLOS(location, visionRadius);
    }

    //Item pickup, but with a little logic for determining if a UI needs to get involved.
    private void PickupSmartDetection()
    {
        CustomTile tile = Map.singleton.GetTile(location);
        Inventory onFloor = tile.GetComponent<Inventory>();
        switch (onFloor.count)
        {
            case 0:
                return; //Exit early
            case 1:
                //TODO: Come back and look at this again, may be some weird edge cases
                inventory.PickUpAll(); //Not the smartest code, but should handle any error cases okay
                break;
            default:
                //Open dialouge box
                uiControls.OpenInventoryPickup();
                break;

        }
    }

    /*
     * Attempt to move in a given direction. Does checks for if a wall is passable
     * and if a monster is standing there. If so, performs the necessary actions
     * or logs the messages.
     */ 
    private void AttemptMovement(Direction dir)
    {
        Vector2Int newLocation = GetUnitStep(dir);
        CustomTile tile = Map.singleton.GetTile(newLocation.x, newLocation.y);
        
        //Does tile have a monster? (Allows edge case for monster in wall)
        Monster target = tile.currentlyStanding;
        if (target)
        {
            //TODO: Actually attack monsters
            return;
        }
        
        //Is tile passable?
        if (tile.BlocksMovement())
        {
            Logger.Log($"You bumped into a {tile.name}");
            return;
        }
        
        MoveUnit(dir);
        LOS.GeneratePlayerLOS(location, visionRadius);
    }
    
    


}
