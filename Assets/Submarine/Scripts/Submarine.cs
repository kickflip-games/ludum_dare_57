using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Submarine : MonoBehaviour {
    public float maxSpeed = 5;

    public float MinSpeed
    {
        get { return -maxSpeed / 2.0f; }
    }
    [SerializeField]
    private float _speed;

    
    public float currentSpeed
    {
        get => _speed;
        set
        {
            float clampedValue = Mathf.Clamp(value, MinSpeed, maxSpeed);
            if (Mathf.Approximately(_speed, clampedValue)) return;
            _speed = clampedValue;
            OnSpeedChanged?.Invoke(_speed);
            _speedPercent = Mathf.Clamp(currentSpeed / maxSpeed, -1, 1);
            SetFx();
        }
    }
    
    public event Action<float> OnSpeedChanged;
    
    public float maxPitchSpeed = 3;
    public float maxTurnSpeed = 50;
    public float acceleration = 2;

    public float smoothSpeed = 3;
    public float smoothTurnSpeed = 3;

    public Transform propeller;
    public Transform rudderPitch;
    public Transform rudderYaw;
    public float propellerSpeedFac = 2;
    public float rudderAngle = 30;
    
    // Tunable variable for mouse sensitivity
    public float mouseSensitivity = 1.0f;

    public int score;
    
    

    
    Vector3 velocity;
    float yawVelocity;
    float pitchVelocity;
    private float _speedPercent;
    bool isTurning;
    public Material propSpinMat;
    
    public bool isPaused = false;
    Renderer[] renderers;

    // Reference to the GyroHandler (assign these via the Inspector)
    public GyroHandler gyroHandler;
    public VariableJoystick variableJoystick; 
    
    private List<SubmarineTrail> Trails = new List<SubmarineTrail>();
    private SubFx _subFx;
    
    void Start () {
        _subFx = GetComponentInChildren<SubFx>();
        foreach (SubmarineTrail t in GetComponentsInChildren<SubmarineTrail>()) {
            Trails.Add(t);
        }
        currentSpeed = maxSpeed / 2.0f;
        renderers = GetComponentsInChildren<Renderer>();

    }
    
    public void ToggleTrailIfTurning() {

        foreach (var trail in Trails) {
            if (isTurning) {
                trail.ToggleTrail(true);
            } else {
                trail.ToggleTrail(false);
            }
        }
    }
    
    
    
    
    void Update () {
        if (isPaused) {
            return;
        }
        
        // Handle acceleration with Q (decelerate) and E (accelerate)
        float accelDir = 0;
        if (Input.GetKey(KeyCode.Q)) {
            accelDir -= 1;
        }
        if (Input.GetKey(KeyCode.E)) {
            accelDir += 1;
        }
        
        currentSpeed += acceleration * Time.deltaTime * accelDir;
        
        

        // --- Determine control mode ---
        // 1. If running on mobile and gyroscope input is active, use it.
        if (Application.isMobilePlatform && gyroHandler != null && gyroHandler.hasGyroInput) {
            GyroInput();
        }
        // 2. Else, if the left mouse button is held, use mouse-based control.
        // else if (Input.GetMouseButton(0)) {
        //     MouseInput();
        // }
        // 3. Otherwise, fall back to keyboard-based controls.
        else if (variableJoystick!= null && variableJoystick.gameObject.activeSelf) {
            JoystickInput();
        }
        else {
            KeyboardInput();
        }
        
        // Move the submarine forward.
        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);

        // Rotate the propeller and update material transparency based on speed.
        propeller.Rotate(Vector3.forward * Time.deltaTime * propellerSpeedFac * _speedPercent, Space.Self);
        propSpinMat.color = new Color(propSpinMat.color.r, propSpinMat.color.g, propSpinMat.color.b, _speedPercent * 0.3f);
        
        isTurning = Mathf.Abs(yawVelocity) > 10.0f || Mathf.Abs(pitchVelocity) > 10.0f;
        ToggleTrailIfTurning();
    }

    void KeyboardInput()
    {
        float targetPitchVelocity = Input.GetAxisRaw("Vertical") * maxPitchSpeed;
        pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);

        float targetYawVelocity = Input.GetAxisRaw("Horizontal") * maxTurnSpeed;
        yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);

        transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * _speedPercent;
        rudderYaw.localEulerAngles = Vector3.up * (yawVelocity / maxTurnSpeed) * rudderAngle;
        rudderPitch.localEulerAngles = Vector3.left * (pitchVelocity / maxPitchSpeed) * rudderAngle;
    }

    void MouseInput()
    {
        Vector3 mousePos = Input.mousePosition;
        // Set z so that ScreenToWorldPoint can use it correctly
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 targetPoint = Camera.main.ScreenToWorldPoint(mousePos);

        // Calculate the desired direction to the target point
        Vector3 desiredDirection = (targetPoint - transform.position).normalized;

        // Compute yaw difference (horizontal rotation)
        Vector3 flatCurrent = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 flatDesired = Vector3.ProjectOnPlane(desiredDirection, Vector3.up).normalized;
        float targetYawDiff = Vector3.SignedAngle(flatCurrent, flatDesired, Vector3.up);

        // Compute pitch difference (vertical rotation) using the submarine’s right vector.
        float targetPitchDiff = -Vector3.SignedAngle(transform.forward, desiredDirection, transform.right);

        // Map differences to target velocities (using 45° for full input)
        float targetYawVelocity = (targetYawDiff / 45f) * maxTurnSpeed * mouseSensitivity;
        float targetPitchVelocity = (targetPitchDiff / 45f) * maxPitchSpeed * mouseSensitivity;

        yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);
        pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);
            
        transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * _speedPercent;
        rudderYaw.localEulerAngles = Vector3.up * (yawVelocity / maxTurnSpeed) * rudderAngle;
        rudderPitch.localEulerAngles = Vector3.left * (pitchVelocity / maxPitchSpeed) * rudderAngle;
    }

    void GyroInput()
    {
        // Map the gyro data to yaw and pitch velocities.
        // Adjust the multipliers as needed for your game feel.
        float targetYawVelocity = gyroHandler.rotationRate.y * maxTurnSpeed;
        float targetPitchVelocity = gyroHandler.rotationRate.x * maxPitchSpeed;
            
        yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);
        pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);
            
        transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * _speedPercent;
        rudderYaw.localEulerAngles = Vector3.up * (yawVelocity / maxTurnSpeed) * rudderAngle;
        rudderPitch.localEulerAngles = Vector3.left * (pitchVelocity / maxPitchSpeed) * rudderAngle;
    }

    void JoystickInput()
    {
        float targetPitchVelocity = variableJoystick.Vertical * maxPitchSpeed;
        pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);

        float targetYawVelocity = variableJoystick.Horizontal * maxTurnSpeed;
        yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);

        transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * _speedPercent;
        rudderYaw.localEulerAngles = Vector3.up * (yawVelocity / maxTurnSpeed) * rudderAngle;
        rudderPitch.localEulerAngles = Vector3.left * (pitchVelocity / maxPitchSpeed) * rudderAngle;
    }
    
    public void TakeDamage () {
        // Flash all renderers briefly.
        StartCoroutine(FlashRenderers());

        // Move submarine back a few meters upon taking damage.
        Vector3 back = transform.position - transform.forward * 2;
        transform.position = back;
    }

    IEnumerator FlashRenderers () {
        // Flash renderers on and off for 0.5 seconds.
        for (int i = 0; i < 10; i++) {
            foreach (var renderer in renderers) {
                renderer.enabled = !renderer.enabled;
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void IncreaseScore() {
        score++;
        print("Score: " + score);
    }

    public void PauseMovement(bool pause) {
        isPaused = pause;
    }

    void SetFx()
    {
        _subFx.PercentParicles = _speedPercent;
    }
    
}
