using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

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

    public override IEnumerator DetermineAction()
    {
        if (InputTracking.HasNextAction())
        {
            PlayerAction action = InputTracking.PopNextAction();
            switch (action)
            {
                //Handle Movement code
                case PlayerAction.MOVE_UP:
                    SetAction(new MoveAction(location + Vector2Int.up));
                    break;
                case PlayerAction.MOVE_DOWN:
                    SetAction(new MoveAction(location + Vector2Int.down));
                    break;
                case PlayerAction.MOVE_LEFT:
                    SetAction(new MoveAction(location + Vector2Int.left));
                    break;
                case PlayerAction.MOVE_RIGHT:
                    SetAction(new MoveAction(location + Vector2Int.right));
                    break;
                case PlayerAction.MOVE_UP_LEFT:
                    SetAction(new MoveAction(location + new Vector2Int(-1, 1)));
                    break;
                case PlayerAction.MOVE_UP_RIGHT:
                    SetAction(new MoveAction(location + new Vector2Int(1, 1)));
                    break;
                case PlayerAction.MOVE_DOWN_LEFT:
                    SetAction(new MoveAction(location + new Vector2Int(-1, -1)));
                    break;
                case PlayerAction.MOVE_DOWN_RIGHT:
                    SetAction(new MoveAction(location + new Vector2Int(1, -1)));
                    break;
                case PlayerAction.WAIT:
                    SetAction(new WaitAction());
                    break;
                case PlayerAction.DROP_ITEMS:
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
                case PlayerAction.EQUIP:
                    uiControls.OpenEquipmentInspect();
                    yield return new WaitUntil(() => !UIController.WindowsOpen);
                    break;
                case PlayerAction.APPLY:
                    uiControls.OpenInventoryApply();
                    yield return new WaitUntil(() => !UIController.WindowsOpen);
                    break;
                case PlayerAction.FIRE:
                    EquipmentSlot slot = equipment.equipmentSlots.First(x => x.type.Contains(EquipSlotType.RANGED_WEAPON));
                  
                    if (slot.active)
                    {
                        //Fire!
                        //This is the coolest piece of code I have ever written
                        bool canFire = false;
                        RangedWeapon weapon = slot.equipped.held[0].GetComponent<RangedWeapon>();
                        uiControls.OpenTargetting(weapon.targeting, (b) => canFire = b);
                        yield return new WaitUntil(() => !UIController.WindowsOpen);
                        if (canFire)
                        {
                            weapon.Fire(player);
                        }
                        break;
                    }
                    else
                    {
                        //TODO: Pull up the screen to equip something
                        Debug.Log("Console log: You must have a ranged weapon equipped!");
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

    //Special case, because it affects the world around it through the player's view.
    public override void UpdateLOS()
    {
        view = LOS.GeneratePlayerLOS(location, visionRadius);
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
                //Use the new pickup action system to just grab whatever is there.
                //If this breaks, the problem now lies in that file, instead of cluttering Player.cs
                SetAction(new PickupAction(0));
                break;
            default:
                //Open dialouge box
                uiControls.OpenInventoryPickup();
                break;

        }
    }
    


}
