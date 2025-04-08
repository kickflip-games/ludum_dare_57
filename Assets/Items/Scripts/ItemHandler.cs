using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemHandler : MonoBehaviour {
    
    
    // The item prefab to spawn.
    public GameObject itemPrefab;
    public Vector3 spawnBounds = new Vector3(10, 0, 10);
    public static event Action<int> OnItemSpawned;
    public int totalSpawnedItems = 0;
    
    // The radius used to check for collisions. This should roughly match your item's size.
    public float checkRadius = 0.5f;
    // Layers to check for colliders (set this in the Inspector to avoid unwanted layers).
    public LayerMask collisionLayers;
    
    // Maximum attempts to find a valid spawn location.
    public int maxAttempts = 10;
    
    
    public int itemsToSpawn = 10;
    
    public List<GameObject> spawnedItems = new List<GameObject>();

    // Example of spawning an item at Start.
    void Start() {
        for (int i = 0; i < itemsToSpawn; i++)
        {
            GameObject o = SpawnItem();
            if (o != null)
            {
                spawnedItems.Add(o);
            }
        }
        // print the number of spawned items
        totalSpawnedItems = spawnedItems.Count;
        Debug.Log("Spawned " + totalSpawnedItems + " items.");
        OnItemSpawned?.Invoke(totalSpawnedItems);
    }

    // Tries to spawn an item in a random position within the spawn radius,
    // ensuring it doesn't overlap any colliders.
    public GameObject SpawnItem() {
        for (int i = 0; i < maxAttempts; i++) {
            // Pick a random position within the Cube(transform.position, spawnBounds); 
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-spawnBounds.x / 2, spawnBounds.x / 2),
                Random.Range(-spawnBounds.y / 2, spawnBounds.y / 2),
                Random.Range(-spawnBounds.z / 2, spawnBounds.z / 2)
            );
            
            // Check if this position is free of any colliders in the specified layers.
            Collider[] hitColliders = Physics.OverlapSphere(randomPos, checkRadius, collisionLayers);
            
            if (hitColliders.Length == 0) {
                // If there are no colliders, instantiate the item.
                return Instantiate(itemPrefab, randomPos, Quaternion.identity);
                
            }
        }
        // If we exit the loop, no valid position was found.
        Debug.LogWarning("Could not find a valid spawn location without colliders after " + maxAttempts + " attempts.");
        return null;
    }
    
    // OnGizmos draw a bounds around the region to sspawn
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spawnBounds);
        
    }
}