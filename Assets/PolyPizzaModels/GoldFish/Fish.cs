using System;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

public class Fish : MonoBehaviour
{
    [SerializeField]
    private bool drawDebug = false;
    
    // Idle settings
    [Header("Idle Settings")]
    public float radius = 3.0f;              // The radius of the circular path
    public float idleSpeed = 1f;             // The speed of the fish while idling

    [SerializeField]
    [Range(0, Mathf.PI * 2)]
    private float angle;
    [SerializeField]
    private Vector3 initPosition = Vector3.zero; // Initial position of the fish
    private Vector3 centerPosition
    {
        get => initPosition + Vector3.forward * radius;
    }

    // Follow settings
    [Header("Follow Settings")]
    public Transform target;

    public float closeRadius = 1.0f; // The target to follow
    public float maxSpeed = 30f;
    public float acceleration = 20f;
    public float decelerationMultiplier = 2f; // How quickly the fish slows down
    public float reTargetInterval = 0.5f;
    private Vector3 targetFollowPosition;
    private float lastRetargetTime;
    private float currentSpeed = 0f;

    // Smoothing parameters
    private Vector3 smoothVelocity = Vector3.zero;
    [SerializeField] private float positionSmoothTime = 0.5f;
    [SerializeField] private float rotationSmoothTime = 0.5f;

    enum FishState
    {
        Idle,
        Following,
    }

    private FishState currentState = FishState.Idle;

    void Start()
    {
        angle = Random.Range(0f, 2 * Mathf.PI);
        initPosition = transform.position;
    }

    void Update()
    {
        if (currentState == FishState.Following)
            FollowingState();
        else if (currentState == FishState.Idle)
            IdleState();

        UpdateState();
    }

    void UpdateState()
    {
        if (currentState == FishState.Idle && target != null)
        {
            PickNewFollowPosition();
            currentState = FishState.Following;
        }
        else if (currentState == FishState.Following && target == null)
        {
            currentState = FishState.Idle;
        }
    }

    void IdleState()
    {
        // Update the angle based on speed and time, wrap around when exceeding 2*PI
        angle += idleSpeed * Time.deltaTime;
        if (angle > 2 * Mathf.PI)
            angle -= 2 * Mathf.PI;

        // Update the fish's idle circular position
        transform.position = new Vector3(
            centerPosition.x + Mathf.Cos(angle) * radius,
            transform.position.y,
            centerPosition.z + Mathf.Sin(angle) * radius
        );

        // Face the direction of movement
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        // slerp the rotation to face the direction of movement
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
        
    }

    void FollowingState()
    {
        if (target == null)
            return;

        // Compute the distance to the target (i.e. the object being followed)
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Check if the fish is within the closeRadius of the target.
        if (distanceToTarget <= closeRadius)
        {
            // When close to the target, stop picking new follow positions.
            // Also, set the follow position to the target's position,
            // so that we decelerate smoothly.
            targetFollowPosition = target.position;
        }
        else
        {
            // If the fish is outside the closeRadius, pick a new follow position every reTargetInterval.
            if (Time.time - lastRetargetTime >= reTargetInterval)
            {
                PickNewFollowPosition();
                lastRetargetTime = Time.time;
            }
        }

        // Determine the vector toward our target follow position.
        Vector3 toTarget = targetFollowPosition - transform.position;
        Vector3 directionToTarget = toTarget.normalized;

        // Set the desired speed: if outside the closeRadius, accelerate up to maxSpeed; else decelerate (target speed 0).
        float desiredSpeed = (distanceToTarget > closeRadius) ? maxSpeed : 0f;

        // Smooth acceleration or deceleration.
        if (currentSpeed < desiredSpeed)
        {
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        }
        else if (currentSpeed > desiredSpeed)
        {
            currentSpeed = Mathf.Max(currentSpeed - acceleration * decelerationMultiplier * Time.deltaTime, desiredSpeed);
        }

        // Calculate a target position based on the current movement direction.
        Vector3 targetPosition = transform.position + directionToTarget * currentSpeed * Time.deltaTime;
        // Use SmoothDamp for smooth positional interpolation.
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref smoothVelocity, positionSmoothTime);

        // Smoothly rotate the fish to face the target follow position. + 90 deg
        Quaternion targetRotation = Quaternion.LookRotation(target.forward);
        targetRotation *= Quaternion.Euler(0, 90, 0);
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
    
        
    }

    void PickNewFollowPosition()
    {
        // Pick a random direction around the target.
        Vector3 randomDirection = Random.insideUnitSphere;
        // Ensure the new position is not directly in front of the target.
        if (Vector3.Dot(randomDirection.normalized, target.forward) > 0.75f)
        {
            randomDirection = Quaternion.AngleAxis(180f, Vector3.up) * randomDirection; // Flip the direction.
        }

        // Calculate a new position relative to the target within a range defined by followDistance.
        targetFollowPosition = target.position + randomDirection * Random.Range(closeRadius * 0.5f, closeRadius * 1.5f);
        // Clamp the Y-axis if you want the fish to stay at the same height.
        targetFollowPosition.y = target.position.y;
    }
    
    private void OnDrawGizmos()
    {
        if (!drawDebug)
            return;
        
        if (currentState == FishState.Idle)
        {
            Gizmos.color = Color.green;
            DebugExtension.DebugCircle(centerPosition, Vector3.up, Color.green, radius);
            Gizmos.DrawWireSphere(centerPosition, radius);
        }
        else if (currentState == FishState.Following)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetFollowPosition);
            
            // draw radius around the targetFollowPosition
            Gizmos.color = Color.red;
            DebugExtension.DebugCircle(target.position, Vector3.up, Color.red, closeRadius);
            
        }
    }
}
