using UnityEngine;
using UnityEngine.UI;
using SK.GyroscopeWebGL;

namespace SK.GyroscopeWebGL.Examples
{
    public class SK_GyroscopeTest : MonoBehaviour
    {
        public Text Label;
        public Transform Model;
        public Button Button;
        public Button CalibrateButton;  // Reference to the new calibration button

        // Holds the calibration offset (default: identity means no calibration)
        private Quaternion calibrationOffset = Quaternion.identity;
        // Store the most recent reading for calibration purposes.
        private GyroscopeData lastReading;

        void Awake()
        {
            Button.onClick.AddListener(ToggleGyroscope);
            CalibrateButton.onClick.AddListener(CalibrateGyro);
        }

        private void Start()
        {
            SK_DeviceSensor.StartGyroscopeListener(OnGyroscopeReading);
            Button.GetComponentInChildren<Text>().text = SK_DeviceSensor.IsGyroscopeStarted ? "Gyro Stop" : "Gyro Start";
        }

        void OnDestroy()
        {
            SK_DeviceSensor.StopGyroscopeListener();
        }

        private void OnGyroscopeReading(GyroscopeData reading)
        {
            // Store the last reading
            lastReading = reading;

            // Apply calibration offset (current sensor reading becomes zero)
            Quaternion calibratedRotation = calibrationOffset * reading.UnityRotation;

            // Update the UI label with the calibrated rotation values
            Label.text = $"";
            
            // Update the 3D model's rotation
            Model.rotation = calibratedRotation;
        }

        private void ToggleGyroscope()
        {
            if (SK_DeviceSensor.IsGyroscopeStarted)
            {
                SK_DeviceSensor.StopGyroscopeListener();
            }
            else
            {
                SK_DeviceSensor.StartGyroscopeListener(OnGyroscopeReading);
            }

            Button.GetComponentInChildren<Text>().text = SK_DeviceSensor.IsGyroscopeStarted ? "Gyro Stop" : "Gyro Start";
        }
        
        // Calibration method to set the current orientation as the new zero.
        private void CalibrateGyro()
        {
            // Ensure there's a valid sensor reading for calibration.
            // (If GyroscopeData is a class, you can check for null; if it's a struct, consider another flag.)
            // Here we assume a valid reading has been obtained.
            calibrationOffset = Quaternion.Inverse(lastReading.UnityRotation);

            // Update the label to confirm calibration (optional)
            Label.text += "\nCalibrated!";
        }
    }
}
