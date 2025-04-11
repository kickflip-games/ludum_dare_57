using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    [Header("Title Setting")]
    public string Title;
    public Color TitleColor;
    public TMP_FontAsset TitleFont;
    public int TitleFontSize = 10;

    [Header("Bar Setting")]
    // public Color BarColor;
    public Color BarBackGroundColor;
    public Sprite BarBackGroundSprite;
    public Gradient BarGradient;
    [Range(1f, 100f)]
    public int Alert = 20;
    public Color BarAlertColor;

    [Header("Sound Alert")]
    public AudioClip sound;
    public bool repeat = false;
    public float RepeatRate = 1f;

    private Image bar, barBackground;
    private float nextPlay;
    private AudioSource audiosource;
    private TextMeshProUGUI txtTitle;
    private float barValue;
    private RectTransform rectTransform;
    
    [Header("Animation")]
    float anim_duration = 1.5f;
    

    public float BarValue
    {
        get { return barValue; }
        set
        {
            Debug.Log("BarValue set to: " + value);
            value = Mathf.Clamp(value, 5, 100);
            barValue = value;
            UpdateValue(barValue);
        }
    }
    
    
    

    private void Awake()
    {
        bar = transform.Find("Bar").GetComponent<Image>();
        barBackground = GetComponent<Image>();
        txtTitle = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        barBackground = transform.Find("BarBackground").GetComponent<Image>();
        audiosource = GetComponent<AudioSource>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        txtTitle.text = Title;
        txtTitle.color = TitleColor;
        txtTitle.font = TitleFont;
        txtTitle.fontSize = TitleFontSize;

        bar.color = BarGradient.Evaluate(barValue/100);
        barBackground.color = BarBackGroundColor;
        barBackground.sprite = BarBackGroundSprite;

        UpdateValue(barValue);
    }

    void UpdateValue(float val)
    {
        
        // Do Tween animations for updating the bar:
        // 1. update the bar value
        // 2. update the bar color
        // 3. update the title text
        
        float start = bar.fillAmount * 100;
        // DoTween animation for changeing the title text value 
        DOTween.To(() => start, x => {
            start = x;
            bar.fillAmount = x / 100;
            bar.color = BarGradient.Evaluate(x / 100);
            txtTitle.text = Title + " " + (int)(x) + "%";
        }, val, anim_duration).SetEase(Ease.OutBounce);
        
        
        
        // get rect transform scale of the game object
        float orig_scale = rectTransform.localScale.x;
        // Scale the game object and then scale it back to normal (with previous tween)
        rectTransform.DOScale(orig_scale*1.5f, anim_duration).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            rectTransform.DOScale(orig_scale, anim_duration).SetEase(Ease.OutCubic);
        });
        
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateValue(50);
            txtTitle.color = TitleColor;
            txtTitle.font = TitleFont;
            txtTitle.fontSize = TitleFontSize;
            barBackground.color = BarBackGroundColor;
            barBackground.sprite = BarBackGroundSprite;
        }
        else
        {
            if (Alert >= barValue && Time.time > nextPlay)
            {
                nextPlay = Time.time + RepeatRate;
                if (sound != null && audiosource != null)
                    audiosource.PlayOneShot(sound);
            }
        }
    }
}