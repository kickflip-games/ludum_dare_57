using System.Collections;
using UnityEngine;

public class Crab : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2.0f;
    public float rotationSpeed = 60.0f;
    public float waitTimeAtDestination = 1.0f;
    public Vector2 walkingBounds = new Vector2(1, 1);

    [Header("Back-and-Forth Settings")]
    public int minCycles = 2;
    public int maxCycles = 3;

    [Header("Debug Settings")]
    public bool drawBoundsGizmo = true;

    private Vector3 startPoint;
    private Vector3 targetPoint;
    private Transform model;

    private void Start()
    {
        // Use the first child if available; otherwise, use this transform.
        model = transform.childCount > 0 ? transform.GetChild(0) : transform;
        startPoint = model.position;
        StartCoroutine(CrabMovementLoop());
    }

    /// <summary>
    /// Main loop: Pick a new target, perform one rotation (so that the crab's left side is aligned with travel direction),
    /// then do several back-and-forth movements: walk sideways to the target, pause, walk sideways back to start, pause.
    /// </summary>
    IEnumerator CrabMovementLoop()
    {
        while (true)
        {
            // Choose a random destination relative to the start.
            PickRandomDestination();

            // Calculate movement direction from start to target.
            Vector3 moveDirection = (targetPoint - startPoint).normalized;
            // Set desiredRotation such that the crab's left vector becomes equal to moveDirection.
            // Multiplying by a 90° rotation rotates the forward direction so the left side faces moveDirection.
            Quaternion desiredRotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, 90, 0);

            // Rotate once to align the crab's left side with moveDirection.
            yield return StartCoroutine(RotateTo(desiredRotation));

            // Determine number of cycles (back-and-forth) to perform.
            int cycles = Random.Range(minCycles, maxCycles + 1);
            for (int i = 0; i < cycles; i++)
            {
                // Move sideways from start to target along the local left direction.
                yield return StartCoroutine(SidewaysMoveToPoint(targetPoint, Vector3.left));
                yield return new WaitForSeconds(waitTimeAtDestination);

                // Return sideways from target back to start along the local right direction.
                yield return StartCoroutine(SidewaysMoveToPoint(startPoint, Vector3.right));
                yield return new WaitForSeconds(waitTimeAtDestination);
            }
        }
    }

    /// <summary>
    /// Rotates the model smoothly to the target rotation.
    /// </summary>
    IEnumerator RotateTo(Quaternion targetRotation)
    {
        while (Quaternion.Angle(model.rotation, targetRotation) > 1f)
        {
            model.rotation = Quaternion.RotateTowards(model.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
        model.rotation = targetRotation;
    }

    /// <summary>
    /// Moves the model sideways (in its local space) until it reaches the specified destination.
    /// The localDirection should be Vector3.left when moving from start to target, 
    /// and Vector3.right when returning from target to start.
    /// </summary>
    IEnumerator SidewaysMoveToPoint(Vector3 destination, Vector3 localDirection)
    {
        while (Vector3.Distance(model.position, destination) > 0.5f)
        {
            // Transform the local direction into world space.
            Vector3 worldDirection = model.TransformDirection(localDirection);
            model.position += worldDirection * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Picks a random target destination within the walking bounds relative to the start point.
    /// </summary>
    void PickRandomDestination()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-walkingBounds.x / 2, walkingBounds.x / 2),
            0,
            Random.Range(-walkingBounds.y / 2, walkingBounds.y / 2)
        );
        targetPoint = startPoint + randomOffset;
    }


    private void OnDrawGizmos()
    {
        // Draw the walking bounds.
        if (drawBoundsGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(walkingBounds.x, 0.1f, walkingBounds.y));
        }

        // When playing, draw a line and spheres for the start and target points.
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint, targetPoint);
            Gizmos.DrawSphere(startPoint, 0.1f);
            Gizmos.DrawSphere(targetPoint, 0.1f);
        }
    }
}
