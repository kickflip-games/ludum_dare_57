using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class TrashSpawner : MonoBehaviour
{
    [Header("Spawning")] [Tooltip("How often to spawn trash items (in seconds).")] [Range(0.1f, 10f)]
    public float spawnRate = 1f;

    [Tooltip("Radius around the spawner where trash will appear.")] [Range(0.1f, 20f)]
    public float spawnRadius = 0.2f;


    [Header("Initial Movement")] [Tooltip("Minimum initial movement speed.")] [Range(0f, 10f)]
    public float minMoveSpeed = 0.1f;

    [Tooltip("Maximum initial movement speed.")] [Range(0f, 10f)]
    public float maxMoveSpeed = 0.3f;

    [Tooltip("Duration of the initial movement animation (in seconds).")] [Range(0.1f, 10f)]
    public float moveDuration = 3f;

    [Header("Scaling")] [Tooltip("Minimum initial scale of the spawned trash.")] [Range(0.1f, 5f)]
    public float minScale = 0.3f;

    [Tooltip("Maximum initial scale of the spawned trash.")] [Range(0.1f, 5f)]
    public float maxScale = 0.8f;

    [Header("Disappearance")] [Tooltip("Delay before the shrinking animation starts (in seconds).")] [Range(0f, 5f)]
    public float shrinkStartDelay = 0.5f;

    [Tooltip("Duration of the shrinking animation (in seconds).")] [Range(0.1f, 10f)]
    public float shrinkDuration = 3f;


    [Header("Prefab")] [Tooltip("The GameObject prefab to spawn as trash.")]
    public List<GameObject> trashPrefabs;


    private float nextSpawnTime = 0f;

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnTrash();
            nextSpawnTime = Time.time + 1f / spawnRate;
        }
    }

    void SpawnTrash()
    {
        if (trashPrefabs == null)
        {
            Debug.LogError("Trash Prefab is not assigned in the TrashSpawner!");
            return;
        }


        // Generate a random position within the spawn radius
        Vector3 randomSpawnOffset = Random.insideUnitSphere * spawnRadius;
        Vector3 spawnPosition = transform.position + randomSpawnOffset;

        // Instantiate the trash object
        int randomIndex = Random.Range(0, trashPrefabs.Count);
        GameObject selectedPrefab = trashPrefabs[randomIndex];
        GameObject go = Instantiate(selectedPrefab, spawnPosition, Random.rotation);
        // go.transform.SetParent(transform); // Set the parent to the spawner
        RandomiseAndAnimate(go);
    }


    void RandomiseAndAnimate(GameObject go)
    {
        // Apply a random initial scale
        float randomScale = Random.Range(minScale, maxScale);
        go.transform.localScale = Vector3.one * randomScale;

        // Apply random rotation 
        go.transform.rotation = Random.rotation;

        // Apply a random initial movement and rotation using DOTween
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        float randomSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        Vector3 targetPosition = go.transform.position + randomDirection * randomSpeed * moveDuration;

        go.transform.DORotate(new Vector3(0, 360, 0), moveDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);

        go.transform.DOMove(targetPosition, moveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Optional: What happens after the initial movement?
            });

        // Start the self-destruct sequence using DOTween for scaling
        DOVirtual.DelayedCall(shrinkStartDelay, () => ShrinkAndDisappear(go));
    }

    void ShrinkAndDisappear(GameObject trashObject)
    {
        if (trashObject == null) return;

        trashObject.transform.DOScale(Vector3.zero, shrinkDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => { Destroy(trashObject); });
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}