using System.Collections;
using UnityEngine;
using UnityEngine.Rendering; // Needed for AsyncGPUReadback

public class BoidManager : MonoBehaviour {

    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    Boid[] boids;

    void Start () {
        boids = FindObjectsOfType<Boid>();
        foreach (Boid b in boids) {
            b.Initialize(settings, null);
        }
    }

    void Update () {
        if (boids != null) {
            if (!SystemInfo.supportsComputeShaders) {
                RunBoidsOnCPU();
            } else {
                RunBoidsOnGPU();
            }
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
    
    void RunBoidsOnGPU() {
        int numBoids = boids.Length;
        // Prepare the data to be sent to the GPU.
        var boidData = new BoidData[numBoids];
        for (int i = 0; i < numBoids; i++) {
            boidData[i].position = boids[i].position;
            boidData[i].direction = boids[i].forward;
        }
        
        // Create a compute buffer and load data.
        var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
        boidBuffer.SetData(boidData);

        // Set compute shader buffers and parameters.
        compute.SetBuffer(0, "boids", boidBuffer);
        compute.SetInt("numBoids", numBoids);
        compute.SetFloat("viewRadius", settings.perceptionRadius);
        compute.SetFloat("avoidRadius", settings.avoidanceRadius);

        int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
        compute.Dispatch(0, threadGroups, 1, 1);

        // Request an asynchronous GPU readback of the compute buffer.
        AsyncGPUReadback.Request(boidBuffer, request => {
            // First, check for errors
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
            boidBuffer.Release();
        });
    }
    
    
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

            boids[i].avgFlockHeading = flockHeading;
            boids[i].centreOfFlockmates = flockCentre;
            boids[i].avgAvoidanceHeading = avoidanceHeading;
            boids[i].numPerceivedFlockmates = numFlockmates;

            boids[i].UpdateBoid();
        }
    }

    
}
