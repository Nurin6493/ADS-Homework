using UnityEngine;

public class DroneCommunication : MonoBehaviour
{
    private Drone head;

    public void Initialize(Drone firstDrone)
    {
        head = firstDrone;
    }

    public void AddDrone(Drone newDrone)
    {
        if (head == null)
        {
            head = newDrone;
            return;
        }

        Drone current = head;
        while (current.NextDrone != null)
        {
            current = current.NextDrone;
        }
        current.NextDrone = newDrone; 
    }

    public Drone FindDrone(int id)
    {
        Drone current = head;
        while (current != null)
        {
            Debug.Log($"Checking drone with ID: {current.Id}");
            if (current.Id == id) return current;
            current = current.NextDrone;
        }
        Debug.Log("Drone not found.");
        return null; 
    }


    public void SelfDestruct(int id)
    {
        Drone current = head;
        Drone previous = null;

        while (current != null)
        {
            if (current.Id == id)
            {
                Debug.Log($"Drone {id} found for self-destruct.");

                if (previous != null)
                {
                    previous.NextDrone = current.NextDrone;
                }
                else
                {
                    head = current.NextDrone;
                }

                Destroy(current.gameObject);
                Debug.Log($"Drone {id} has been destroyed.");
                return;
            }
            previous = current;
            current = current.NextDrone;
        }
        Debug.Log($"Drone with ID {id} not found for self-destruct.");
    }


    public bool DeleteDroneById(int id)
    {
        Drone current = head;
        Drone previous = null;

        while (current != null)
        {
            if (current.Id == id)
            {
                if (previous != null)
                {
                    previous.NextDrone = current.NextDrone; // Bypass the current drone
                }
                else
                {
                    head = current.NextDrone;
                }

                Destroy(current.gameObject); 
                Debug.Log($"Drone with ID {id} has been deleted.");
                return true;
            }
            previous = current;
            current = current.NextDrone;
        }

        Debug.Log($"Drone with ID {id} not found for deletion.");
        return false;
    }


}
