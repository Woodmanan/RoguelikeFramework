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
    ACTIVATE
}

[System.Flags]
public enum SlotType
{
    NONE = 0,
    HEAD = (1 << 0),
    LEFT_HAND = (1 << 1),
    RIGHT_HAND = (1 << 2),
    BODY = (1 << 3),
    RANGED_WEAPON = (1 << 4),
    TAIL = (1 << 5)
}

public enum UIState
{
    NONE,
    INVENTORY,
    PICKUP_MANY
}