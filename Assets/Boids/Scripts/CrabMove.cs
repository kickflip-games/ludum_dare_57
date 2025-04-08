using System.Collections;
using UnityEngine;

public class CrabMovement : MonoBehaviour {
    // Speed at which the crab moves.
    public float moveSpeed = 2.0f;
    // Speed of rotation (degrees per second).
    public float rotationSpeed = 60.0f;
    // How long the crab waits at each destination before picking another.
    public float waitTimeAtDestination = 1.0f;
    
    // The allowed area for wandering (typically set by the spawner).
    // Expecting this Collider to be from one of the chunks.
    public Collider allowedArea;

    // The current destination point within the allowed area.
    private Vector3 targetPoint;
    // Flag to prevent multiple destination changes at the same time.
    private bool isWaiting = false;

    void Start() {
        // Pick an initial destination if an allowed area is defined.
        if (allowedArea != null) {
            PickRandomDestination();
        }
    }

    void Update() {
        if (allowedArea != null) {
            // Determine the direction to the target destination.
            Vector3 direction = (targetPoint - transform.position).normalized;

            // Smoothly rotate toward the destination.
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction,
                                                         rotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

            // Move forward.
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            // If we are close to the target destination and not already waiting, pause then choose a new destination.
            if (!isWaiting && Vector3.Distance(transform.position, targetPoint) < 0.5f) {
                StartCoroutine(WaitAndPickNewDestination());
            }
        }
        else {
            // Optional fallback: if no allowed area is defined,
            // simply move forward with no destination constraints.
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

    IEnumerator WaitAndPickNewDestination() {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtDestination);
        PickRandomDestination();
        isWaiting = false;
    }

    void PickRandomDestination() {
        if (allowedArea != null) {
            // Get the bounds of the allowed area (chunk).
            Vector3 min = allowedArea.bounds.min;
            Vector3 max = allowedArea.bounds.max;
            // Choose a random position within the XZ bounds.
            float randomX = Random.Range(min.x, max.x);
            float randomZ = Random.Range(min.z, max.z);
            // Use the center Y of the bounds to stay on the surface.
            float randomY = allowedArea.bounds.center.y;
            targetPoint = new Vector3(randomX, randomY, randomZ);
        }
    }
}
