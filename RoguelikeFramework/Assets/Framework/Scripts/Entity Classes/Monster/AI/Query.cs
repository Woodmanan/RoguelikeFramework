using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/* 
 * Welcome to the Query class! This class is an attempt to make
 * an editor-friendly extension to the concept of monsters asking
 * questions about their world. Used in conjunction with the monster
 * action controller, this class should help to give our AI's the 
 * semblance of real intelligence.
 * 
 * Each instance of the Query class represents a single question, of the
 * form:
 * 
 *     Does x entity(s) have a value (that is within a parameter?) (with some weight)
 *     
 * In this way, we can have a fast heuristic that asks a fairly complex question about
 * the world, and weigh those heurstics together to create intelligence. For instance,
 * a healing spell might ask "Is there any with health < 30%", and rate itself highly if
 * there is. If might also as "Is there any ally with health < 60%" but with a much lower
 * weight, so that the option becomes available once any allies are damaged, but is
 * much preferable when an ally actually needs it.
 * 
 * Queries come in the form 
 * 
 * Does SUBJECT (SUBJECT MODIFIER) (SUBJECT VALUE) have PROPERTY EQUALITY than VALUE (VALUE MODIFIER) (WEIGHT)
 * 
 * This form covers all the base cases I could think of, but might need to be re-examined and reworked in the future!
 */

//Enums for the query - only place in the project that gets it's own enums, since these are class-exclusive

public enum QuerySubject
{
    ALWAYS,
    MONSTER,
    ENEMIES,
    ALLIES_EXCLUSIVE,
    ALLIES_INCLUSIVE,
    ABILITY,
    ITEM
}

public enum QuerySubjectModifier
{
    ANY,
    NEAREST,
    FARTHEST,
    ALL,
    MORE_THAN,
    LESS_THAN,
    PERCENT_TOTAL,
    PERCENT_OF
}

public enum QueryProperty
{
    RESOURCE,
    DISTANCE,
    POWER,
    NO_COOLDOWN,
    STACK_VALUE
}

public enum QueryEquality
{
    LESS,
    LESS_EQUAL,
    EQUAL,
    MORE_EQUAL,
    MORE
}

public enum QueryValueModifier
{
    VALUE,
    PERCENT,
    NEAREST_INT
}

[Serializable]
public class Query
{
    public int outOf = 1;
    public List<QueryTerm> terms;
    
    public float Evaluate(Monster owner, List<Monster> inSight, Ability ability, ItemStack item)
    {
        if (outOf == 0) return 0.0f;

        float value = 0;
        foreach (QueryTerm term in terms)
        {
            value += term.Evaluate(owner, inSight, ability, item);
        }
        value = value / outOf;

        return Mathf.Clamp(value, 0.0f, 1.0f);
    }
}

[Serializable]
public class QueryTerm
{
    public QuerySubject subject;
    public QuerySubjectModifier subjectMod;
    public int subjectCount;

    public QueryProperty property;
    public Resources resource;
    public QueryEquality equality;

    public float value;
    public QueryValueModifier valueMod;

    public int weight = 1;

    public float Evaluate(Monster owner, List<Monster> inSight, Ability ability, ItemStack item)
    {
        return (weight * PerformEvaluation(owner, inSight, ability, item));
    }

