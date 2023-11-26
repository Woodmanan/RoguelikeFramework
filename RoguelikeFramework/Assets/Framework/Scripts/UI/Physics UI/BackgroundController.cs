using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    public static BackgroundController Singleton;
    public static BackgroundController singleton
    {
        get
        {
            if (!Singleton)
            {
                Singleton = GameObject.FindObjectOfType<BackgroundController>();
            }

            return Singleton;
        }

        set
        {
            Singleton = value;
        }
    }

    Material material;
    public bool hasChanges = false;

    public List<Rect> values;
    // Start is called before the first frame update
    void Start()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        if (Singleton != this)
        {
            Destroy(this.gameObject);
            return;
        }

        material = GetComponent<Image>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasChanges)
        {   
            float[] rectValues = new float[200];
            for (int i = 0; i < values.Count; i++)
            {
                int offset = i * 4;
                rectValues[offset + 0] = values[i].x;
                rectValues[offset + 1] = values[i].y;
                rectValues[offset + 2] = values[i].width;
                rectValues[offset + 3] = values[i].height;
            }

            material.SetFloatArray("_boxRects", rectValues);
            material.SetInt("_numRects", values.Count);

            hasChanges = false;
        }
    }
}
