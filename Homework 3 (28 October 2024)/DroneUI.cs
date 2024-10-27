using UnityEngine;
using TMPro;

public class DroneUI : MonoBehaviour
{
    public TMP_InputField idInputField;
    public TMP_Text messageOutputText;
    public Flock flock; 

    void Start()
    {
        flock = FindObjectOfType<Flock>(); 
    }

    public void OnSearchDroneButton()
    {
        if (int.TryParse(idInputField.text, out int id))
        {
            flock.SearchDroneById(id);
            messageOutputText.text = $"Searching for drone with ID {id}. Check console for results.";
        }
        else
        {
            messageOutputText.text = "Invalid ID.";
        }
    }

    public void OnSelfDestructButton()
    {
        if (int.TryParse(idInputField.text, out int id))
        {
            bool result = flock.DeleteDroneById(id);
            messageOutputText.text = result
                ? $"Drone with ID {id} has been deleted."
                : $"Drone with ID {id} not found.";
        }
        else
        {
            messageOutputText.text = "Invalid ID.";
        }
    }
}
