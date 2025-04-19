using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SK.GyroscopeWebGL;

namespace SK.GyroscopeWebGL.Examples
{
    public class SK_GyroscopeTest : MonoBehaviour
    {
        public Text Label;         // For displaying sensor values (optional)
        public Transform Model;    // The 3D model whose rotation will be updated

        // Calibration offset; default is identity (no calibration)
        private Quaternion calibrationOffset = Quaternion.identity;
        // Store the most recent gyroscope reading
        private GyroscopeData lastReading;
        
        public bool isGyroEnabled = false;
        
        
        
        public bool deviceSupportsGyro
        {
            get
            {
                // Check if the device supports gyroscope
                return SystemInfo.supportsGyroscope && Application.isMobilePlatform;
            }
        }

        private void Start()
        {
            if (!deviceSupportsGyro)
            {
                Debug.LogWarning("Gyroscope not supported on this device.");
                return;
            }
            else
            {
                // Start the gyroscope listener by default
                SK_DeviceSensor.StartGyroscopeListener(OnGyroscopeReading);
            
                // Automatically calibrate the gyroscope after a short delay to allow valid readings to come in.
                StartCoroutine(CalibrateGyroAtStart());
            }

        }

        private void OnDestroy()
        {
            SK_DeviceSensor.StopGyroscopeListener();
        }

        /// <summary>
        /// Wait a short time before calibrating, so that a valid sensor reading is available.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CalibrateGyroAtStart()
        {
            // Wait for a half-second; adjust if necessary for your specific hardware/environment.
            yield return new WaitForSeconds(0.5f);
            
            // Only calibrate if a valid sensor reading has been received.
            if (lastReading != null)
            {
                calibrationOffset = Quaternion.Inverse(lastReading.UnityRotation);
                Debug.Log("Gyroscope calibrated at start.");
            }
            else
            {
                Debug.LogWarning("No valid gyro reading available for calibration.");
            }
        }

        /// <summary>
        /// This callback is called every time a new gyroscope reading is received.
        /// </summary>
        /// <param name="reading">The current sensor reading.</param>
        private void OnGyroscopeReading(GyroscopeData reading)
        {
            if (!isGyroEnabled)
                return;
            
            // Save the latest reading for calibration purposes.
            lastReading = reading;

            // Apply the calibration offset: this adjusts the reading so that the calibrated orientation becomes zero.
            Quaternion calibratedRotation = calibrationOffset * reading.UnityRotation;

            // Optionally display the sensor data on the UI Label.
            if (Label != null)
            {
                Label.text = $"alpha: {reading.Alpha}, beta: {reading.Beta}, gamma: {reading.Gamma}, absolute: {reading.Absolute}, unityRotation: {calibratedRotation}";
            }

            // Update the model's rotation using the calibrated rotation.
            Model.rotation = calibratedRotation;
        }
    }
}
