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
    public Material propSpinMat;
    
    public bool isPaused = false;

    Renderer[] renderers;
    
    void Start () {
        currentSpeed = maxSpeed / 2.0f;
        renderers = GetComponentsInChildren<Renderer> ();
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
        currentSpeed = Mathf.Clamp (currentSpeed, -maxSpeed/2.0f, maxSpeed);
        float speedPercent = Mathf.Clamp(currentSpeed / maxSpeed, -1, 1);

        // Use mouse control if left mouse button is held, otherwise use keyboard controls.
        if (Input.GetMouseButton(0)) {
            // Mouse-based control
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 targetPoint = Camera.main.ScreenToWorldPoint(mousePos);

            // Calculate the desired direction to the target point
            Vector3 desiredDirection = (targetPoint - transform.position).normalized;

            // Compute the yaw difference (horizontal rotation)
            Vector3 flatCurrent = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 flatDesired = Vector3.ProjectOnPlane(desiredDirection, Vector3.up).normalized;
            float targetYawDiff = Vector3.SignedAngle(flatCurrent, flatDesired, Vector3.up);

            // Compute the pitch difference (vertical rotation) using the submarine’s right vector as the axis.
            // Multiplying by -1 to invert up/down so that mouse up produces the expected pitch.
            float targetPitchDiff = -Vector3.SignedAngle(transform.forward, desiredDirection, transform.right);

            // Map these differences to target velocities and apply mouse sensitivity (using 45° for full input).
            float targetYawVelocity = (targetYawDiff / 45f) * maxTurnSpeed * mouseSensitivity;
            float targetPitchVelocity = (targetPitchDiff / 45f) * maxPitchSpeed * mouseSensitivity;

            // Smooth out the rotational input for yaw and pitch.
            yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);
            pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);
            
            // Apply rotations, taking speed percentage into account.
            transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * speedPercent;

            // Update rudder angles to simulate control surfaces.
            rudderYaw.localEulerAngles = Vector3.up * (yawVelocity / maxTurnSpeed) * rudderAngle;
            rudderPitch.localEulerAngles = Vector3.left * (pitchVelocity / maxPitchSpeed) * rudderAngle;
        }
        else {
            // Keyboard-based control
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

        // Rotate the propeller and update material transparency based on speed percentage.
        propeller.Rotate(Vector3.forward * Time.deltaTime * propellerSpeedFac * speedPercent, Space.Self);
        propSpinMat.color = new Color(propSpinMat.color.r, propSpinMat.color.g, propSpinMat.color.b, speedPercent * 0.3f);
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
