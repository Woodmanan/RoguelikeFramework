using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : CustomTile
{
    public LevelConnection connection;
    public bool up;
    public bool from;
    [SerializeField] Sprite upSprite;
    [SerializeField] Sprite downSprite;
    [SerializeField] Sprite upSpriteOneWay;
    [SerializeField] Sprite downSpriteOneWay;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetMatchingLevel()
    {
        if (from)
        {
            return connection.toLevel;
        }
        else
        {
            return connection.fromLevel;
        }
    }

    public Vector2Int GetMatchingLocation()
    {
        if (from)
        {
            return connection.toLocation;
        }
        else
        {
            return connection.fromLocation;
        }
    }

    public void SetConnection(LevelConnection connection, bool from)
    {
        this.from = from;
        int fromIndex = LevelLoader.singleton.GetIndexOf(connection.from);
        int toIndex = LevelLoader.singleton.GetIndexOf(connection.to);
        SpriteRenderer render = GetComponent<SpriteRenderer>();

        if (from)
        {
            up = fromIndex > toIndex;
        }
        else
        {
            up = toIndex > fromIndex;
        }

        {//Set sprite based on paramters
            if (connection.oneWay)
            {
                if (up)
                {
                    render.sprite = upSpriteOneWay;
                }
                else
                {
                    render.sprite = downSpriteOneWay;
                }
            }
            else
            {
                if (up)
                {
                    render.sprite = upSprite;
                }
                else
                {
                    render.sprite = downSprite;
                }
            }
        }

        if (connection.fromBranch != connection.toBranch) //Detects a branch change
        {
            if (connection.toBranch) //We get our sprites from our entry point, always
            {
                if (up && connection.toBranch.exitTile)
                {
                    render.sprite = connection.toBranch.exitTile;
                }
                else if (!up && connection.toBranch.entryTile)
                {

                    render.sprite = connection.toBranch.entryTile;
                }
            }
        }

        this.connection = connection;
    }
}
