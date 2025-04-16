using System;
using DG.Tweening;
using UnityEngine;

using UnityEngine.UI;
    

namespace Ilumisoft.RadarSystem.UI
{
    /// <summary>
    /// Concrete component for the icon being visible on the radar of a locatable
    /// </summary>
    [AddComponentMenu("Radar System/UI/Locatable Icon")]
    [RequireComponent(typeof(CanvasGroup))]
    public class LocatableIcon : LocatableIconComponent
    {
        
        
        [Header("Icon Sprites")]
        [SerializeField]
        private Sprite defaultIcon;
        [SerializeField]
        public Sprite upIcon;
        [SerializeField]
        public Sprite downIcon;
        
        protected Image iconImage;
        protected RectTransform iconRectTransform;
        
        protected CanvasGroup CanvasGroup { get; set; }

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            iconImage = GetComponent<Image>();
            iconRectTransform = GetComponent<RectTransform>();

        }
        
        

        public override void SetVisible(bool visibility)
        {
            CanvasGroup.alpha = visibility ? 1.0f : 0.0f;
        }

        public override void SetSprite(int id)
        {
            if (id > 0)
            {
                iconImage.sprite = upIcon;
            }
            else if (id < 0)
            {
                iconImage.sprite = downIcon;
            }
            else
            {
                iconImage.sprite = defaultIcon;
            }
        }
        
        
        public void PlayPulseAnimation()
        {
            // DoTween to pulsate the icon
            if (!DOTween.IsTweening(iconRectTransform))
            {
                // DoTween to pulsate the icon
                iconRectTransform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 1, 0.5f)
                    .SetEase(Ease.OutSine)
                    .SetLoops(2, LoopType.Yoyo);
            }
        
        }
        
        
    }
}