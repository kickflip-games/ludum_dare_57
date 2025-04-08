using System.Collections;
using UnityEngine;

public class CrabSpawner : MonoBehaviour {
    // The crab prefab to spawn.
    public GameObject crabPrefab;

    // How often to spawn a crab (in seconds).
    public float spawnInterval = 3f;

    // Reference to the parent object that holds your chunk GameObjects.
    // Each chunk should have a Collider (for example, BoxCollider) to define its bounds.
    public Transform chunksHolder;

    // Array to hold chunk colliders.
    private Collider[] chunkColliders;

    void Start() {
        if (chunksHolder != null) {
            // Get all Collider components from the children of chunksHolder.
            chunkColliders = chunksHolder.GetComponentsInChildren<Collider>();
        }
        StartCoroutine(SpawnCrabs());
    }

    IEnumerator SpawnCrabs() {
        while (true) {
            if (chunkColliders != null && chunkColliders.Length > 0) {
                // Pick a random chunk from the available ones.
                int randomIndex = Random.Range(0, chunkColliders.Length);
                Collider chosenChunk = chunkColliders[randomIndex];

                // Get the bounds of the chosen chunk.
                Vector3 min = chosenChunk.bounds.min;
                Vector3 max = chosenChunk.bounds.max;

                // Pick a random position on the XZ plane within the bounds.
                float spawnX = Random.Range(min.x, max.x);
                float spawnZ = Random.Range(min.z, max.z);
                // Use the center Y value (assuming the chunk is flat or the crab is intended to spawn at that height).
                float spawnY = chosenChunk.bounds.center.y;
                Vector3 spawnPos = new Vector3(spawnX, spawnY, spawnZ);

                // Instantiate the crab prefab without restricting its movement.
                Instantiate(crabPrefab, spawnPos, Quaternion.identity);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
