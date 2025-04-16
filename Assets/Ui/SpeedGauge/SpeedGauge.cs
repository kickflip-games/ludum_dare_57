using UnityEngine;


[ExecuteAlways]
public class SpeedGauge : MonoBehaviour
{
    public float[] valueRange = { 0, 1 };
    public float value;


    [Tooltip("The position of the needle on the gauge (0.0f = 0 degrees / min, 1.0f = -90 degrees/max)")]
    [Range(0, 1)]
    [SerializeField]
    private float NeedlePosition = 0.5f; // 0.0f = 0 degrees, 1.0f = -90 degrees

    private GameObject _needle;
    private readonly float[] _rotatationRange = { 0.1f, -89.9f };

    private Submarine _player;

    private void Awake()
    {
        _needle = transform.Find("Needle").gameObject;
        _player = FindObjectsByType<Submarine>(FindObjectsSortMode.None)[0];
    }
    
    private void OnEnable()
    {
        if (_player != null)
        {
            Debug.Log("Player found. Setting needle speed");
            valueRange = new[] { _player.MinSpeed, _player.maxSpeed };
            value = _player.currentSpeed;
            _player.OnSpeedChanged += UpdateNeedle;
        }
        else
        {
            value = (valueRange[0] + valueRange[1]) / 2;
        }
    }
    
    private void OnDisable()
    {
        if (_player != null)
        {
            _player.OnSpeedChanged -= UpdateNeedle;
        }
    }
    
    private void Update()
    {
         //Only editor moode
        if (!Application.isPlaying)
            UpdateNeedle(value);
    }
    

    void UpdateNeedle(float val)
    {
        
        value = Mathf.Clamp(val, valueRange[0], valueRange[1]);
        // map value (min_val, max_val) to set the needle position (0.0f,  1.0f)
        NeedlePosition = Mathf.InverseLerp(valueRange[0], valueRange[1], value);
        float rotation = Mathf.Lerp(_rotatationRange[0], _rotatationRange[1], NeedlePosition);
        _needle.transform.localRotation = Quaternion.Euler(0, 0, rotation);
    }

    public void UpdatePlayerSpeed(float val)
    {
        value = Mathf.Lerp(valueRange[0], valueRange[1], val);
        if (_player != null)
        {
            float newSpeed = Mathf.Lerp(_player.MinSpeed, _player.maxSpeed, val);
            _player.currentSpeed = newSpeed;
            // UpdateNeedle(val);
        }
            
    }
    
}