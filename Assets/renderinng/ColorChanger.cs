using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorChanger : StaticInstance<ColorChanger>
{
    public VolumeProfile volumeProfile;
    [Range(0f, 100f)]
    public float dirtPercentage = 0f;
    public Gradient colorGradient;

    [SerializeField]
    private float maxBloomDirtIntensity = 10f;


    private ColorAdjustments colorAdjustments;
    private Bloom bloom;

    protected override void Awake()
    {
        base.Awake();

        if (volumeProfile == null)
        {
            Debug.LogError("Volume Profile not assigned to ColorChanger script on " + gameObject.name);
            enabled = false;
            return;
        }

        if (colorGradient == null)
        {
            Debug.LogError("Color Gradient not assigned to ColorChanger script on " + gameObject.name);
            enabled = false;
            return;
        }

        if (!volumeProfile.TryGet(out colorAdjustments))
        {
            Debug.LogError("Color Adjustments not found in Volume Profile on " + gameObject.name);
            enabled = false;
            return;
        }
        if (!volumeProfile.TryGet(out bloom))
        {
            Debug.LogError("Bloom not found in Volume Profile on " + gameObject.name);
            enabled = false;
            return;
        }

        UpdatePostProc();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UpdatePostProc();
    }
#endif

    public void SetDirtPercentage(float percentage)
    {
        dirtPercentage = Mathf.Clamp(percentage, 0f, 100f);
        UpdatePostProc();
    }

    public float GetDirtPercentage()
    {
        return dirtPercentage;
    }

    private void UpdatePostProc()
    {
        if (colorAdjustments == null || colorGradient == null || bloom == null)
            return;

        // use exponential curve for dirt intensity
        bloom.dirtIntensity.value = Mathf.Pow(dirtPercentage / 100f, 2) * maxBloomDirtIntensity;

        Color targetColor = colorGradient.Evaluate(dirtPercentage / 100f);
        colorAdjustments.colorFilter.value = targetColor;
        colorAdjustments.colorFilter.overrideState = true;
    }
}