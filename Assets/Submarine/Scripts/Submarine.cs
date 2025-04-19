using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Submarine : MonoBehaviour {
    public float maxSpeed = 5;

    public float MinSpeed {
        get { return -maxSpeed / 2.0f; }
    }

    [SerializeField]
    private float _speed;

    public float currentSpeed {
        get => _speed;
        set {
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

    // Input references
    // public GyroHandler gyroHandler;
    public VariableJoystick variableJoystick;

    private List<SubmarineTrail> Trails = new List<SubmarineTrail>();
    private SubFx _subFx;

    void Start() {
        _subFx = GetComponentInChildren<SubFx>();
        foreach (SubmarineTrail t in GetComponentsInChildren<SubmarineTrail>()) {
            Trails.Add(t);
        }
        currentSpeed = maxSpeed / 2.0f;
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update() {
        if (isPaused) return;

        // Handle acceleration (Q to decelerate, E to accelerate)
        float accelDir = 0;
        if (Input.GetKey(KeyCode.Q)) accelDir -= 1;
        if (Input.GetKey(KeyCode.E)) accelDir += 1;
        currentSpeed += acceleration * Time.deltaTime * accelDir;

        // Combine keyboard and joystick input
        CombinedInput();

        // // Fallback to gyro input if no directional input and on mobile with gyro
        // if (Mathf.Approximately(pitchVelocity, 0f) && Mathf.Approximately(yawVelocity, 0f)
        //     && Application.isMobilePlatform && gyroHandler != null && gyroHandler.hasGyroInput) {
        //     GyroInput();
        // }

        // Move submarine forward
        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);

        // Animate propeller and adjust material transparency
        propeller.Rotate(Vector3.forward * Time.deltaTime * propellerSpeedFac * _speedPercent, Space.Self);
        propSpinMat.color = new Color(propSpinMat.color.r, propSpinMat.color.g, propSpinMat.color.b, _speedPercent * 0.3f);

        // Trail toggling
        isTurning = Mathf.Abs(yawVelocity) > 10.0f || Mathf.Abs(pitchVelocity) > 10.0f;
        ToggleTrailIfTurning();
    }

    void CombinedInput() {
        float targetPitchVelocity = 0f;
        float targetYawVelocity = 0f;

        // Keyboard axes
        targetPitchVelocity += Input.GetAxisRaw("Vertical") * maxPitchSpeed;
        targetYawVelocity += Input.GetAxisRaw("Horizontal") * maxTurnSpeed;

        // Joystick axes
        if (variableJoystick != null && variableJoystick.gameObject.activeSelf) {
            targetPitchVelocity += variableJoystick.Vertical * maxPitchSpeed;
            targetYawVelocity += variableJoystick.Horizontal * maxTurnSpeed;
        }

        // Smooth transitions
        pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);
        yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);

        // Apply rotation
        transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * _speedPercent;

        // Rudder visuals
        rudderYaw.localEulerAngles = Vector3.up * (yawVelocity / maxTurnSpeed) * rudderAngle;
        rudderPitch.localEulerAngles = Vector3.left * (pitchVelocity / maxPitchSpeed) * rudderAngle;
    }

    // void GyroInput() {
    //     float targetYawVelocity = gyroHandler.rotationRate.y * maxTurnSpeed;
    //     float targetPitchVelocity = gyroHandler.rotationRate.x * maxPitchSpeed;
    //
    //     yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);
    //     pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);
    //
    //     transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * _speedPercent;
    //     rudderYaw.localEulerAngles = Vector3.up * (yawVelocity / maxTurnSpeed) * rudderAngle;
    //     rudderPitch.localEulerAngles = Vector3.left * (pitchVelocity / maxPitchSpeed) * rudderAngle;
    // }

    public void TakeDamage() {
        StartCoroutine(FlashRenderers());
        Vector3 back = transform.position - transform.forward * 2;
        transform.position = back;
    }

    IEnumerator FlashRenderers() {
        for (int i = 0; i < 10; i++) {
            foreach (var renderer in renderers) {
                renderer.enabled = !renderer.enabled;
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void IncreaseScore() {
        score++;
        Debug.Log("Score: " + score);
    }

    public void PauseMovement(bool pause) {
        isPaused = pause;
        if (pause) _subFx.PercentParicles = 0;
    }

    void SetFx() {
        _subFx.PercentParicles = _speedPercent;
    }

    void ToggleTrailIfTurning() {
        foreach (var trail in Trails) {
            trail.ToggleTrail(isTurning);
        }
    }
}