    //Actual Evaluation funciton! Returns a number between 1 and 0.
    private float PerformEvaluation(Monster owner, List<Monster> inSight, Ability ability, ItemStack item)
    {
        switch (subject)
        {
            case QuerySubject.ALWAYS:
                if (valueMod == QueryValueModifier.PERCENT) return value / 100;
                return Mathf.Clamp(value, 0.0f, 1.0f);
            case QuerySubject.MONSTER:
                return EvaluateSingleMonster(owner, owner);
            case QuerySubject.ENEMIES:
                return EvaluateManyMonsters(owner, inSight.Where(x => (owner.faction & x.faction) == 0).ToList());
            case QuerySubject.ALLIES_INCLUSIVE:
                return EvaluateManyMonsters(owner, inSight.Where(x => (owner.faction & x.faction) != 0).ToList());
            case QuerySubject.ALLIES_EXCLUSIVE:
                return EvaluateManyMonsters(owner, inSight.Where(x => (owner.faction & x.faction) != 0 && owner != x).ToList()); //Should probably just remove the monster? This is cleaner, though. Hm.
            case QuerySubject.ABILITY:
                if (ability == null)
                {
                    Debug.LogError("Can't use ability check on something that is not an abliity!");
                    return 0.0f;
                }
                return EvaluateAbility(owner, ability);
            case QuerySubject.ITEM:
                if (item == null)
                {
                    Debug.LogError("Can't use item check on something that is not an item!");
                    return 0.0f;
                }
                return EvaluateItem(owner, item);
            default:
                UnityEngine.Debug.LogError($"Subject {subject} was invalid?? You definitely changed something and broke queries. Go fix it!!!");
                UnityEngine.Debug.LogError("Hint: This is called because you didn't case for it. Click here and add a case!");
                return 0.0f;
        }
    }

    public float EvaluateAbility(Monster owner, Ability ability)
    {
        switch (property)
        {
            case QueryProperty.NO_COOLDOWN:
                return ability.currentCooldown == 0 ? 1.0f : 0.0f;
            case QueryProperty.POWER:
                return EvaluatePower(ability);
            default:
                UnityEngine.Debug.LogError($"Abilities cannot be compared using {property}!");
                return 0.0f;
        }
    }

    public float EvaluateItem(Monster owner, ItemStack stack)
    {
        switch (property)
        {
            case QueryProperty.STACK_VALUE:
                return EvaluateStackSize(stack);
            default:
                UnityEngine.Debug.LogError($"Items cannot be compared using {property}!");
                return 0.0f;
        }
    }

    public float EvaluateManyMonsters(Monster owner, List<Monster> monsters)
    {
        if (monsters.Count == 0) { return 0.0f; }
        float val;

        switch (subjectMod)
        {
            case QuerySubjectModifier.ANY:
                foreach (Monster m in monsters)
                {
                    if (EvaluateSingleMonster(owner, m) > .99f)
                    {
                        return 1.0f;
                    }
                }
                return 0.0f;
            case QuerySubjectModifier.NEAREST:
                monsters = monsters.OrderBy(x => Vector2Int.Distance(owner.location, x.location)).ToList();
                return EvaluateSingleMonster(owner, monsters[0]);
            case QuerySubjectModifier.FARTHEST:
                monsters = monsters.OrderBy(x => Vector2Int.Distance(owner.location, x.location)).ToList();
                return EvaluateSingleMonster(owner, monsters[monsters.Count - 1]);
            case QuerySubjectModifier.ALL:
                foreach (Monster m in monsters)
                {
                    if (EvaluateSingleMonster(owner, m) < .99f)
                    {
                        return 0.0f;
                    }
                }
                return 1.0f;
            case QuerySubjectModifier.MORE_THAN:
                val = 0.0f;
                foreach (Monster m in monsters)
                {
                    val += EvaluateSingleMonster(owner, m);
                }
                return (val > subjectCount) ? 1.0f : 0.0f;
            case QuerySubjectModifier.LESS_THAN:
                val = 0.0f;
                foreach (Monster m in monsters)
                {
                    val += EvaluateSingleMonster(owner, m);
                }
                return (val < subjectCount) ? 1.0f : 0.0f;
            case QuerySubjectModifier.PERCENT_TOTAL:
                val = 0.0f;
                foreach (Monster m in monsters)
                {
                    val += EvaluateSingleMonster(owner, m);
                }
                return val / monsters.Count;
            case QuerySubjectModifier.PERCENT_OF:
                val = 0.0f;
                foreach (Monster m in monsters)
                {
                    val += EvaluateSingleMonster(owner, m);
                }
                if (val >= subjectCount) { return 1.0f; }
                return val / subjectCount;
            default:
                UnityEngine.Debug.Log($"Cannot evaluate Subject modifier {subjectMod} for many monsters!");
                return 0.0f;
        }
    }

