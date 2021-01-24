using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityEvent
{
    public EventType type;
    public Monster createdBy;
    public Monster target;

    public int[] intValues;
    public float[] floatValues;

    public EntityEvent HealingEvent(int healingAmount)
    {
        EntityEvent toReturn = new EntityEvent();
        toReturn.type = EventType.HEALING;
        toReturn.intValues = new int[1];
        toReturn.intValues[0] = healingAmount;
        return toReturn;
    }
}
