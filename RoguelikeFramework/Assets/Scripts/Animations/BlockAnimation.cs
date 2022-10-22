using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Temporary buffer class - lets some systems insert blockers to force other anims to finish.
public class BlockAnimation : RogueAnimation
{
    public BlockAnimation() : base(0.0f, true)
    {

    }
}
