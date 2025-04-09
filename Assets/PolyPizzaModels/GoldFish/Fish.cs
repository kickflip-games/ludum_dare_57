using System;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;


public class Fish : MonoBehaviour
{
    // The central point around which the fish will swim

    [Header("Wiggle Settings")] 
    public float wiggleTime = 0.5f; // The speed of the wiggle
    public float wiggleAmount = 15f; // The amount of the wiggle


    [Header("Idle Settings")] public Transform centerPoint;
    public float radius; // The radius of the circular path
    public float idleSpeed = 1f; // The speed of the fish
    private float angle;


    // header
    [Header("Follow Settings")] public Transform target;
    public float followDistance = 5f; // Ideal distance to maintain from the target
    public float minDistanceToRecalculate = 2f; // How far the fish needs to be before picking a new spot
    public float maxSpeed = 3f;
    public float acceleration = 1f;
    public float decelerationMultiplier = 2f; // How quickly the fish slows down
    public float reTargetInterval = 0.5f;
    private Vector3 targetFollowPosition;
    private float lastRetargetTime;
    private float currentSpeed = 0f;


    enum FishState
    {
        Idle,
        Following,
    }

    private FishState currentState = FishState.Idle;

    void Start()
    {
        angle = Random.Range(0f, 2 * Mathf.PI);
        if (centerPoint == null)
        {
            Debug.LogError("Center point is not assigned.");
            centerPoint = this.transform; // Default to the fish's own position
        }

        radius = Vector3.Distance(transform.position, centerPoint.position);

        // Wiggle (forever)
        transform.DORotate(new Vector3(0, wiggleAmount, 0), wiggleTime).SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetSpeedBased(true)
            .SetUpdate(true);
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
        // Update the angle based on the speed and time
        angle += idleSpeed * Time.deltaTime;

        // Calculate the new position of the fish
        float x = centerPoint.position.x + Mathf.Cos(angle) * radius;
        float z = centerPoint.position.z + Mathf.Sin(angle) * radius;

        // Update the fish's position
        transform.position = new Vector3(x, transform.position.y, z);

        // Make the fish face the direction it is moving
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        transform.forward = direction;
    }

    void FollowingState()
    {
        if (target == null) return;

        // Check if it's time to pick a new target follow position
        if (Time.time - lastRetargetTime >= reTargetInterval || Vector3.Distance(transform.position, targetFollowPosition) < minDistanceToRecalculate)
        {
            PickNewFollowPosition();
            lastRetargetTime = Time.time;
        }

        // Calculate the direction to the current target follow position
        Vector3 directionToTarget = (targetFollowPosition - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, targetFollowPosition);

        // Calculate desired speed based on distance
        float desiredSpeed = maxSpeed;
        if (distanceToTarget < followDistance * 0.75f) // Start slowing down earlier
        {
            desiredSpeed = Mathf.Lerp(0, maxSpeed, distanceToTarget / (followDistance * 0.75f));
        }
        else if (distanceToTarget > followDistance * 1.25f) // Speed up if too far
        {
            desiredSpeed = maxSpeed;
        }

        // Apply acceleration/deceleration
        if (currentSpeed < desiredSpeed)
        {
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        }
        else if (currentSpeed > desiredSpeed)
        {
            currentSpeed = Mathf.Max(currentSpeed - acceleration * decelerationMultiplier * Time.deltaTime, desiredSpeed);
        }

        // Move towards the target follow position
        transform.position += directionToTarget * currentSpeed * Time.deltaTime;

        // Make the fish face the direction of movement
        if (directionToTarget != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, directionToTarget, Time.deltaTime * 5f); // Smooth turning
        }

    }

    void PickNewFollowPosition()
    {
        // Pick a random direction around the target
        Vector3 randomDirection = Random.insideUnitSphere;
        // Ensure the new position is not in front of the target (you can adjust the dot product threshold)
        if (Vector3.Dot(randomDirection.normalized, target.forward) > 0.5f)
        {
            randomDirection = Quaternion.AngleAxis(180f, Vector3.up) * randomDirection; // Flip the direction
        }

        // Calculate a new target position nearby the target
        targetFollowPosition = target.position + randomDirection * Random.Range(followDistance * 0.5f, followDistance * 1.5f);
        // Optionally, you could clamp the Y-axis to keep fish at a similar height
        targetFollowPosition.y = transform.position.y;
    }
}