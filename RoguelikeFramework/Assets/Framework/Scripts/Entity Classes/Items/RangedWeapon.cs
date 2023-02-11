using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RangedWeapon : Weapon
{
    public Targeting targeting;
    [SerializeReference]
    public List<RogueAnimation> animations;
}
