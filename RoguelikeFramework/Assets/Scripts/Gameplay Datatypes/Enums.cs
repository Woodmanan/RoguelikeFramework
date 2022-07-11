using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    WAIT,
    VIEW_INVENTORY,
    PICK_UP_ITEMS,
    DROP_ITEMS,
    ESCAPE_SCREEN,
    OPEN_INVENTORY,
    APPLY,
    EQUIP,
    UNEQUIP,
    CAST_SPELL,
    ACCEPT,
    FIRE,
    ASCEND,
    DESCEND,
    AUTO_ATTACK,
    AUTO_EXPLORE
}

[Flags]
public enum Faction
{
    STANDARD    = (1 << 0),
    PLAYER      = (1 << 1)
}

public enum DamageType
{
    NONE,
    BLUNT,
    CUTTING,
    PIERCING
}

[Flags]
public enum DamageSource
{
    MONSTER         = (1 << 0),
    ITEM            = (1 << 1),
    ABILITY         = (1 << 2),
    EFFECT          = (1 << 3),
    MELEEATTACK     = (1 << 4),
    RANGEDATTACK    = (1 << 5),
    UNARMEDATTACK   = (1 << 6)
}

public enum AttackResult
{
    HIT,
    MISSED,
    BLOCKED
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
    TAIL
}

//Order is very important here! Order written is order shown in inventory.
[Flags]
public enum ItemType
{
    MELEE_WEAPON    = (1 << 0),
    RANGED_WEAPON   = (1 << 1),
    ARMOR           = (1 << 2),
    CONSUMABLE      = (1 << 3),
    ACTIVATABLE     = (1 << 4),
    MISC            = (1 << 5)
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

public enum Resource
{
    HEALTH,
    MANA,
    STAMINA,
    XP
}

[Flags]
public enum AbilityTypes
{
    Conjuration = 1 << 0,
    Elemental   = 1 << 1,
    Healing     = 1 << 2
}

[Flags]
public enum TargetTags
{
    POINTS_SHARE_OVERLAP    = (1 << 0),
    POINTS_REQUIRE_LOS      = (1 << 1),
    LINES_PIERCE            = (1 << 2),
    INCLUDES_CASTER_SPACE   = (1 << 3),
    RECOMMENDS_SELF_TARGET  = (1 << 4),
    RECOMMNEDS_ALLY_TARGET  = (1 << 5),
    RETARGETS_SAME_MONSTER  = (1 << 6)
}

public enum TargetPriority
{
    NEAREST,
    FARTHEST,
    HIGHEST_HEALTH,
    LOWEST_HEALTH
}

public enum ItemRarity
{
    COMMON,
    UNCOMMON,
    RARE,
    EPIC,
    LEGENDARY,
    UNIQUE
}