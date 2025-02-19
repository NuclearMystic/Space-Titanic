using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private Transform ladderSnapPoint;  // Player aligns with this, but only Y should change
    [SerializeField] private Transform ladderBottom;     // Lowest point of the ladder
    [SerializeField] private Transform ladderTop;        // Highest point of the ladder
    [SerializeField] private Transform topExitPoint;     // Where the player lands after climbing up

    private PlayerController player;
    private bool playerInRange = false;

    private float fixedX, fixedZ; // Store original X and Z positions

    void Start()
    {
        // Save the original position of the snap point
        fixedX = ladderSnapPoint.position.x;
        fixedZ = ladderSnapPoint.position.z;
    }

    void Update()
    {
        if (playerInRange && player != null)
        {
            // Keep the snap point at the player's Y within the ladder range, but keep X and Z unchanged
            float clampedY = Mathf.Clamp(player.transform.position.y, ladderBottom.position.y, ladderTop.position.y);
            ladderSnapPoint.position = new Vector3(fixedX, clampedY, fixedZ);

            // Handle interaction input in Update() to ensure it always registers
            if (Input.GetButtonDown("Interact"))
            {
                player.StartClimbing(this, ladderSnapPoint.position);
                UIController.Instance.HideInteractionPrompt();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            playerInRange = true;
            UIController.Instance.ShowInteractionPrompt(player.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            UIController.Instance.HideInteractionPrompt();
            player = null;
        }
    }

    public void CheckIfAtTop(PlayerController player)
    {
        if (player.transform.position.y >= ladderTop.position.y - 0.1f)
        {
            player.ExitLadderAtTop(topExitPoint.position);
        }
    }
}
