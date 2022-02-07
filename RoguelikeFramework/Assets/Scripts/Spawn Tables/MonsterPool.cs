using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Monster Pool", menuName = "Distribution/Monster Pool", order = 4)]
public class MonsterPool : ScriptableObject
{
    public List<MonsterTable> tables;

    public float chanceForOutOfDepth;
    public int maxDepthIncrease;
    
    public void SetupTables()
    {
        tables = tables.Select(x => Instantiate(x)).ToList();
        foreach (MonsterTable table in tables)
        {
            table.CalculateDepths();
        }
    }

    public Monster SpawnMonster(int depth)
    {
        //Do out-of-depth check
        if (Random.Range(0.0f, 99.99f) < chanceForOutOfDepth)
        {
            depth += Random.Range(0, maxDepthIncrease);
        }

        //Filter to just tables that can support our query
        List<MonsterTable> options = tables.Where(x => x.containedDepths.Contains(depth)).ToList();
        if (options.Count == 0)
        {
            Debug.LogError($"This pool can't support spawning monsters at depth {depth}!");
            return null;
        }

        return options[Random.Range(0, options.Count)].RandomMonsterByDepth(depth);
    }
}
