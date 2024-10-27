using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Flock : MonoBehaviour
{
    private float startTime;
    private int? highlightedDroneId = null;  
    private int lastAssignedId = 0;
    public Drone agentPrefab;
    private Drone headDrone; // Head of the linked list
    public FlockBehavior behavior;
    float squareMaxSpeed;
    float squareNeighborRadius;
    float squareAvoidanceRadius;
    public float SquareAvoidanceRadius { get { return squareAvoidanceRadius; } }
    private StreamWriter csvWriter;
    private bool isActive = false;
    public Color highlightColor = Color.yellow;
    private Color defaultColor = Color.white;
    public DroneCommunication droneComm;
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

    public void SearchDroneById(int id)
    {
        highlightedDroneId = id;

        Drone currentDrone = headDrone;
        while (currentDrone != null)
        {
            currentDrone.GetComponent<SpriteRenderer>().color = defaultColor;

            if (currentDrone.Id == id)
            {
                currentDrone.GetComponent<SpriteRenderer>().color = highlightColor;
                Debug.Log("Drone with ID " + id + " found and highlighted.");
                return;
            }

            currentDrone = currentDrone.NextDrone;
        }

        highlightedDroneId = null;
        Debug.Log("Drone with ID " + id + " not found.");
    }

    public bool DeleteDroneById(int id)
    {
        Drone currentDrone = headDrone;
        Drone previousDrone = null;

        while (currentDrone != null)
        {
            if (currentDrone.Id == id)
            {
                if (previousDrone == null)
                {
                    headDrone = currentDrone.NextDrone; 
                }
                else
                {
                    previousDrone.NextDrone = currentDrone.NextDrone; 
                }

                Destroy(currentDrone.gameObject);
                return true;
            }

            previousDrone = currentDrone;
            currentDrone = currentDrone.NextDrone;
        }

        return false;
    }

    void Start()
    {
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        droneComm = gameObject.AddComponent<DroneCommunication>();

        StartFlock();

        if (headDrone != null)
        {
            droneComm.Initialize(headDrone); 
        }

        InitializeCSV();
    }

    void Update()
    {
        if (!isActive) return;
        startTime = Time.time;

        Drone[] drones = ToArray();
        PartitionDronesByTemperature(drones);

        Drone currentDrone = headDrone;
        while (currentDrone != null)
        {
            List<Transform> context = GetNearbyObjects(currentDrone);
            Vector2 move = behavior.CalculateMove(currentDrone, context, this);
            move *= driveFactor;

            if (move.sqrMagnitude > squareMaxSpeed)
            {
                move = move.normalized * maxSpeed;
            }

            currentDrone.Move(move);
            currentDrone = currentDrone.NextDrone;
        }

        float deltaTime = Time.deltaTime;
        float fps = 1.0f / deltaTime;
        string currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        //Debug.Log($"Timestamp: {currentTime} | FPS: {fps}");
        WriteToCSV(currentTime, fps);
    }

    public void StartFlock()
    {
        Drone previousDrone = null;

        for (int i = 0; i < startingCount; i++)
        {
            Drone newAgent = Instantiate(
                agentPrefab,
                Random.insideUnitCircle * startingCount * AgentDensity,
                Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                transform
            );
            newAgent.Id = lastAssignedId++;
            newAgent.name = "Agent " + newAgent.Id;
            newAgent.Initialize(this);

            if (previousDrone == null)
            {
                headDrone = newAgent; 
            }
            else
            {
                previousDrone.NextDrone = newAgent; 
            }

            previousDrone = newAgent; 
        }

        isActive = true;
    }

    public void StopFlock()
    {
        isActive = false; 
        lastAssignedId = 0; 
    }

    void PartitionDronesByTemperature(Drone[] drones)
    {
        if (drones.Length == 0) return;

        float pivotTemperature = drones[0].Temperature;

        Drone currentDrone = headDrone;
        while (currentDrone != null)
        {
            if (highlightedDroneId.HasValue && currentDrone.Id == highlightedDroneId.Value)
            {
                currentDrone = currentDrone.NextDrone;
                continue;
            }

            currentDrone.GetComponent<SpriteRenderer>().color = currentDrone.Temperature <= pivotTemperature ? Color.blue : Color.red;
            currentDrone = currentDrone.NextDrone;
        }

        //Debug.Log("Partitioning done using temperature. Pivot: " + pivotTemperature);
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

    private void InitializeCSV()
    {
        string filePath = Path.Combine(Application.dataPath, "TimingResults.csv");
        csvWriter = new StreamWriter(filePath, false);
        csvWriter.WriteLine("Timestamp, FPS");
    }

    private void WriteToCSV(string timestamp, float fps)
    {
        csvWriter.WriteLine($"{timestamp}, {fps}");
        csvWriter.Flush();
    }

    private void OnDestroy()
    {
        csvWriter.Close();
    }

    public Drone[] ToArray()
    {
        List<Drone> droneList = new List<Drone>();
        Drone currentDrone = headDrone;
        while (currentDrone != null)
        {
            droneList.Add(currentDrone);
            currentDrone = currentDrone.NextDrone;
        }
        return droneList.ToArray();
    }
}
