using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    public TMP_Text itemCountText;
    public TMP_Text timeText;


    public GameObject messagePanel;
    private CanvasGroup _messagePanelCanvasGroup;
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
            int v = (int)(((float)itemsCollected / totalItemsToCollect) * 100.0);
            print("cleanup_percent: " + v);
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
        _messagePanelCanvasGroup = messagePanel.GetComponent<CanvasGroup>();
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
        // messagePanel.SetActive(true);
        // messageText.text = "Collect Fishies!\nClick to start the game";
        
        AnimateMessagePanel(
            "Clear Trash!\nClick to start the game",
            0.1f,
            false
        );
    }

    // Combined Stop Timer and Show End Panel for the GameManager event
    private void StopTimerAndShowEndPanel()
    {
        StopTimer();
        // messagePanel.SetActive(true);
        // messageText.text = "All fishies collected in " + timeText.text + "!\nClick to restart";
        String txt = "All clean! You took " + timeText.text + "!\nClick to restart";
        AnimateMessagePanel(
            txt,
            1.0f,
            false
        );


    }

    public void AnimateMessagePanel(string txt, float duration, bool hide)
    {
        if (!hide)
        {
            _messagePanelCanvasGroup.alpha = 0;
            messagePanel.SetActive(true);
        }
        
        
        // DoTween to animate the message panel 
        float targetAlpha = hide ? 0 : 0.5f;
        _messagePanelCanvasGroup.DOFade(targetAlpha, duration).OnComplete(() =>
        {
            messagePanel.SetActive(!hide);
        });
        
        // Pulsate the message text while message panel is active 
        if (!hide)
        {
            messageText.text = txt;
            messageText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), duration, 1, 0);
        }
        else
        {
            messageText.transform.localScale = Vector3.one;
            messageText.text = "";
        }
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
        
        // Hide the start panel
        AnimateMessagePanel(
            "",
            0.1f,
            true
        );
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