using UnityEngine;
using DG.Tweening;



public class Wiggle : MonoBehaviour
{
    
    [Range(0, 90)]
    public float wiggleAngle = 10; // The angle of the wiggle
    
    [Range(0, 10)]
    public float wiggleDuration = 0.5f; // The speed of the wiggle
    
    private void Start()
    {
        // Set the initial position of the fish
        transform.localPosition = new Vector3(0, 0, 0);

        // Start the wiggle animation
        OneWiggle();

    }

    private void OneWiggle()
    {
        transform.DOLocalRotate(new Vector3(0, wiggleAngle, 0), wiggleDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                transform.DOLocalRotate(new Vector3(0, -wiggleAngle, 0), wiggleDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(OneWiggle);
            });
    }
    
}