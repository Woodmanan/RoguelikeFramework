using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class Player : Monster
{
    //UI Stuff!
    [SerializeField] UIController uiControls;

    private static Monster _player;
    public static Monster player
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
    public override void Start()
    {
        base.Start();
        player = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Special case, because it affects the world around it through the player's view.
    public override void UpdateLOS()
    {
        view = LOS.GeneratePlayerLOS(Map.current, location, visionRadius);
    }

    public override void Die()
    {
        base.Die();
        if (resources.health == 0)
        {
            Debug.Log("Game should be over! This message should be replaced by loading an exit level instead.");
        }
    }

    //Item pickup, but with a little logic for determining if a UI needs to get involved.
    private void PickupSmartDetection()
    {
        CustomTile tile = Map.current.GetTile(location);
        Inventory onFloor = tile.GetComponent<Inventory>();
        switch (onFloor.Count)
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
