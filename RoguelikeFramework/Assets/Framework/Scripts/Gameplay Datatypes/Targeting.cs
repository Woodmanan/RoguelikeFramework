using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * Targetting: a generalized class for describing how an effect should be projected
 * 
 * The class is based around a few principles:
 * 1. All effects originate from a single point (namely, the thing that casted it). This simplifies
 *    a lot of code, and we can get behaviour that circumvents this pretty easily with smite-style targetting.
 * 2. An ability may be able to select multiple targets, but each selection is exactly one point. (IE, can target 3 enemies
 *    at once, but the selection has to be made 3 times). AOE abilities select the center point of their explosions.
 * 3. Points can only be selected from areas within LOS, for the player. This won't actually be enforced on the code side,
 *    and will instead be relegated to the UI side of things. Possibly bad practice, but it keeps things clean for reuse with
 *    monster AI.
 * 
 *    
 * These are done to simplify the amount of interactions that the player has to go through, and help to keep the code
 * simple and consistent. Effects run through selecting simple points, and the backend code handles converting those to actual points.
 */

[Serializable]
public class Targeting 
{
    /**************************
     * SCRIPTABLE OBJECT STUFF
     **************************/

    //[SerializeField] bool includeBrenshamLine = false; //Encapsulated by targetting options now
    [Header("Runtime attributes")]
    public int numPoints = 1;
    public TargetType targetingType;
    public AreaType areaType;
    [SerializeField] public int range;
    [SerializeField] public int radius;
    [SerializeField][Range(0f, 180f)] float degree;

    public TargetTags options = TargetTags.POINTS_SHARE_OVERLAP | TargetTags.POINTS_REQUIRE_LOS;

    public TargetPriority targetPriority;
    

    public Targeting ShallowCopy()
    {
        return (Targeting) this.MemberwiseClone();
    }


    /***********************
     * RUNTIME OBJECT STUFF
     ***********************/

    //Make sure not to have any of these serialized by the end of testing - we don't want this clutter to end up in the actual objects
    [HideInInspector] public Vector2Int origin;
    [HideInInspector] public Vector2Int target;
    LOSData currentLOS;
    [HideInInspector] public List<Vector2Int> points;
    [HideInInspector] public bool isValid = false;
    [HideInInspector] public bool isFinished;
    [HideInInspector] public int length;
    [HideInInspector] public int offset;
    [HideInInspector] public bool[,] area;
    private bool marking = false;

    List<Monster> tempAffected; //Used for allowing overlap
    [HideInInspector] public List<Monster> affected;



    /* 
     * Logic flow for this whole thing:
     * Initialize, do any setup that needs to be done.
     * Enter targetting mode:
     *  - Determine if this thing needs to display. If not, lock in point and skip.
     *  - Accept either jumps or incremental movement
     *  - Determine if a new location is valid
     *  - Output some sort of drawing 
     * If accepted:
     *  - Add point, restart loop
     * If no more points to add:
     *  - Lock it in, crunch out the targetting info that's been requested
     */

    //Duplicates this object, and does any setup that needs to happen
    public Targeting Initialize()
    {
        return this;
    }

    public bool CanSkip()
    {
        if (targetingType == TargetType.SELF && areaType == AreaType.SINGLE_TARGET)
        {
            return true;
        }

        /*Upon testing, this seems like a dumb idea. I'll leave it here for debugging purposes, though!
        if ((targetingType == TargetType.SINGLE_SQAURE_LINES || targetingType == TargetType.SINGLE_TARGET_LINES) && (range == 0))
        {
            return true;
        }*/

        //I lied, this is a better check
        if ((range == 0) && (targetingType != TargetType.FULL_LOS && targetingType != TargetType.SELF))
        {
            Debug.LogError("Targeting that really should have range > 0 doesn't. Is this a mistake?");
        }

        return false;
    }

    public bool BeginTargetting(Vector2Int startPosition, LOSData los)
    {
        origin = startPosition;
        target = startPosition;

        currentLOS = los;
        points.Clear();
        isFinished = false;

        

        int maxEffectSize = range + radius;
        offset = maxEffectSize;
        length = maxEffectSize * 2 + 1;

        //Fix 2D problems
        if (area == null || area.Length != length)
        {
            area = new bool[length, length];
        }

        affected = new List<Monster>();
        tempAffected = new List<Monster>();

        isValid = IsValid();
        

        return !CanSkip(); //Returns true if we should open targetting, false if we can skip
    }

