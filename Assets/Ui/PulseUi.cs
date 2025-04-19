using UnityEngine;
using DG.Tweening;

public class PulseUi : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Pulse the UI element using Tweening
        transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 3, 1)
            .SetEase(Ease.OutBack)
            .SetLoops(-1, LoopType.Yoyo);
        
    }


}
