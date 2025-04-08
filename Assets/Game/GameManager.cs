using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    
    
    private int totalItemsToCollect;
    private int itemsCollected = 0;
    
    
    public static event Action OnGameOver;
    public static event Action OnGameStart;
    private Submarine _player;
    
    bool _isGameRunning = false;
    bool _isGameComplete = false;

    
    private void Start()
    {

        _player = FindObjectOfType<Submarine>();
        if (_player == null)
        {
            Debug.LogError("Player not found in the scene.");
        }
        _player.PauseMovement(true);
    }
    
    void Update()
    {
        // if Any key is pressed/mouse clicked/screen touched,  and game is not started
        if (!_isGameRunning)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                // check if game is not complete
                if (!_isGameComplete)
                {
                    StartGame();
                }
                else
                {
                    // Restart the game (reload the scene)
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
    }
    
    
    private void OnEnable()
    {
        ItemHandler.OnItemSpawned += InitializeItemCount;
        Item.OnItemCollected += IncrementItemCount;
    }

    private void OnDisable()
    {
        ItemHandler.OnItemSpawned += InitializeItemCount;
        Item.OnItemCollected -= IncrementItemCount;
    }
    

    public void InitializeItemCount(int totalItems)
    {
        totalItemsToCollect = totalItems;
        itemsCollected = 0; // Reset the count when initializing
    }

    public void IncrementItemCount()
    {
        itemsCollected++;
        if (itemsCollected >= totalItemsToCollect)
        {
            Debug.Log("All items collected!");
            GameOver();
        }
    }


    void GameOver()
    {
        _player.PauseMovement(true);
        OnGameOver?.Invoke();
        StartCoroutine(WaitAndAllowReset());
    }

    private IEnumerator WaitAndAllowReset()
    {
        yield return new WaitForSeconds(3f);
        _isGameRunning = false;
        _isGameComplete = true;
    }

    void StartGame()
    {
        _player.PauseMovement(false);
        _isGameRunning = true;
        OnGameStart?.Invoke();
    }

    
}
