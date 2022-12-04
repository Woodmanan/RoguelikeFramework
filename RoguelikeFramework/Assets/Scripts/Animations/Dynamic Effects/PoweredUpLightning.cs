using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredUpLightning : MonoBehaviour
{
    public RogueTile tower;
    public SpriteRenderer monster;
    LineRenderer lineRenderer;

    public float minOffset;
    public float maxOffset;
    public float minWidth;
    public float maxWidth;

    public float pointEvery = 1;

    bool on;

    public void Setup(Monster monster, RogueTile tower)
    {
        this.tower = tower;
        this.monster = monster.GetComponent<SpriteRenderer>();
        HandlePowerState(tower.isVisible && monster.enabled);
    }

    public void CheckPowerState()
    {
        if (tower == null || monster == null)
        {
            HandlePowerState(false);
        }
        else
        {
            HandlePowerState(monster.enabled); //|| tower.isVisible);
        }
    }

    public void HandlePowerState(bool newState)
    {
        if (on != newState)
        {
            on = newState;
            if (on)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }

    public void Show()
    {
        lineRenderer.enabled = true;
    }

    public void Hide()
    {
        lineRenderer.enabled = false;
    }

    public void SetupLine()
    {
        pointEvery = Mathf.Max(pointEvery, 0.01f);
        Vector3 start = tower.transform.position;
        Vector3 end = monster.transform.position;
        int numPoints = Mathf.RoundToInt(Vector3.Distance(start, end) / pointEvery);
        lineRenderer.positionCount = numPoints;

        Vector3[] positions = new Vector3[numPoints];
        Keyframe[] keys = new Keyframe[numPoints];
        
        Vector3 sideways = Vector3.Cross(start - end, Vector3.forward).normalized;

        for (int i = 0; i < numPoints; i++)
        {
            float t = ((float)i) / numPoints;

            positions[i] = Vector3.Lerp(start, end, t) + Random.Range(minOffset, maxOffset) * sideways;
            keys[i] = new Keyframe(Random.Range(minWidth, maxWidth), t);
        }
        lineRenderer.SetPositions(positions);
        lineRenderer.widthCurve.keys = keys;
    }

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        CheckPowerState();
        if (on)
        {
            SetupLine();
        }
    }
}
