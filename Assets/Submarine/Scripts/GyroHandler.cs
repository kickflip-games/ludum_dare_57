using UnityEngine;

public class GyroHandler : MonoBehaviour
{
    // Latest rotation rate received from the mobile device (in degrees per second, normalized)
    public Vector3 rotationRate { get; private set; }
    
    // True once valid gyroscope data is received
    public bool hasGyroInput { get; private set; } = false;

    // This method is called via SendMessage from JavaScript
    public void SetRotationRate(string input)
    {
        // Expected format: "alpha|beta|gamma"
        string[] angles = input.Split('|');
        if (angles.Length >= 3)
        {
            if (float.TryParse(angles[0], out float alpha) &&
                float.TryParse(angles[1], out float beta) &&
                float.TryParse(angles[2], out float gamma))
            {
                // You can adjust the divisor (90f in this case) depending on your desired sensitivity
                rotationRate = new Vector3(alpha, beta, gamma) / 90f;
                hasGyroInput = true;
            }
        }
    }
}
