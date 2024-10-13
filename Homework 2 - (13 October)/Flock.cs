using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Flock : MonoBehaviour
{
    // Unity-native timing variables
    private float startTime;

    public Drone agentPrefab;
    List<Drone> agents = new List<Drone>();
    public FlockBehavior behavior;

    [Range(10, 5000)]
    public int startingCount = 250;
    const float AgentDensity = 0.08f;

    [Range(1f, 100f)]
    public float driveFactor = 10f;
    [Range(1f, 100f)]
    public float maxSpeed = 5f;
    [Range(1f, 10f)]
    public float neighborRadius = 1.5f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.5f;

    float squareMaxSpeed;
    float squareNeighborRadius;
    float squareAvoidanceRadius;
    public float SquareAvoidanceRadius { get { return squareAvoidanceRadius; } }

    private StreamWriter csvWriter; // CSV writer

    // Start is called before the first frame update
    void Start()
    {
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        // Initialize the CSV file
        InitializeCSV();

        for (int i = 0; i < startingCount; i++)
        {
            Drone newAgent = Instantiate(
                agentPrefab,
                UnityEngine.Random.insideUnitCircle * startingCount * AgentDensity,
                Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)),
                transform
                );
            newAgent.name = "Agent " + i;
            newAgent.Initialize(this);
            agents.Add(newAgent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Start measuring time
        startTime = Time.time;

        Drone[] drones = agents.ToArray();

        // Perform partitioning based on temperature
        PartitionDronesByTemperature(drones);

        foreach (Drone agent in agents)
        {
            // Get nearby objects
            List<Transform> context = GetNearbyObjects(agent);

            // Calculate next move direction
            Vector2 move = behavior.CalculateMove(agent, context, this);
            move *= driveFactor;
            if (move.sqrMagnitude > squareMaxSpeed)
            {
                move = move.normalized * maxSpeed;
            }

            // Apply movement
            agent.Move(move);
        }

        // Measure the frame time using Time.deltaTime
        float deltaTime = Time.deltaTime;
        float fps = 1.0f / deltaTime;

        // Log the current time and FPS
        string currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        UnityEngine.Debug.Log($"Timestamp: {currentTime} | FPS: {fps}");

        // Write to CSV
        WriteToCSV(currentTime, fps);
    }

    // O(N) partition function
    void PartitionDronesByTemperature(Drone[] drones)
    {
        if (drones.Length == 0) return;

        float pivotTemperature = drones[0].Temperature;

        foreach (Drone drone in drones)
        {
            if (drone.Temperature <= pivotTemperature)
            {
                drone.GetComponent<SpriteRenderer>().color = Color.blue;  // Cool color
            }
            else
            {
                drone.GetComponent<SpriteRenderer>().color = Color.red;   // Warm color
            }
        }

        Debug.Log("Partitioning done using temperature. Pivot: " + pivotTemperature);
    }

    List<Transform> GetNearbyObjects(Drone agent)
    {
        List<Transform> context = new List<Transform>();
        Collider2D[] contextColliders = Physics2D.OverlapCircleAll(agent.transform.position, neighborRadius);
        foreach (Collider2D c in contextColliders)
        {
            if (c != agent.AgentCollider)
            {
                context.Add(c.transform);
            }
        }
        return context;
    }

    // Initialize the CSV file
    private void InitializeCSV()
    {
        string filePath = Path.Combine(Application.dataPath, "TimingResults.csv");

        // Create or overwrite the CSV file and write the header
        csvWriter = new StreamWriter(filePath, false);
        csvWriter.WriteLine("Timestamp, FPS");
    }

    // Write timing results to the CSV file
    private void WriteToCSV(string timestamp, float fps)
    {
        csvWriter.WriteLine($"{timestamp}, {fps}");
        csvWriter.Flush(); // Ensure data is written to the file
    }

    // Cleanup on destroy
    private void OnDestroy()
    {
        csvWriter.Close(); // Close the CSV writer when the script is destroyed
    }
}
