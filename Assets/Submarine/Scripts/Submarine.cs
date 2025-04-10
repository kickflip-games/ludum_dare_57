using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Submarine : MonoBehaviour {
    public float maxSpeed = 5;
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
    float currentSpeed;
    bool isTurning;
    public Material propSpinMat;
    
    public bool isPaused = false;
    Renderer[] renderers;

    // Reference to the GyroHandler (assign this via the Inspector)
    public GyroHandler gyroHandler;
    
    private List<SubmarineTrail> Trails = new List<SubmarineTrail>();
    
    void Start () {
        currentSpeed = maxSpeed / 2.0f;
        renderers = GetComponentsInChildren<Renderer>();
        foreach (SubmarineTrail t in GetComponentsInChildren<SubmarineTrail>()) {
            Trails.Add(t);
        }
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
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed / 2.0f, maxSpeed);
        float speedPercent = Mathf.Clamp(currentSpeed / maxSpeed, -1, 1);

        // --- Determine control mode ---
        // 1. If running on mobile and gyroscope input is active, use it.
        if (Application.isMobilePlatform && gyroHandler != null && gyroHandler.hasGyroInput) {
            // Map the gyro data to yaw and pitch velocities.
            // Adjust the multipliers as needed for your game feel.
            float targetYawVelocity = gyroHandler.rotationRate.y * maxTurnSpeed;
            float targetPitchVelocity = gyroHandler.rotationRate.x * maxPitchSpeed;
            
            yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);
            pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);
            
            transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * speedPercent;
            rudderYaw.localEulerAngles = Vector3.up * (yawVelocity / maxTurnSpeed) * rudderAngle;
            rudderPitch.localEulerAngles = Vector3.left * (pitchVelocity / maxPitchSpeed) * rudderAngle;
        }
        // 2. Else, if the left mouse button is held, use mouse-based control.
        else if (Input.GetMouseButton(0)) {
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
            
            transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * speedPercent;
            rudderYaw.localEulerAngles = Vector3.up * (yawVelocity / maxTurnSpeed) * rudderAngle;
            rudderPitch.localEulerAngles = Vector3.left * (pitchVelocity / maxPitchSpeed) * rudderAngle;
        }
        // 3. Otherwise, fall back to keyboard-based controls.
        else {
            float targetPitchVelocity = Input.GetAxisRaw("Vertical") * maxPitchSpeed;
            pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);

            float targetYawVelocity = Input.GetAxisRaw("Horizontal") * maxTurnSpeed;
            yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);

            transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * speedPercent;
            rudderYaw.localEulerAngles = Vector3.up * (yawVelocity / maxTurnSpeed) * rudderAngle;
            rudderPitch.localEulerAngles = Vector3.left * (pitchVelocity / maxPitchSpeed) * rudderAngle;
        }
        
        // Move the submarine forward.
        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);

        // Rotate the propeller and update material transparency based on speed.
        propeller.Rotate(Vector3.forward * Time.deltaTime * propellerSpeedFac * speedPercent, Space.Self);
        propSpinMat.color = new Color(propSpinMat.color.r, propSpinMat.color.g, propSpinMat.color.b, speedPercent * 0.3f);
        
        isTurning = Mathf.Abs(yawVelocity) > 10.0f || Mathf.Abs(pitchVelocity) > 10.0f;
        ToggleTrailIfTurning();
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
}
