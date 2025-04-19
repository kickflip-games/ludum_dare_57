using DG.Tweening;
using SK.GyroscopeWebGL.Examples;
using UnityEngine;
using TMPro;
using UIAssistant;
using UnityEngine.UIElements;

public class GyroOptionSelector : MonoBehaviour
{

    public SK_GyroscopeTest gyroHandler;

    [SerializeField] private ColoredToggle _toggle;
    
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private CanvasGroup canvasGroup;

    public GameObject optionsPanel;

    [SerializeField] private TMP_Text gyroSupportedTxt;
    
    private Submarine player;
    
    private void Awake()
    {
        player = FindObjectOfType<Submarine>();
        optionsPanel = canvasGroup.gameObject;
    }

    void Start()
    {
        canvasGroup.alpha = 0;
       
        if (gyroHandler.deviceSupportsGyro)
        {
            gyroSupportedTxt.text = "Gyroscope Supported";
            gyroSupportedTxt.color = Color.green;
            _toggle.interactable = true;
        }
        else
        {
            gyroSupportedTxt.text = "Gyroscope Not Supported";
            gyroSupportedTxt.color = Color.red;
            _toggle.interactable = false;
        }
        if (gyroHandler != null)
        {
            _toggle.isOn = gyroHandler.isGyroEnabled;
        }
        else
        {
            _toggle.isOn = false;
        } optionsPanel.SetActive(false);
    }

    
    
    public void ToggleMenu(bool isOpen)
    {
        if (player != null)
        {
            player.isPaused = isOpen;
        }
        Vector3 targetScale = isOpen ? Vector3.one : Vector3.zero;
        
        var sequence = DOTween.Sequence();
        sequence.Join(canvasGroup.DOFade(isOpen ? 1 : 0, 0.5f))
            .Join(rectTransform.DOScale(targetScale, 0.5f).SetEase(Ease.OutBack));
        sequence.OnComplete(() =>
            {
                optionsPanel.SetActive(isOpen);
                rectTransform.localScale = Vector3.one;
            });
    }
    
    
    public void ToggleGyro(bool isOn)
    {
        if (gyroHandler != null)
        {
            gyroHandler.isGyroEnabled = isOn;
        }
    }
    
}