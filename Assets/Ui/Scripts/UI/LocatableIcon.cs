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
        
        public Sprite defaultIcon;
        public Sprite upIcon;
        public Sprite downIcon;
        protected Image iconImage;
        
        protected CanvasGroup CanvasGroup { get; set; }

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            iconImage = GetComponent<Image>();
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
        
    }
}