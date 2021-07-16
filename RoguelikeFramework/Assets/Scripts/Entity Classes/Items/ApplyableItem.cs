using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyableItem : MonoBehaviour
{

    public List<Effect> effectsToApply;

    // Start is called before the first frame update
    void Start()
    {
        this.enabled = false; //Saves us some power
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Apply(Monster m)
    {
        foreach (Effect e in effectsToApply)
        {
            Effect nextEffect = Instantiate(e);
            m.AddEffect(nextEffect);
        }
    }
}
