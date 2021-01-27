using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conversions
{
    public static int NumberingToInt(char c)
    {
        int num = (int)c;
        if (num >= 97)
        {
            return (num - 97);
        }
        else
        {
            return (num - 39);
        }
    }

    public static char IntToNumbering(int num)
    {
        if (num < 26)
        {
            return (char)(num + 97);
        }
        else
        {
            return (char)(num + 39);
        }
    }
}
