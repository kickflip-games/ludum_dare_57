using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SubmarineTrail : MonoBehaviour
{
    public Color startColor = Color.white;

    public Color endColor = new Color(0, 0, 0, 0);
    public float trailTime = 0.5f;
    
    // public Material trailMaterial;
    private TrailRenderer _trail;
    private bool IsOn = false;
    


    public void Start()
    {
        IsOn = false;
        _trail = GetComponent<TrailRenderer>();
        _trail.time = trailTime;
        _trail.startColor = startColor;
        _trail.endColor = endColor;
        ToggleTrail(false);
    }

    public void ToggleTrail(bool turnOn)
    {
        if (turnOn && !IsOn) // Trail is off and we have just toggled it on
        {
            // Debug.Log("Trail is off and we have just toggled it on");
            IsOn = true;
            GraduallyChangeDuration(0, trailTime, 0.2f);
        }
        else if (!turnOn && IsOn) // Trail is on and we have just toggled it off
        {
            // Debug.Log("Trail is on and we have just toggled it off");
            IsOn = false;
            GraduallyChangeDuration(trailTime, 0, 0.2f);
        }
    }

    void GraduallyChangeDuration(float start, float end, float duration_to_change)
    {
        _trail.time = start;
        DOTween.To(() => _trail.time, x => _trail.time = x, end, duration_to_change).OnComplete(() =>
        {
            _trail.time = end;
        });
    }
    

    private IEnumerator DestroyTrail(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(_trail);
        _trail = null;
    }



}