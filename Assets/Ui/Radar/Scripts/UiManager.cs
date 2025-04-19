using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using TMPro;


public class UiManager : MonoBehaviour
{
    public TMP_Text itemCountText;
    public TMP_Text timeText;


    public GameObject startPanel;
    public GameObject endPanel;
    
    private CanvasGroup _startPanelCanvasGroup;
    private CanvasGroup _endPanelCanvasGroup;
    
    
    public ProgressBar cleanup_progress_bar;

    public GyroOptionSelector optionsPanel;

    [SerializeField] private GameObject _gameUi;
    private CanvasGroup _gameUiCanvasGroup;


    [SerializeField] private GameState _gameState = GameState.START;

    private GameManager _gameManager;
    private Coroutine _timerCoroutine;
    private float _elapsedTime = 0f;
    private int totalItemsToCollect;
    private int itemsCollected = 0;
    private float animDuration = 0.5f;


    public enum GameState
    {
        START,
        RUNNING,
        PAUSED,
        OVER
    }


    private int cleanup_percent
    {
        get
        {
            int v = (int)(((float)itemsCollected / totalItemsToCollect) * 100.0);
            print("cleanup_percent: " + v);
            return Math.Clamp(v, 5, 100);
            ;
        }
    }

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _gameState = GameState.START;

        _startPanelCanvasGroup = startPanel.GetComponent<CanvasGroup>();
        _gameUiCanvasGroup = _gameUi.GetComponent<CanvasGroup>();
        _endPanelCanvasGroup = endPanel.GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        ItemHandler.OnItemSpawned += InitializeItemCount;
        Item.OnItemCollected += IncrementItemCount;

        if (_gameManager != null)
        {
            GameManager.OnGameStart += SwitchToRunningGameUi;
            GameManager.OnGameOver += SwitchToEndGameUi;
        }
    }

    private void OnDisable()
    {
        ItemHandler.OnItemSpawned -= InitializeItemCount;
        Item.OnItemCollected -= IncrementItemCount;
        if (_gameManager != null)
        {
            GameManager.OnGameStart -= SwitchToRunningGameUi;
            GameManager.OnGameOver -= SwitchToEndGameUi;
        }

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }
    }

    private void SwitchToRunningGameUi()
    {
        SetGameStateUi(GameState.RUNNING);
    }

    private void SwitchToEndGameUi()
    {
        SetGameStateUi(GameState.OVER);
    }


    private void Start()
    {


        cleanup_progress_bar.BarValue = 5;
        if (_gameManager != null)
        {
            SetGameStateUi(GameState.START);
        }
        else
        {
            SetGameStateUi(GameState.RUNNING);
        }
    }


    public void SetGameStateUi(GameState state)
    {
        if (state == GameState.START)
        {
            _gameState = GameState.START;
            _gameUiCanvasGroup.alpha = 0;

            startPanel.SetActive(true);
            _startPanelCanvasGroup.alpha = 1;
            
            endPanel.SetActive(false);
            _endPanelCanvasGroup.alpha = 0;

            SetPauseMenu(false);
        }
        else if (state == GameState.RUNNING)
        {
            _gameState = GameState.RUNNING;
            _elapsedTime = 0f;
            _timerCoroutine = StartCoroutine(TimerCoroutine());

            _gameUi.SetActive(true);
            _gameUiCanvasGroup.alpha = 0;
            _gameUi.transform.localScale = Vector3.zero;
            SetPauseMenu(false);

            var sequence = DOTween.Sequence();
            sequence.Join(_gameUiCanvasGroup.DOFade(1, animDuration))
                .Join(_startPanelCanvasGroup.DOFade(0, animDuration))
                .Join(_gameUi.transform.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBounce));
            sequence.OnComplete(() =>
            {
                startPanel.SetActive(false);
                _gameUi.transform.localScale = Vector3.one;
            });
        }
        else if (state == GameState.PAUSED)
        {
            _gameState = GameState.PAUSED;
            
            var sequence = DOTween.Sequence();
            sequence.Join(_gameUiCanvasGroup.DOFade(0, animDuration))
                .Join(_gameUi.transform.DOScale(Vector3.zero, animDuration).SetEase(Ease.OutBounce));
            sequence.OnComplete(() => _gameUi.SetActive(false));
            SetPauseMenu(true);
        }
        else if (state == GameState.OVER)
        {
            StopTimer();
            
            
            _gameState = GameState.OVER;
            SetPauseMenu(false);
            _startPanelCanvasGroup.alpha = 0;
            startPanel.SetActive(false);
            
            endPanel.SetActive(true);
            _endPanelCanvasGroup.alpha = 0;
            
            var sequence = DOTween.Sequence();
            sequence.Join(_gameUiCanvasGroup.DOFade(0, animDuration))
                .Join(_gameUi.transform.DOScale(Vector3.zero, animDuration).SetEase(Ease.OutBounce))
                .Join(_endPanelCanvasGroup.DOFade(1, animDuration));
            sequence.OnComplete(() => { _gameUi.SetActive(false); }
            );
        }
    }

    public void InitializeItemCount(int totalItems)
    {
        totalItemsToCollect = totalItems;
        UpdateItemCountUI();
        cleanup_progress_bar.BarValue = 5;
        ColorChanger.Instance.SetDirtPercentage(100.0f-cleanup_percent);
    }

    public void IncrementItemCount()
    {
        itemsCollected++;
        UpdateItemCountUI();
        cleanup_progress_bar.BarValue = cleanup_percent;
        ColorChanger.Instance.SetDirtPercentage(100.0f-cleanup_percent);
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


    public void ShowOptions()
    {
        if (optionsPanel.optionsPanel.activeSelf)
            SetGameStateUi(GameState.RUNNING);
        else
            SetGameStateUi(GameState.PAUSED);
    }

    public void SetPauseMenu(bool show)
    {
        optionsPanel.optionsPanel.SetActive(show);
        optionsPanel.ToggleMenu(show);
    }
    
    
    public void RestartGame()
    {
        // Restart the game or reload the scene
        // This is a placeholder; implement your own restart logic
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    
}