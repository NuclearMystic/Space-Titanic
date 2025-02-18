using UnityEngine;

public class Ladder : MonoBehaviour
{
    private bool playerInRange = false;
    private PlayerController player;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                UIController.Instance.ShowInteractionPrompt(player.transform);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                UIController.Instance.ShowInteractionPrompt(player.transform);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (player != null)
            {
                UIController.Instance.HideInteractionPrompt();
                player = null;
            }
        }
    }

    void Update()
    {
        if (playerInRange && player != null && Input.GetButtonDown("Interact"))
        {
            player.StartClimbing(transform);
            UIController.Instance.HideInteractionPrompt();
        }
    }
}
