// BoidManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering; // Needed for AsyncGPUReadback

public class BoidManager : MonoBehaviour {

    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;

    Boid[] boids;

    // Persistent compute buffer (assumes boid count remains constant)
    ComputeBuffer boidBuffer;


    void Start() {
        
        Debug.Log("------COMPUTE SHADER INFO------");
        Debug.Log("Max compute buffer inputs: " + SystemInfo.maxComputeBufferInputsVertex);
        Debug.Log("Max compute buffer inputs (fragment): " + SystemInfo.maxComputeBufferInputsFragment);
        Debug.Log("Supports compute shaders: " + SystemInfo.supportsComputeShaders);
        Debug.Log("Max compute work group size: " + SystemInfo.maxComputeWorkGroupSize); // Available in newer Unity versions
        Debug.Log("-------------------------------");
        
        boids = FindObjectsOfType<Boid>();
        foreach (Boid b in boids) {
            b.Initialize(settings, null);
        }
        // Create the compute buffer for all boids once.
        if (boids != null && boids.Length > 0) {
            boidBuffer = new ComputeBuffer(boids.Length, BoidData.Size);
        }
    }

    void OnDestroy() {
        if (boidBuffer != null)
            boidBuffer.Release();
    }

    void Update() {
        if (boids == null) return;

        // Choose processing path depending on GPU support.
        if (!SystemInfo.supportsComputeShaders) {
            RunBoidsOnCPU();
        } else {
            RunBoidsOnGPU();
        }
    }

    public struct BoidData {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size {
            get {
                // 5 Vector3 values (3 floats each) and 1 int.
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }

    // GPU processing path.
    void RunBoidsOnGPU() {
        int numBoids = boids.Length;

        // Prepare data to send to the GPU.
        var boidData = new BoidData[numBoids];
        for (int i = 0; i < numBoids; i++) {
            boidData[i].position = boids[i].position;
            boidData[i].direction = boids[i].forward;
            // Reset computed values to zero before the compute shader writes into them.
            boidData[i].flockHeading = Vector3.zero;
            boidData[i].flockCentre = Vector3.zero;
            boidData[i].avoidanceHeading = Vector3.zero;
            boidData[i].numFlockmates = 0;
        }

        // Update our persistent compute buffer with the new boid data.
        boidBuffer.SetData(boidData);

        // Set compute shader buffers and parameters.
        compute.SetBuffer(0, "boids", boidBuffer);
        compute.SetInt("numBoids", numBoids);
        compute.SetFloat("viewRadius", settings.perceptionRadius);
        compute.SetFloat("avoidRadius", settings.avoidanceRadius);

        int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
        compute.Dispatch(0, threadGroups, 1, 1);

        // Request an asynchronous GPU readback.
        AsyncGPUReadback.Request(boidBuffer, request => {
            if (request.hasError) {
                Debug.LogError("GPU readback error detected.");
            } else {
                // Obtain a native array of the boid data.
                var data = request.GetData<BoidData>();

                // Update each boid using the data returned from the compute shader.
                for (int i = 0; i < numBoids; i++) {
                    boids[i].avgFlockHeading = data[i].flockHeading;
                    boids[i].centreOfFlockmates = data[i].flockCentre;
                    boids[i].avgAvoidanceHeading = data[i].avoidanceHeading;
                    boids[i].numPerceivedFlockmates = data[i].numFlockmates;

                    // You can call a method on each boid to process the new data.
                    boids[i].UpdateBoid();
                }
            }
            // Always release the compute buffer when done.
            // boidBuffer.Release();
        });
    }

    // CPU fallback path.
    void RunBoidsOnCPU() {
        for (int i = 0; i < boids.Length; i++) {
            Vector3 flockHeading = Vector3.zero;
            Vector3 flockCentre = Vector3.zero;
            Vector3 avoidanceHeading = Vector3.zero;
            int numFlockmates = 0;

            for (int j = 0; j < boids.Length; j++) {
                if (i == j) continue;
                float dist = Vector3.Distance(boids[i].position, boids[j].position);
                if (dist < settings.perceptionRadius) {
                    flockHeading += boids[j].forward;
                    flockCentre += boids[j].position;
                    numFlockmates++;
                    if (dist < settings.avoidanceRadius) {
                        avoidanceHeading += (boids[i].position - boids[j].position);
                    }
                }
            }

            if (numFlockmates > 0) {
                flockCentre /= numFlockmates;
                flockHeading /= numFlockmates;
            }

            // For CPU, update boids immediately.
            boids[i].avgFlockHeading = flockHeading;
            boids[i].centreOfFlockmates = flockCentre;
            boids[i].avgAvoidanceHeading = avoidanceHeading;
            boids[i].numPerceivedFlockmates = numFlockmates;

            boids[i].UpdateBoid();
        }
    }
}
