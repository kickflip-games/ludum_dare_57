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
    
    public int score;
    
    Vector3 velocity;
    float yawVelocity;
    float pitchVelocity;
    float currentSpeed;
    public Material propSpinMat;
    
    public bool isPaused = false;

    Renderer[] renderers;
    
    void Start () {
        currentSpeed = maxSpeed;
        renderers = GetComponentsInChildren<Renderer> ();
    }

    void Update () {
        
        
        if (isPaused) {
            return;
        }
        
        
        float accelDir = 0;
        if (Input.GetKey (KeyCode.Q)) {
            accelDir -= 1;
        }
        if (Input.GetKey (KeyCode.E)) {
            accelDir += 1;
        }

        currentSpeed += acceleration * Time.deltaTime * accelDir;
        currentSpeed = Mathf.Clamp (currentSpeed, 0, maxSpeed);
        float speedPercent = currentSpeed / maxSpeed;

        Vector3 targetVelocity = transform.forward * currentSpeed;
        velocity = Vector3.Lerp (velocity, targetVelocity, Time.deltaTime * smoothSpeed);

        float targetPitchVelocity = Input.GetAxisRaw ("Vertical") * maxPitchSpeed;
        pitchVelocity = Mathf.Lerp (pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);

        float targetYawVelocity = Input.GetAxisRaw ("Horizontal") * maxTurnSpeed;
        yawVelocity = Mathf.Lerp (yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);
        transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * speedPercent;
        transform.Translate (transform.forward * currentSpeed * Time.deltaTime, Space.World);

        rudderYaw.localEulerAngles = Vector3.up * yawVelocity / maxTurnSpeed * rudderAngle;
        rudderPitch.localEulerAngles = Vector3.left * pitchVelocity / maxPitchSpeed * rudderAngle;

        propeller.Rotate (Vector3.forward * Time.deltaTime * propellerSpeedFac * speedPercent, Space.Self);
        propSpinMat.color = new Color (propSpinMat.color.r, propSpinMat.color.g, propSpinMat.color.b, speedPercent * .3f);

    }
    
    
    
    

    public void TakeDamage () {
        // Flash all renderers red for 0.5 seconds
        StartCoroutine (FlashRenderers ());
        
        // Move submarine back a few meters and freeze for 0.5 seconds
        Vector3 back = transform.position - transform.forward * 2;
        transform.position = back;
        
    }
    
    IEnumerator FlashRenderers () {
        // flash renderes on and off (ennabel and disable) for 0.5 seconds
        for (int i = 0; i < 10; i++) {
            foreach (var renderer in renderers) {
                renderer.enabled = !renderer.enabled;
            }
            yield return new WaitForSeconds (0.05f);
        }
        
        
    }

    public void IncreaseScore()
    {
        score++;
        print("Score: " + score);
    }
    
    
    public void  PauseMovement(bool pause)
    {
        isPaused = pause;
    }
    
    
}
