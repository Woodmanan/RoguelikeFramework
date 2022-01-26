using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class Player : Monster
{
    public RexRoom testRoom;

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
}
