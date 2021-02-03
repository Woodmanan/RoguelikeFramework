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
 */
public enum MapSpace
{
    Chebyshev,
    Euclidean,
    Manhattan
}

/* Really can't decide if this should be plural or not.
 * If you have a stronger opinion than I, feel free to
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
    EQUIP,
    UNEQUIP,
    ACCEPT
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

public enum UIState
{
    NONE,
    INVENTORY,
    PICKUP_MANY
}