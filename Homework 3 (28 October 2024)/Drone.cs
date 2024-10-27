using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Drone : MonoBehaviour
{
    public int Temperature { set; get; } = 0;
    public int Id; // Unique ID for the drone
    private Drone nextDrone; // Reference to the next drone in the linked list

    public Drone NextDrone // Property to access the next drone
    {
        get { return nextDrone; }
        set { nextDrone = value; }
    }

    private Flock agentFlock;
    public Flock AgentFlock => agentFlock;

    private Collider2D agentCollider;
    public Collider2D AgentCollider => agentCollider;

    void Start()
    {
        agentCollider = GetComponent<Collider2D>();
        Debug.Log($"Drone {Id} initialized.");
    }

    private void Update()
    {
        Temperature = (int)(Random.value * 100);
    }

    public void Initialize(Flock flock)
    {
        agentFlock = flock;
    }

    public void Move(Vector2 velocity)
    {
        transform.up = velocity;
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    public void ReceiveMessage(string message)
    {
        if (message == "self-destruct")
        {
            gameObject.SetActive(false);
            Debug.Log($"Drone {Id} has self-destructed.");
        }
        else
        {
            Debug.Log($"Drone {Id} received message: {message}");
        }
    }
}
