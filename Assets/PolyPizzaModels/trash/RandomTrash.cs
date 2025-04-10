using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RandomTrash : MonoBehaviour
{
    
    public List<GameObject> trashObjects; // List of trash objects to choose from

    
    
    void Start()
    {
        // Check if the list is not empty
        if (trashObjects.Count > 0)
        {
            // Randomly select a trash object from the list
            int randomIndex = Random.Range(0, trashObjects.Count);
            GameObject selectedTrash = trashObjects[randomIndex];
            Instantiate(selectedTrash, transform);
            
        }
        else
        {
            Debug.LogWarning("No trash objects available to spawn.");
        }
        

    }
    
    

    
    
    
}