    public bool IsValid()
    {
        bool valid = true;
        
        valid = valid && currentLOS.ValueAtWorld(target);

        if (targetingType == TargetType.SINGLE_SQAURE_LINES || targetingType == TargetType.SINGLE_TARGET_LINES)
        {
            BresenhamResults brensham = LOS.GetLineFrom(origin, target, true, !options.HasFlag(TargetTags.LINES_PIERCE));

            valid = valid && !brensham.blocked;
        }

        if (targetingType == TargetType.SINGLE_TARGET_LINES || targetingType == TargetType.SMITE_TARGET)
        {
            Monster atTarget = Map.current.GetTile(target).currentlyStanding;
            valid = valid && (atTarget != null);
        }

        if ((options & TargetTags.REQUIRES_WALKABLE_POINT) > 0)
        {
            valid = valid && !Map.current.GetTile(target).BlocksMovement();
        }

        return valid;
    }

    //TODO: If ranges ever care about distance type, this is where we change it
    //TODO: Make this functionality a new vector2int distance function.
    //Currently relies on chebyshev distance
    public void MoveTargetOffset(Vector2Int offset)
    {
        Vector2Int newPosition = target + offset;
        if (Mathf.Abs(newPosition.x - origin.x) > range || Mathf.Abs(newPosition.y - origin.y) > range)
        {
            Debug.Log("Target offset moved target out of bounds, resetting it.");
            return;
        }

        //Target should be good! Set it now
        target = newPosition;
        isValid = IsValid(); //Another beautiful line of code, almost as bad as Player player = Player.player
    }

    public void MoveTarget(Vector2Int newPosition)
    {
        if (Mathf.Abs(newPosition.x - origin.x) > range || Mathf.Abs(newPosition.y - origin.y) > range)
        {
            Debug.Log("Target offset moved target out of bounds, resetting it.");
            return;
        }

        //Target should be good! Set it now
        target = newPosition;
        isValid = IsValid(); //Another beautiful line of code, almost as bad as Player player = Player.player
    }

    public bool LockPoint()
    {
        GenerateArea();
        if (isValid)
        {
            points.Add(target);

            //Add temp to current
            if (options.HasFlag(TargetTags.POINTS_SHARE_OVERLAP))
            {
                //Need to add uniquely
                foreach (Monster m in tempAffected)
                {
                    if (!affected.Contains(m))
                    {
                        affected.Add(m);
                    }
                }
            }
            else
            {
                foreach (Monster m in tempAffected)
                {
                    affected.Add(m);
                }
            }

            if (points.Count == numPoints)
            {
                isFinished = true;
                return true;
            }
        }
        Debug.Log("Point was locked in, but wasn't valid. Lock-in was skipped.");
        return false;
    }


    public void GenerateArea()
    {
        tempAffected.Clear();
        for (int i = 0; i < area.GetLength(0); i++)
        {
            for (int j = 0; j < area.GetLength(1); j++)
            {
                area[i, j] = false;
            }
        }

        //Am I a type that draws
        if (targetingType == TargetType.SINGLE_SQAURE_LINES || targetingType == TargetType.SINGLE_TARGET_LINES)
        {
            foreach (Vector2Int point in points)
            {
                DrawBrensham(point);
            }

            if (!isFinished)
            {
                marking = true;
                DrawBrensham(target);
            }
        }

        if (!options.HasFlag(TargetTags.INCLUDES_CASTER_SPACE))
        {
            MarkArea(origin.x, origin.y, false);
            marking = false;
        }

        foreach (Vector2Int point in points)
        {
            DrawArea(point);
        }

        if (!isFinished)
        {
            marking = true;
            DrawArea(target);
            marking = false;
        }
        
        
    }

    public bool ContainsWorldPoint(Vector2Int location)
    {
        return ContainsWorldPoint(location.x, location.y);
    }

    public bool ContainsWorldPoint(int x, int y)
    {
        int xSpot = x - origin.x + offset;
        int ySpot = y - origin.y + offset;
        return area[xSpot, ySpot];
    }

    private void MarkHighlightOnly(int x, int y, bool val)
    {
        int xSpot = x - origin.x + offset;
        int ySpot = y - origin.y + offset;
        if (xSpot >= 0 && xSpot < length && ySpot >= 0 && ySpot < length)
        {
            area[xSpot, ySpot] = val;
        }
    }

    private void MarkArea(int x, int y, bool val)
    {
        int xSpot = x - origin.x + offset;
        int ySpot = y - origin.y + offset;
        if (xSpot >= 0 && xSpot < length && ySpot >= 0 && ySpot < length)
        {
            if (options.HasFlag(TargetTags.POINTS_REQUIRE_LOS))
            {
                val = val && currentLOS.ValueAtWorld(x, y);
            }

            if (!Map.current.ValidLocation(x, y))
            {
                return;
            }

            area[xSpot, ySpot] = val && !Map.current.GetTile(x, y).isHidden;
            if (marking)
            {
                Monster m = Map.current.GetTile(x, y).currentlyStanding;
                if (m != null)
                {
                    bool contains = tempAffected.Contains(m);
                    //Need to affect a monster
                    if (val)
                    {
                        if (!contains)
                        {
                            tempAffected.Add(m);
                        }
                    }
                    else
                    {
                        tempAffected.Remove(m); //Might be expensive
                    }
                }
            }
        }
    }

