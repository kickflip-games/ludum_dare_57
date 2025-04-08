using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class Fish : MonoBehaviour
{
    // The central point around which the fish will swim

    public Transform centerPoint;
    public float radius; // The radius of the circular path
    public float speed = 1f; // The speed of the fish
    public float wiggleSpeed = 5f; // The speed of the wiggle
    public float wiggleAmount = 15f; // The amount of the wiggle

    private float angle;

    void Start()
    {
        // Initialize the angle based on the fish's starting position
        angle = Random.Range(0f, 2 * Mathf.PI);
        // radiuss == distance from the center point
        radius = Vector3.Distance(transform.position, centerPoint.position);
        
    }
    
    
    void Update()
    {
        // Update the angle based on the speed and time
        angle += speed * Time.deltaTime;

        // Calculate the new position of the fish
        float x = centerPoint.position.x + Mathf.Cos(angle) * radius;
        float z = centerPoint.position.z + Mathf.Sin(angle) * radius;

        // Update the fish's position
        transform.position = new Vector3(x, transform.position.y, z);

        // Make the fish face the direction it is moving
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        transform.forward = direction;

        // Add wiggling effect
        float wiggle = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount;
        transform.Rotate(0, wiggle, 0);
    }
}