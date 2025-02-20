using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private Transform ladderSnapPoint;  // Player aligns with this, only Y changes
    [SerializeField] private Transform ladderBottom;     // Lowest point of the ladder
    [SerializeField] private Transform ladderTop;        // Highest point of the ladder
    [SerializeField] private Transform topExitPoint;     // Where the player lands after climbing up

    private PlayerController player;
    private bool playerInRange = false;

    private float fixedX, fixedZ; // Store original X and Z positions
    private const float topThreshold = 0.1f; // Configurable exit threshold

    void Start()
    {
        fixedX = ladderSnapPoint.position.x;
        fixedZ = ladderSnapPoint.position.z;
    }

    void Update()
    {
        if (playerInRange && player != null && Input.GetKeyDown(KeyCode.E))
        {
            StartClimb();
            UIController.Instance.HideInteractionPrompt();
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
            if (player != null)
                UIController.Instance.HideInteractionPrompt();
            player = null;
        }
    }

    private void StartClimb()
    {
        if (player == null) return;

        // Lock X and Z, adjust Y
        float clampedY = Mathf.Clamp(player.transform.position.y, ladderBottom.position.y, ladderTop.position.y);
        Vector3 snapPosition = new Vector3(fixedX, clampedY, fixedZ);

        player.StartClimbing(this, snapPosition);
    }

    public void CheckIfAtTop(PlayerController player)
    {
        if (Mathf.Abs(player.transform.position.y - ladderTop.position.y) < topThreshold)
        {
            player.ExitLadderAtTop(topExitPoint.position);
        }
    }
}
