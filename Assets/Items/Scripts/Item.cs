using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public static event Action OnItemCollected;

    // Flag to indicate whether this item was collected
    [SerializeField]
    private bool isCollected = false;
    
    

    // The target submarine that this item should follow
    private Transform targetSubmarine;
    private Fish fish;

    // The offset relative to the submarine’s position (adjust as needed)
    public Vector3 followOffset = new Vector3(0, 0, -2);
    // How fast the item moves toward the target position
    public float followSpeed = 5f;

    // (Optional) Cache Rigidbody and Collider components for disabling physics after pickup.
    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        fish = GetComponentInChildren<Fish>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Print out the collider's name for debugging.
        Debug.Log("Item trigger: Collider: " + other.gameObject.name);
        
        // Check if the colliding object is the player (submarine).
        if (other.CompareTag("Player"))
        {
            // Get the Submarine component from the colliding object’s parent.
            Submarine player = other.GetComponentInParent<Submarine>();
            // Increase player's score.
            player.IncreaseScore();

            // Fire any subscribed events.
            OnItemCollected?.Invoke();
            
            // Instead of destroying the item, start following the submarine.
            StartFollowing(player.transform);
        }
    }

    /// <summary>
    /// Begins the following behavior so that this item smoothly follows the given submarine.
    /// </summary>
    /// <param name="submarineTransform">Transform of the submarine.</param>
    public void StartFollowing(Transform submarineTransform)
    {
        if (!isCollected)
        {
            isCollected = true;
            targetSubmarine = submarineTransform;
            fish.target = targetSubmarine;
            fish.transform.parent = null;
            
            
            
            
            // Optionally, disable physics and colliders so the item no longer interacts with the world.
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            if (col != null)
            {
                col.enabled = false;
            }

            // Destroy this
            Destroy(this.gameObject);
            
        }
    }

    private void Update()
    {
        // If the item was collected and a target has been set, smoothly move the item to its follow position.
        if (isCollected && targetSubmarine != null)
        {
            // Determine the desired position relative to the submarine.
            Vector3 desiredPosition = targetSubmarine.position + targetSubmarine.TransformDirection(followOffset);
            // Smoothly interpolate the item's position toward the desired position.
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

            // Optionally, rotate the item to slowly match the submarine’s rotation.
            transform.rotation = Quaternion.Lerp(transform.rotation, targetSubmarine.rotation, followSpeed * Time.deltaTime);
        }
    }
}
