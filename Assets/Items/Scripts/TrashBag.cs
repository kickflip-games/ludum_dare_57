using UnityEngine;

public class TrashBag : MonoBehaviour
{
    // Randomize trash bag direction (small velocity) 
    public float[] speedRange = { 0.1f, 0.5f }; // Speed range for the trash bag
    public float[] rotationSpeedRange = { 0.1f, 1.0f }; // Rotation speed range for the trash bag


    void Start()
    {
        // Randomize the direction of the trash bag
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        Vector3 randomRotation = Random.insideUnitSphere.normalized;

        // Set a small velocity for the trash bag
        float speed = Random.Range(speedRange[0], speedRange[1]);
        float rotationSpeed = Random.Range(rotationSpeedRange[0], rotationSpeedRange[1]);
        
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = randomDirection * speed;
        rb.angularVelocity = randomRotation * rotationSpeed;
    }
}