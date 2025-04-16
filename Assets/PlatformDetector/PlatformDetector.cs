using UnityEngine;

[ExecuteAlways]
public class PlatformDetector : MonoBehaviour
{
    public enum PlatformType
    {
        Editor,
        WebGL_Mobile,
        WebGL_Desktop,
        Other
    }

    // Singleton instance
    public static PlatformDetector Instance { get; private set; }

    [Header("Debug Settings")]
    [Tooltip("Check to override platform detection for debugging purposes.")]
    public bool overridePlatformDetection = false;

    [Tooltip("Manually select the platform for debugging.")]
    public PlatformType manualPlatform = PlatformType.Other;

    // Stores the detected (or manually set) platform
    public PlatformType CurrentPlatform { get; private set; }

    void Awake()
    {
        // Setup singleton instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // if not in editor mode, make this object persistent across scenes
        
        if (!Application.isEditor)
            DontDestroyOnLoad(gameObject);

        // Platform detection using preprocessor directives
#if UNITY_EDITOR
        CurrentPlatform = PlatformType.Editor;
#elif UNITY_WEBGL
        CurrentPlatform = IsMobileBrowser() ? PlatformType.WebGL_Mobile : PlatformType.WebGL_Desktop;
#else
        CurrentPlatform = PlatformType.Other;
#endif
        
        if (overridePlatformDetection)
            CurrentPlatform = manualPlatform;
        Debug.Log("Running on: " + CurrentPlatform);
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private bool IsMobileBrowser()
    {
        // Check for touch capability on WebGL
        return Input.touchSupported;
    }
#endif

    // Display the detected platform on the game screen (bottom left corner)
    void OnGUI()
    {
        GUIStyle style = new GUIStyle
        {
            fontSize = 8,
            normal = { textColor = Color.white }
        };

        // Adjust the label's position to the bottom left of the screen.
        GUI.Label(new Rect(5, Screen.height - 15, 300, 20), "Platform: " + CurrentPlatform, style);
    }
}