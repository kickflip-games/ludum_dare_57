using Ilumisoft.RadarSystem.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Ilumisoft.RadarSystem
{
    [AddComponentMenu("Radar System/Radar")]
    [DefaultExecutionOrder(-10)]
    public class Radar : MonoBehaviour
    {
        /// <summary>
        /// Dictionary allowing to access the icon of a locatable
        /// </summary>
        readonly Dictionary<LocatableComponent, LocatableIconComponent> locatableIconDictionary = new();

        [SerializeField]
        [Tooltip("The container icons will be added to")]
        private RectTransform iconContainer;

        [SerializeField, Min(1)]
        [Tooltip("The detection range of the radar in meter")]
        private float range = 20;

        [SerializeField]
        [Tooltip("Whether the radar should apply the rotation of the player")]
        private bool applyRotation = true;

        /// <summary>
        /// The detection range of the radar
        /// </summary>
        public float Range { get => range; set => range = value; }

        /// <summary>
        /// Whether the radar rotates with the player or not
        /// </summary>
        public bool ApplyRotation { get => applyRotation; set => applyRotation = value; }

        /// <summary>
        /// Reference to the player
        /// </summary>
        public GameObject Player;

        // --- New fields for vertical threshold and icon sprites ---
        [SerializeField]
        [Tooltip("Vertical difference threshold to change icon (in meters)")]
        private float verticalThreshold = 1.0f;

        // Section
        [Header("Radar Sweep")]
        [Tooltip("Radar sweep line")]
        public RectTransform radarSweepLine;
        [Tooltip("Speed of the radar sweep line (in degrees per second)")]
        public float sweepSpeed = 180.0f;
        
        
        private void OnEnable()
        {
            LocatableManager.OnLocatableAdded += OnLocatableAdded;
            LocatableManager.OnLocatableRemoved += OnLocatableRemoved;
        }

        private void OnDisable()
        {
            LocatableManager.OnLocatableAdded -= OnLocatableAdded;
            LocatableManager.OnLocatableRemoved -= OnLocatableRemoved;
        }

        /// <summary>
        /// Callback invoked when a locatable has been added
        /// </summary>
        private void OnLocatableAdded(LocatableComponent locatable)
        {
            // Create the icon for the locatable and add a new entry to the dictionary
            if (locatable != null && !locatableIconDictionary.ContainsKey(locatable))
            {
                var icon = locatable.CreateIcon();
                icon.transform.SetParent(iconContainer.transform, false);
                locatableIconDictionary.Add(locatable, icon);
            }
        }

        /// <summary>
        /// Callback invoked when a locatable has been removed
        /// </summary>
        private void OnLocatableRemoved(LocatableComponent locatable)
        {
            // Remove the locatable from the dictionary and destroy the icon
            if (locatable != null && locatableIconDictionary.TryGetValue(locatable, out LocatableIconComponent icon))
            {
                locatableIconDictionary.Remove(locatable);
                Destroy(icon.gameObject);
            }
        }

        private void Update()
        {
            
            float previousRotation = (radarSweepLine.eulerAngles.z % 360) - 180;
            radarSweepLine.eulerAngles -= new Vector3(0, 0, sweepSpeed * Time.deltaTime);
            float currentRotation = (radarSweepLine.eulerAngles.z % 360) - 180;
            
            if (Player != null)
            {
                UpdateLocatableIcons();
            }
        }

        /// <summary>
        /// Updates the position of all icons and adjusts the icon sprite based on vertical offset.
        /// </summary>
        private void UpdateLocatableIcons()
        {
            LocatableComponent closestLocatable = null;
            float closestDistanceSqr = float.MaxValue;
        
            // Run through all locatables in the dictionary
            foreach (var locatable in locatableIconDictionary.Keys)
            {
                // Update the icon position, icon sprite, and visibility for the locatable
                if (locatableIconDictionary.TryGetValue(locatable, out var icon))
                {
                    if (TryGetIconLocation(locatable, out var iconLocation))
                    {
                        // Determine the vertical difference between the locatable and the player.
                        float verticalDiff = locatable.transform.position.y - Player.transform.position.y;
                        if (verticalDiff > verticalThreshold)
                        {
                            // Locatable is significantly above the player
                            icon.SetSprite(1);
                        }
                        else if (verticalDiff < -verticalThreshold)
                        {
                            // Locatable is significantly below the player
                            icon.SetSprite(-1);
                        }
                        else
                        {
                            // Within threshold—use default icon.
                            icon.SetSprite(0);
                        }
        
                        icon.SetVisible(true);
        
                        var rectTransform = icon.GetComponent<RectTransform>();
                        rectTransform.anchoredPosition = iconLocation;
        
                        // Calculate the squared distance to the player
                        float distanceSqr = (locatable.transform.position - Player.transform.position).sqrMagnitude;
                        if (distanceSqr < closestDistanceSqr)
                        {
                            closestDistanceSqr = distanceSqr;
                            closestLocatable = locatable;
                        }
                    }
                    else
                    {
                        icon.SetVisible(false);
                    }
                }
            }
        
            // Trigger animation on the closest locatable's icon
            if (closestLocatable != null && locatableIconDictionary.TryGetValue(closestLocatable, out var closestIcon))
            {
                if (closestIcon is LocatableIcon locatableIcon)
                {
                    locatableIcon.PlayPulseAnimation();
                }
            }
        }

        /// <summary>
        /// Computes the location of the icon on the radar. Returns true if the icon is visible, false otherwise
        /// </summary>
        /// <param name="locatable"></param>
        /// <param name="iconLocation"></param>
        /// <returns></returns>
        private bool TryGetIconLocation(LocatableComponent locatable, out Vector2 iconLocation)
        {
            iconLocation = GetDistanceToPlayer(locatable);

            float radarSize = GetRadarUISize();
            var scale = radarSize / Range;
            iconLocation *= scale;

            // Rotate the icon by the player's y rotation if enabled.
            if (ApplyRotation)
            {
                // Get the forward vector of the player projected on the xz plane.
                var playerForwardDirectionXZ = Vector3.ProjectOnPlane(Player.transform.forward, Vector3.up);
                // Create a rotation from the direction.
                var rotation = Quaternion.LookRotation(playerForwardDirectionXZ);
                // Mirror y rotation.
                var euler = rotation.eulerAngles;
                euler.y = -euler.y;
                rotation.eulerAngles = euler;
                // Rotate the icon location in 3D space.
                var rotatedIconLocation = rotation * new Vector3(iconLocation.x, 0.0f, iconLocation.y);
                // Convert from 3D to 2D.
                iconLocation = new Vector2(rotatedIconLocation.x, rotatedIconLocation.z);
            }

            if (iconLocation.sqrMagnitude < radarSize * radarSize || locatable.ClampOnRadar)
            {
                // Clamp the icon location so it never shows outside the radar.
                iconLocation = Vector2.ClampMagnitude(iconLocation, radarSize);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the size of the radar UI.
        /// </summary>
        private float GetRadarUISize()
        {
            return iconContainer.rect.width / 2;
        }

        /// <summary>
        /// Returns the distance to the player on the xz plane.
        /// </summary>
        private Vector2 GetDistanceToPlayer(LocatableComponent locatable)
        {
            Vector3 distanceToPlayer = locatable.transform.position - Player.transform.position;
            return new Vector2(distanceToPlayer.x, distanceToPlayer.z);
        }
    }
}
