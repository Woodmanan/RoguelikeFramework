using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    NORTH,
    EAST,
    SOUTH,
    WEST,
    NORTH_WEST,
    NORTH_EAST,
    SOUTH_WEST,
    SOUTH_EAST
}

/*
 * Different types of space:
 * 1. Chebyshev: 8 way movement
 * 2. Euclidean: 8 way movement, corners are sqrt(2)
 * 3. Manhattan: 4 way movement
 */
public enum MapSpace
{
    Chebyshev,
    Euclidean,
    Manhattan
}

/* Really can't decide if this should be plural or not.
 * If you have a stronger opinion than I do, feel free to
 * yell at me and make me change it.
 */
public enum PlayerAction
{
    NONE,
    MOVE_LEFT,
    MOVE_RIGHT,
    MOVE_UP,
    MOVE_DOWN,
    MOVE_UP_LEFT,
    MOVE_UP_RIGHT,
    MOVE_DOWN_LEFT,
    MOVE_DOWN_RIGHT,
    VIEW_INVENTORY,
    PICK_UP_ITEMS,
    DROP_ITEMS,
    ESCAPE_SCREEN,
    OPEN_INVENTORY,
    APPLY,
    EQUIP,
    UNEQUIP,
    ACCEPT,
    FIRE
}

public enum DamageType
{
    NONE,
    BLUNT,
    CUTTING,
    PIERCING
}

public enum EventType
{
    NONE,
    GAIN_ENERGY,
    MELEE_ATTACK,
    UNARMED_ATTACK,
    RANGED_ATTACK,
    HEALING,
    STATUS_EFFECT
}

public enum ItemAction
{
    INSPECT,
    DROP,
    PICK_UP,
    APPLY,
    ACTIVATE,
    EQUIP,
    UNEQUIP
}

public enum EquipSlotType
{
    NONE,
    HEAD,
    PRIMARY_HAND,
    SECONDARY_HAND,
    BODY,
    RANGED_WEAPON,
    TAIL
}

//Order is very important here! Order written is order shown in inventory.
public enum ItemType
{
    NONE,
    WEAPON,
    ARMOR,
    CONSUMABLE,
    ACTIVATABLE,
    EMPTY
}

public enum TargetType
{
    SELF, //Just picks the caster, and immediantly returns
    SINGLE_TARGET_LINES, //Any valid target (monster), reachable by Brensham lines
    SINGLE_SQAURE_LINES, //Any square, reachable by Brensham lines
    SMITE, //Any square in LOS
    SMITE_TARGET,
    FULL_LOS, //All valid targets in LOS
}

public enum AreaType
{
    SINGLE_TARGET, //Essentially range of 0
    CONE, //A cone-shaped area
    MANHATTAN_AREA, //Range in a diamond shape
    EUCLID_AREA, //Range in a circle
    CHEBYSHEV_AREA, //Range in a square
    LOS_AREA //Like chebyshev, but respects LOS from the given square. Most accurate form of targetting.
}

public enum UIState
{
    NONE,
    INVENTORY,
    PICKUP_MANY
}