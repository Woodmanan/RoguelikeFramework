using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Roll
{
    public int rolls;
    public int dice;
    

    //public bool evaluateEveryRoll;

    //private bool evaluated = false;
    //private int value;

    //Constructors!
    public Roll(int rolls, int dice)
    {
        this.dice = dice;
        this.rolls = rolls;
    }

    public Roll(string value)
    {
        value = value.ToLower();
        string[] splits = value.Split('d');
        if (splits.Length != 2)
        {
            Debug.LogError("Incorrect number of arguments for roll. Should be of from 'XdY");
        }
        if (!int.TryParse(splits[0].Trim(), out rolls))
        {
            Debug.LogError("Roll count (first paramter) was incorrectly formatted");
        }
        if (!int.TryParse(splits[1].Trim(), out dice))
        {
            Debug.LogError("Dice count (second parameter) was incorrectly formatted");
        }
    }

    public int evaluate()
    {
        if (dice == 1)
        {
            return rolls;
        }

        int value = 0;
        for (int i = 0; i < rolls; i++)
        {
            value += UnityEngine.Random.Range(0, dice) + 1;
        }

        return value;
    }
}
