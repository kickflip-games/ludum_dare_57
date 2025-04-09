using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CrabManager : MonoBehaviour
{
    [Header("Crab Settings")]
    [Tooltip("Prefab for the crab model.")]
    public GameObject crabPrefab;
    [Tooltip("Initial spawn area within the walkable region.")]
    public Vector2 spawnArea = new Vector2(2f, 2f);

    [Header("Walkable Region Settings")]
    [Tooltip("Overall walkable region (centered on this object).")]
    public Vector2 walkableRegion = new Vector2(10f, 10f);

    [Header("DOTween Settings")]
    [Tooltip("Duration (seconds) to perform each movement.")]
    public float moveDuration = 3f;
    [Tooltip("Duration (seconds) for the rotation tween.")]
    public float rotationDuration = 0.5f;
    [Tooltip("Time (seconds) to wait at each endpoint.")]
    public float waitTimeAtEndpoint = 1f;
    
    [Header("Cycle Settings")]
    [Tooltip("Probability (0..1) that the crab will backtrack (go to target and return) rather than update its start.")]
    public float backtrackingChance = 0.5f;

    [Header("Debug Settings")]
    [Tooltip("Draw a gizmo line for the current trajectory.")]
    public bool drawGizmoTrajectory = true;

    // Internal references.
    private GameObject crab;
    private Vector3 currentStart;   // The current reference point (spawn or last target in non-backtracking mode).
    private Vector3 currentTarget;  // The destination for the current cycle.

    // Cached walkable region boundaries (centered on this manager's position).
    private float minX, maxX, minZ, maxZ;

    private void Start()
    {
        // Define the walkable region bounds.
        float halfWidth = walkableRegion.x / 2f;
        float halfHeight = walkableRegion.y / 2f;
        minX = transform.position.x - halfWidth;
        maxX = transform.position.x + halfWidth;
        minZ = transform.position.z - halfHeight;
        maxZ = transform.position.z + halfHeight;

        SpawnCrab();
        // Start the movement cycle.
        StartCoroutine(CrabCycleRoutine());
    }

    /// <summary>
    /// Spawns a single crab within the spawn area.
    /// </summary>
    void SpawnCrab()
    {
        Vector3 offset = new Vector3(
            Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f),
            0,
            Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f)
        );
        // Place the crab relative to the manager's position.
        Vector3 spawnPos = transform.position + offset;
        crab = Instantiate(crabPrefab, spawnPos, Quaternion.identity, transform);
        currentStart = spawnPos;
    }

    /// <summary>
    /// Repeatedly performs movement cycles. In each cycle,
    /// the crab either backtracks (goes to a target and then returns) or moves on to a new target.
    /// </summary>
    IEnumerator CrabCycleRoutine()
    {
        while (true)
        {
            // Pick a new target destination within the overall walkable region.
            currentTarget = PickRandomDestination();
            
            // Compute the movement direction from currentStart to currentTarget.
            Vector3 moveDir = (currentTarget - currentStart).normalized;
            // Rotate the crab so that its left side faces the movement direction.
            // Since normally LookRotation gives the forward direction, multiplying by Euler(0,90,0)
            // will offset the rotation so that the crab’s left side (−right) points in moveDir.
            Quaternion desiredRotation = Quaternion.LookRotation(moveDir) * Quaternion.Euler(0, 90, 0);

            // Create a DOTween sequence for the movement.
            Sequence seq = DOTween.Sequence();
            // Rotate the crab (once).
            seq.Append(crab.transform.DORotateQuaternion(desiredRotation, rotationDuration));
            // Move from currentStart to currentTarget.
            seq.Append(crab.transform.DOMove(currentTarget, moveDuration));
            seq.AppendInterval(waitTimeAtEndpoint);

            bool backtracking = Random.value < backtrackingChance;
            if (backtracking)
            {
                // Backtracking mode: move back to the original start.
                seq.Append(crab.transform.DOMove(currentStart, moveDuration));
                seq.AppendInterval(waitTimeAtEndpoint);
            }
            // Wait for the sequence to complete.
            yield return seq.WaitForCompletion();

            // If not backtracking, update currentStart to be the new position.
            if (!backtracking)
            {
                currentStart = currentTarget;
            }

            // Optionally wait a moment before starting the next cycle.
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Returns a random destination within the walkable region.
    /// </summary>
    Vector3 PickRandomDestination()
    {
        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        return new Vector3(x, currentStart.y, z);
    }

    /// <summary>
    /// Draws the walkable region and the current trajectory (from currentStart to currentTarget) for debugging.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw the overall walkable region.
        Gizmos.color = new Color(255,0,0,0.5f);


        Gizmos.DrawCube(transform.position, new Vector3(walkableRegion.x,0.01f, walkableRegion.y));

        // If playing, draw the current trajectory.
        if (Application.isPlaying && drawGizmoTrajectory)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(currentStart, currentTarget);
        }
    }
}
