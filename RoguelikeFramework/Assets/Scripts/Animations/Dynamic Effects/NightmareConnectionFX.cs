using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightmareConnectionFX : MonoBehaviour
{

    public float pointEvery = 1;
    public float sinHeight;
    public float sinPeriod;
    public float sinSpeed = 1f;
    [Range(1, 3)]
    public int numTendrils;

    public SpriteRenderer one;
    public SpriteRenderer two;

    LineRenderer lineRenderer;

    public void Setup(Monster one, Monster two)
    {
        this.one = one.GetComponent<SpriteRenderer>();
        this.two = two.GetComponent<SpriteRenderer>();
    }

    public void SetupLine()
    {
        pointEvery = Mathf.Max(pointEvery, 0.01f);
        int numPoints = Mathf.RoundToInt(Vector3.Distance(one.transform.position, two.transform.position) / pointEvery);
        Vector3[] positions = new Vector3[numPoints * numTendrils];
        for (int c = 0; c < numTendrils; c++)
        {



            Vector3 start = one.transform.position;
            Vector3 end = two.transform.position;

            //Swap for backwards drawing
            if (c % 2 == 1)
            {
                Vector3 temp = end;
                end = start;
                start = temp;
            }

            lineRenderer.positionCount = numPoints * numTendrils;

            Vector3 sideways = Vector3.Cross(start - end, Vector3.forward).normalized;
            float dist = Vector3.Distance(start, end);

            for (int i = 0; i < numPoints; i++)
            {
                float t = ((float)i) / numPoints;

                positions[i + c * numPoints] = Vector3.Lerp(start, end, t) + sinHeight * Mathf.Sin(dist * t * sinPeriod + Time.time * sinSpeed) * sideways;
                positions[i + c * numPoints].z = -7;
            }
            lineRenderer.SetPositions(positions);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        SetupLine();
    }
}