    private void DrawBrensham(Vector2Int point)
    {

        BresenhamResults brensham = LOS.GetLineFrom(origin, point, true, !options.HasFlag(TargetTags.LINES_PIERCE));

        //Skip the first one
        foreach (RogueTile tile in brensham.path)
        {
            int x = tile.location.x;
            int y = tile.location.y;
            MarkArea(x, y, true);
        }
    }

    private void DrawArea(Vector2Int point)
    {
        switch (targetingType)
        {
            case TargetType.SELF:
            case TargetType.SINGLE_TARGET_LINES:
            case TargetType.SINGLE_SQAURE_LINES:
            case TargetType.SMITE:
            case TargetType.SMITE_TARGET:
                //Need to generate an area indicator around the target point
                BuildRange(point);
                break;
            case TargetType.FULL_LOS:
                //Imagine being the nerds who have to generate an area. Just fill in from LOS!
                for (int i = origin.x - radius; i <= origin.x + radius; i++)
                {
                    for (int j = origin.y - radius; j <= origin.y + radius; j++)
                    {
                        MarkArea(i, j, currentLOS.ValueAtWorld(i, j));
                    }
                }
                if (!options.HasFlag(TargetTags.INCLUDES_CASTER_SPACE))
                {
                    MarkArea(origin.x, origin.y, false);
                }
                break;
        }
    }

    private void BuildRange(Vector2Int point)
    {
        switch (areaType)
        {
            case AreaType.SINGLE_TARGET:
                MarkArea(point.x, point.y, true); //Mark us again, just for funsies
                break;
            case AreaType.CHEBYSHEV_AREA: //Easy case first
                for (int i = point.x - radius; i <= point.x + radius; i++)
                {
                    for (int j = point.y - radius; j <= point.y + radius; j++)
                    {
                        MarkArea(i, j, true);
                    }
                }
                break;
            case AreaType.MANHATTAN_AREA: //Second easiest case
                for (int i = point.x - radius; i <= point.x + radius; i++)
                {
                    for (int j = point.y - radius; j <= point.y + radius; j++)
                    {
                        if (Mathf.Abs(point.x - i) + Mathf.Abs(point.y - j) <= radius)
                        {
                            MarkArea(i, j, true);
                        }
                    }
                }
                break;
            case AreaType.EUCLID_AREA: //Most complex
                for (int i = point.x - radius; i <= point.x + radius; i++)
                {
                    for (int j = point.y - radius; j <= point.y + radius; j++)
                    {
                        if (Vector2Int.Distance(point, new Vector2Int(i, j)) <= radius) //TODO: Confirm this actually works
                        {
                            MarkArea(i, j, true);
                        }
                    }
                }
                break;
            case AreaType.LOS_AREA: //Most expensive call, but easy to understand
                LOSData data = LOS.LosAt(Map.current, point, radius);
                for (int i = point.x - radius; i <= point.x + radius; i++)
                {
                    for (int j = point.y - radius; j <= point.y + radius; j++)
                    {
                        if (data.ValueAtWorld(i, j))
                        {
                            MarkArea(i, j, true);
                        }
                    }
                }
                break;
            case AreaType.CONE: //This one is pretty wack. Try some vector fanciness?

                /* 
                 * General Strategy:
                 * Figure out the cosine value we're trying to reach (1/2 degree), and the main direction
                 * Normalize everything down to 1
                 * Run through every square, and figure out it's direction. Normalize it, dot it with
                 * other direction. Compare to cosine value, and if it's >, return a hit
                 *
                 * This trick works because dot(x, y) = |x||y|cos(theta), so by normalizing we can really
                 * cheaply compute the angle between the vectors
                 */

                float cosDeg = Mathf.Cos(Mathf.Deg2Rad * degree / 2);

                Vector2 dir = ((Vector2)(point - origin)).normalized; //Get a good indicator of our direction

                for (int i = origin.x - radius; i <= origin.x + radius; i++)
                {
                    for (int j = origin.y - radius; j <= origin.y + radius; j++)
                    {
                        //Perform checks for in cone
                        Vector2 spot = new Vector2(i - origin.x, j - origin.y);
                        float dist = Mathf.Max(Mathf.Abs(spot.x), Mathf.Abs(spot.y));
                        if (dist <= radius) //TODO: Should this be < ?
                        {
                            if (Vector2.Dot(dir, spot.normalized) > cosDeg)
                            {
                                MarkArea(i, j, true);
                            }
                        }
                    }
                }
                break;
                
        }
    }
}
