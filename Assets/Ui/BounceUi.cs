using UnityEngine;
using DG.Tweening;

public class BounceUi : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // DoTween to bounce the UI element (small up and down movement)
        transform.DOPunchPosition(Vector3.up * 0.5f, 0.5f, 10, 1)
            .SetEase(Ease.OutBack)
            .SetLoops(-1, LoopType.Yoyo);
        
    }

    
}
