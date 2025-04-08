using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Radar : MonoBehaviour
{
    [SerializeField] private Transform pfRadarPing;
    private Transform sweepTransform;
    private float rotationSpeed;
    private float radarDistance;
    private Dictionary<GameObject, GameObject> radarPings = new Dictionary<GameObject, GameObject>();


    private ItemHandler _itemHandler;


    private Vector2 GetVectorFromAngle(float angle)
    {
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private void Start()
    {
        // Get a reference to your ItemHandler in the scene.
        _itemHandler = FindObjectOfType<ItemHandler>();

        if (_itemHandler == null)
        {
            Debug.LogError("ItemHandler not found in scene!");
        }
    }

    private void Awake()
    {
        sweepTransform = transform.Find("Sweep");
        rotationSpeed = 180f;
        radarDistance = 150f;
        
    }

    private void Update()
    {
        float previousRotation = (sweepTransform.eulerAngles.z % 360) - 180;
        sweepTransform.eulerAngles -= new Vector3(0, 0, rotationSpeed * Time.deltaTime);
        float currentRotation = (sweepTransform.eulerAngles.z % 360) - 180;
        

        foreach (GameObject _item in _itemHandler.spawnedItems)
        {
            if (_item == null)
                continue;
            
            Debug.Log("Spawning item for ", _item);
            
            // Calculate the offset from the radar's position.
            Vector3 offset = _item.transform.position - transform.position;

            // Only consider the X and Z components (projecting onto the XZ plane).
            Vector2 radarPos2D = new Vector3(offset.x, offset.z, transform.position.z);
            RadarPing radarPing = Instantiate(pfRadarPing, radarPos2D, Quaternion.identity).GetComponent<RadarPing>();
            radarPing.transform.parent = transform;
            radarPing.SetColor(Color.green);
            radarPing.SetDisappearTimer(360f / rotationSpeed * 1f);
        }
    }
}