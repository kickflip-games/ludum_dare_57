using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableNeedle : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    // Assign these in the Unity Inspector
    public Texture2D handCursor;
    public Texture2D grabbingCursor;
    public Vector2 cursorHotspot = new Vector2(16, 16);

    [SerializeField]
    [Range(0.0f, 360.0f)]
    public float adjustment_angle = 0.0f;
    
    private SpeedGauge speedGauge;
    private RectTransform gaugeRect;
    private bool dragging = false;
    
    [SerializeField]
    bool debugGizmos = false;

    private void Awake()
    {
        // Find the SpeedGauge component in a parent and get its RectTransform
        speedGauge = GetComponentInParent<SpeedGauge>();
        if (speedGauge != null)
        {
            gaugeRect = speedGauge.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("DraggableNeedle: Cannot find SpeedGauge in parent!");
        }
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!dragging)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!dragging)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        UpdateNeedleFromPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragging)
        {
            UpdateNeedleFromPointer(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
        Cursor.SetCursor(handCursor, cursorHotspot, CursorMode.Auto);
    }

    // This method converts the pointer position into a gauge value and rotates the needle accordingly.
    private void UpdateNeedleFromPointer(PointerEventData eventData)
    {
        if (gaugeRect == null)
            return;

        Vector2 localPoint;
        // Convert the screen pointer position to a local point within the gauge's RectTransform.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(gaugeRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Get the center of the gauge in local space.
            Vector2 center = Vector2.zero;
            Vector2 direction = localPoint - center;

            if (debugGizmos)
            {
                // For debugging: convert the gauge center and pointer position to world space.
                Vector3 worldCenter = gaugeRect.TransformPoint(center);
                Vector3 worldPointer;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(gaugeRect, eventData.position, eventData.pressEventCamera, out worldPointer);
                Debug.DrawLine(worldCenter, worldPointer, Color.red, 2f);                
            }

            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float adjustedAngle = angle - adjustment_angle;
            float clampedAngle = Mathf.Clamp(adjustedAngle, -89.99f, 0.1f);
            Debug.Log("Adjusted angle: " + clampedAngle );
            // // Convert the clamped angle to a normalized t value between 0 and 1.
            float t = Mathf.InverseLerp(0f, -90f, clampedAngle);
            speedGauge.UpdatePlayerSpeed(t);
        }
    }
}
