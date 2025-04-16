using System;
using UnityEngine;

[ExecuteAlways]
public class SubFx : MonoBehaviour
{
    private ParticleSystem trailFx;
    private ParticleSystem circleFx;

    
    [Range(0, 1)]
    public float PercentParicles = 0.0f;
    
    [Range(0, 100)]
    [SerializeField]
    private float maxCircleRate = 100.0f;
    
    [Range(0, 100)]
    [SerializeField]
    private float maxTrailRate = 100.0f;
    
    
    
    private float _rateOverTime;
    private float RateOverTime
    {
        get => _rateOverTime;
        set
        {
            if (value < 0)
            {
                value = 0;
            }
            _rateOverTime = value;
            var trailEmmision = trailFx.emission;
            var circleEmmision = circleFx.emission;
            
            trailEmmision.rateOverTime = maxTrailRate * PercentParicles;
            circleEmmision.rateOverTime = maxCircleRate * PercentParicles;
        }
    }

    private void Start()
    {
        trailFx = transform.Find("TrailFx").GetComponent<ParticleSystem>();
        circleFx = transform.Find("CircleFx").GetComponent<ParticleSystem>();

    }
    
    
    private void Update()
    {
        if (Application.isPlaying)
        {
            RateOverTime = Mathf.Lerp(RateOverTime, maxCircleRate * PercentParicles, Time.deltaTime);
        }
        else
        {
            RateOverTime = maxCircleRate * PercentParicles;
        }
    }
    
}