using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAnimation : TargetingAnimation
{
    public GameObject system;

    ParticleSystem instance;

    public bool useTextureForShape;
    public bool textureBlendsColor;

    public ParticleAnimation() : base()
    {
        
    }

    public override void OnVariablesGenerated(Targeting targeting)
    {

    }

    public override void OnStart()
    {
        instance = GameObject.Instantiate(system).GetComponent<ParticleSystem>();
        instance.transform.position = destination;
        MaxDuration = instance.main.duration;

        if (useTextureForShape)
        {
            ParticleSystem.ShapeModule shape = instance.shape;
            shape.texture = owner[0].unity.renderer.sprite.texture;
            shape.textureColorAffectsParticles = textureBlendsColor;
        }

        instance.Play();
    }

    public override void OnStep(float delta)
    {
        
    }

    public override void OnEnd()
    {
        GameObject.Destroy(instance.gameObject);
    }
}
