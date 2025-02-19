using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    private void Start()
    {
        GameObject player = FindPlayer();
        if (player != null)
        {
            player.transform.position = transform.position;
        }
        else
        {
            Debug.LogWarning("Player not found in the scene!");
        }
    }

    private GameObject FindPlayer()
    {
        return GameObject.FindGameObjectWithTag("Player");
    }
}