    public float EvaluateSingleMonster(Monster owner, Monster other)
    {
        switch (property)
        {
            case QueryProperty.RESOURCE:
                return EvaluateMonsterResource(other);
            case QueryProperty.DISTANCE:
                //TODO: Use the map distance type to filter this better.
                return EvaluateDistance(Vector2Int.Distance(owner.location, other.location));
            default:
                UnityEngine.Debug.LogError($"You cannot use comparison type {property} on a monster!");
                return 0.0f;
        }
    }

    public float EvaluateMonsterResource(Monster m)
    {
        float comp = m.currentStats[resource];
        float valueCopy = value;
        switch (valueMod)
        {
            case QueryValueModifier.PERCENT:
                comp = comp / m.currentStats[resource];
                valueCopy = value / 100;
                break;
            case QueryValueModifier.NEAREST_INT:
                comp = Mathf.Round(comp);
                valueCopy = Mathf.Round(value);
                break;
            case QueryValueModifier.VALUE:
                break;
            default:
                UnityEngine.Debug.LogError($"Cannot use value modifier {valueMod} in resource checks!");
                break;
        }

        switch (equality)
        {
            case QueryEquality.LESS:
                return (comp < valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.LESS_EQUAL:
                return (comp <= valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.EQUAL:
                return (comp == valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.MORE_EQUAL:
                return (comp >= valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.MORE:
                return (comp > valueCopy) ? 1.0f : 0.0f;
            default:
                UnityEngine.Debug.LogError($"No resource equality method for {equality} found!");
                return 0.0f;
        }
    }

    public float EvaluateDistance(float distance)
    {
        float valueCopy = value;
        if (valueMod == QueryValueModifier.NEAREST_INT)
        {
            distance = Mathf.Round(distance);
            valueCopy = Mathf.Round(distance);
        }
        switch (equality)
        {
            case QueryEquality.LESS:
                return (distance < valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.LESS_EQUAL:
                return (distance <= valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.EQUAL:
                return (distance == valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.MORE_EQUAL:
                return (distance >= valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.MORE:
                return (distance > valueCopy) ? 1.0f : 0.0f;
            default:
                UnityEngine.Debug.LogError($"No distance equality method for {equality} found!");
                return 0.0f;
        }
    }

    public float EvaluatePower(Ability ability)
    {
        float power = ability.currentStats[AbilityResources.POWER];
        float valueCopy = value;
        if (valueMod == QueryValueModifier.NEAREST_INT)
        {
            power = Mathf.Round(power);
            valueCopy = Mathf.Round(value);
        }

        switch (equality)
        {
            case QueryEquality.LESS:
                return (power < valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.LESS_EQUAL:
                return (power <= valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.EQUAL:
                return (power == valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.MORE_EQUAL:
                return (power >= valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.MORE:
                return (power > valueCopy) ? 1.0f : 0.0f;
            default:
                UnityEngine.Debug.LogError($"No power equality method for {equality} found!");
                return 0.0f;
        }
    }

    public float EvaluateStackSize(ItemStack stack)
    {
        float count = stack.count;
        float valueCopy = value;

        if (valueMod == QueryValueModifier.NEAREST_INT)
        {
            count = Mathf.Round(count);
            valueCopy = Mathf.Round(value);
        }

        switch (equality)
        {
            case QueryEquality.LESS:
                return (count < valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.LESS_EQUAL:
                return (count <= valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.EQUAL:
                return (count == valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.MORE_EQUAL:
                return (count >= valueCopy) ? 1.0f : 0.0f;
            case QueryEquality.MORE:
                return (count > valueCopy) ? 1.0f : 0.0f;
            default:
                UnityEngine.Debug.LogError($"No stack size equality method for {equality} found!");
                return 0.0f;
        }
    }
}
