using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    public TMP_Text itemCountText;
    public TMP_Text timeText;


    public GameObject messagePanel;
    public TMP_Text messageText;
    public ProgressBar cleanup_progress_bar;

    private GameManager _gameManager;
    private Coroutine _timerCoroutine;
    private float _elapsedTime = 0f;
    private int totalItemsToCollect;
    private int itemsCollected = 0;

    private int cleanup_percent
    {
        get
        {
            int v = (itemsCollected / totalItemsToCollect) * 100;
            return Math.Clamp(v, 5, 100);;
        }
    }

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    private void OnEnable()
    {
        ItemHandler.OnItemSpawned += InitializeItemCount;
        Item.OnItemCollected += IncrementItemCount;

        if (_gameManager != null)
        {
            GameManager.OnGameStart += StartTimerAndHideStartUi;
            GameManager.OnGameOver += StopTimerAndShowEndPanel;
        }
    }

    private void OnDisable()
    {
        ItemHandler.OnItemSpawned -= InitializeItemCount;
        Item.OnItemCollected -= IncrementItemCount;
        if (_gameManager != null)
        {
            GameManager.OnGameStart -= StartTimerAndHideStartUi;
            GameManager.OnGameOver -= StopTimerAndShowEndPanel;
        }

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }
    }

    private void Start()
    {
        messageText = messagePanel.GetComponentInChildren<TMP_Text>();
        cleanup_progress_bar.BarValue = 5;
        if (_gameManager != null)
        {
            ShowStartPanel();
        }
        else
        {
            StartTimerAndHideStartUi();
        }
    }


    public void ShowStartPanel()
    {
        messagePanel.SetActive(true);
        messageText.text = "Collect Fishies!\nClick to start the game";
    }

    // Combined Stop Timer and Show End Panel for the GameManager event
    private void StopTimerAndShowEndPanel()
    {
        StopTimer();
        messagePanel.SetActive(true);
        messageText.text = "All fishies collected in " + timeText.text + "!\nClick to restart";
    }


    public void InitializeItemCount(int totalItems)
    {
        totalItemsToCollect = totalItems;
        UpdateItemCountUI();
        cleanup_progress_bar.BarValue = 5;
    }

    public void IncrementItemCount()
    {
        itemsCollected++;
        UpdateItemCountUI();
        cleanup_progress_bar.BarValue = cleanup_percent;
    }

    private void UpdateItemCountUI()
    {
        if (itemCountText != null)
        {
            itemCountText.text = $"  {itemsCollected} / {totalItemsToCollect}";
        }
        else
        {
            Debug.LogError("Item count UI Text element not assigned in the Inspector!");
        }
    }

    public void UpdateTimeUI()
    {
        if (timeText != null)
        {
            timeText.text = GetTimeString(_elapsedTime);
        }
        else
        {
            Debug.LogError("Time UI Text element not assigned in the Inspector!");
        }
    }

    public string GetTimeString(float time)
    {
        if (time < 60)
        {
            return $"{(int)time}s";
        }

        return $"{(int)(time / 60)}m {((int)time % 60)}s";
    }

    private void StartTimerAndHideStartUi()
    {
        _elapsedTime = 0f;
        _timerCoroutine = StartCoroutine(TimerCoroutine());
        messagePanel.SetActive(false);
        messageText.text = "";
    }

    private void StopTimer()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    private IEnumerator TimerCoroutine()
    {
        while (true)
        {
            _elapsedTime += Time.deltaTime;
            UpdateTimeUI();
            yield return null; // Wait for the next frame
        }
    }
}