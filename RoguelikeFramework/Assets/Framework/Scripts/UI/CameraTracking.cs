using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Basic tracking script for keeping up with the player
//TODO: Make this play nicely with the animation system!

/***** Explanation of the modes **********
 * 
 * Jump - Jumps to character position, no animation. Old school style.
 * 
 * Constant Speed - Move to character position with a constant speed.
 *                  Be wary of moving too slow, and losing the character.
 * 
 * Lerp - Lerps between current position and character position.
 *        If the distance is < stopDist, switch into constant speed mode,
 *        using the current lerp speed as the new constant speed. This helps
 *        to prevent aliasing when moving sub pixel values, as well as prevent
 *        the stutter effect that occurs if you just jump to the right spot
 *        when you get close. To make it seamless, I added a stopSpeedMultiplier,
 *        which I manually finagled till it felt right.
 *        
 ****************************************/

//This fix got done on a whim, so it's some of the messiest code in the project.
//If you think there's a more clever way to do this, please please please feel
//free to modify this.

public enum CameraTrackingMode
{
    Jump,
    ConstantSpeed,
    Lerp,
}

public class CameraTracking : MonoBehaviour
{
    const float camera_z_position = -15;

    private static CameraTracking Singleton;
    public static CameraTracking singleton
    {
        get
        {
            if (Singleton) return Singleton;
            else Singleton = Camera.main.GetComponent<CameraTracking>();
            return Singleton;
        }
        set
        {
            Singleton = value;
        }
    }

    public CameraTrackingMode mode;
    public float speed;
    public float lerpAmount;
    [Tooltip("The distance where the lerping switches to constant speed")]
    public float stopDist;
    public float stopSpeed = -1;
    [Tooltip("When switching to constant speed, the amount by which to increase the speed")]
    public float stopSpeedMultiplier = 1f;

    // Start is called before the first frame update
    void Start()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }

        if (Singleton != this)
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void OnPreRender()
    {
        if (!Player.player) return;
        Vector2 target = Player.player[0].unity.transform.position;

        switch (mode)
        {
            case CameraTrackingMode.Jump:
                break;
            case CameraTrackingMode.ConstantSpeed:
                Vector2 dir = target - (Vector2) transform.position;
                float dist = dir.magnitude;
                dir = dir.normalized * speed * Time.deltaTime;
                if (dist < dir.magnitude)
                {
                    target = Player.player[0].unity.transform.position;
                }
                else
                {
                    target = (Vector3) dir + transform.position;
                }
                break;
            case CameraTrackingMode.Lerp:
                float lerpDist = (target - (Vector2) transform.position).magnitude;
                target = Vector2.Lerp(transform.position, target, lerpAmount * Time.deltaTime);
                if (lerpDist < stopDist)
                {
                    if (stopSpeed < 0) stopSpeed = ((Vector2)Player.player[0].unity.transform.position - target).magnitude * stopSpeedMultiplier;
                    target = Player.player[0].unity.transform.position;
                    //Switch to constant speed for stop!
                    dir = target - (Vector2)transform.position;
                    dist = dir.magnitude;
                    dir = dir.normalized * stopSpeed * Time.deltaTime;
                    if (dist < dir.magnitude)
                    {
                        target = Player.player[0].unity.transform.position;
                    }
                    else
                    {
                        target = (Vector3)dir + transform.position;
                    }
                }
                else
                {
                    stopSpeed = -1;
                }
                break;
        }

        transform.position = new Vector3(target.x, target.y, camera_z_position);
    }

    public void JumpToPlayer()
    {
        Vector3 target = Player.player[0].unity.transform.position;
        transform.position = new Vector3(target.x, target.y, camera_z_position);
    }
}
