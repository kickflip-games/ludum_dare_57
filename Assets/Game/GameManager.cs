using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
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


    [SerializeField] private CinemachineCamera _startCam;
    [SerializeField] private CinemachineCamera _followCam;
    


    private void Awake()
    {
        _player = FindObjectOfType<Submarine>();
        if (_player == null)
        {
            Debug.LogError("Player not found in the scene.");
        }
    }


    private void Start()
    {
        _player.PauseMovement(true);
        _startCam.Priority = 1;
        _followCam.Priority = 0;
    }

    public Submarine GetPlayer()
    {
        return _player;
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
    
    public void StartGame()
    {
        if (!_isGameComplete)
        {
            _player.PauseMovement(false);
            _isGameRunning = true;
        
            // Set startCam priority to 0 and followCam to 1
            _startCam.Priority = 0;
            _followCam.Priority = 1;
        
            OnGameStart?.Invoke();
        }
        else
        {
            // Restart the game (reload the scene)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    

    
}
