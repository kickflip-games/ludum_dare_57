using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    
    
    public static event Action OnItemCollected;

    private void OnTriggerEnter(Collider other)
    {
        
        // Print collider 
        Debug.Log("Item trigger: Collider: " + other.gameObject.name);
        
        // if collider compareTag --> Player
        if (other.transform.tag == "Player")
        {
            // Call pickedUp method
            pickedUp();
            // Get Player Script
            Submarine player = other.GetComponentInParent<Submarine>();
            // Increase player score
            player.IncreaseScore();
            
            OnItemCollected?.Invoke();
            
        }
    }

    public void pickedUp()
    {
        // Destroy the item
        Destroy(gameObject);
    }
    
}