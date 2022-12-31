using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Room Placers")]
public class ForestMachine : Machine
{
    public float percentStart;
    public float numRounds;

    public override IEnumerator Activate()
    {
        bool[,] map = new bool[size.x, size.y];
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                map[i, j] = (Random.Range(0, 100) < percentStart);
            }
        }

        for (int r = 0; r < numRounds; r++)
        {
            yield return null;
            bool[,] newMap = new bool[size.x, size.y];
            for (int i = 1; i < map.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < map.GetLength(1) - 1; j++)
                {
                    int sum = 0;
                    for (int x = i - 1; x <= i + 1; x++)
                    {
                        for (int y = j - 1; y <= j + 1; y++)
                        {
                            if (map[x, y]) sum++;
                        }
                    }
                    if (map[i, j])
                    {
                        newMap[i, j] = (sum >= 3);
                    }
                    else
                    {
                        newMap[i, j] = (sum > 4);
                    }
                }
            }

            map = newMap;
        }

        //Map
        int[,] flood = new int[size.x, size.y];
        int c = 0;
        int max = -1;
        int max_ind = -1;
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] && flood[i, j] == 0)
                {
                    c++;
                    int count = Floodfill(ref map, ref flood, new Vector2Int(i, j), c);
                    if (count > max)
                    {
                        max_ind = c;
                        max = count;
                    }
                }
            }
        }

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (flood[i, j] == max_ind)
                {
                    generator.map[i, j] = 1;
                }
            }
        }
        
        Room room = UnityEngine.ScriptableObject.CreateInstance<Room>();
        room.size = size;
        room.SetPosition(Vector2Int.zero);

        generator.rooms.Add(room);
    }

    void PrintMap(bool[,] map)
    {
        string str = "";
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                str += map[i, j] ? "O" : "H";
            }
            str += "\n";
        }
        Debug.Log(str);
    }

    int Floodfill(ref bool[,] map, ref int[,] flood, Vector2Int start, int c)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        int count = 0;

        while (queue.Count > 0)
        {
            Vector2Int spot = queue.Dequeue();
            if (map[spot.x, spot.y] && flood[spot.x, spot.y] == 0)
            {
                flood[spot.x, spot.y] = c;
                count++;

                for (int x = spot.x - 1; x <= spot.x + 1; x++)
                {
                    for (int y = spot.y - 1; y <= spot.y + 1; y++)
                    {
                        if (x >= 0 && x < flood.GetLength(0) && y >= 0 && y < flood.GetLength(1))
                        {
                            queue.Enqueue(new Vector2Int(x, y));
                        }
                    }
                }
            }
        }

        return count;
    }
}