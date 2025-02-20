using UnityEngine;
using System.Collections;

public class GremlinSpawnPoint : MonoBehaviour
{
    public GameObject gremlinPrefab; // Gremlin prefab reference
    public float checkInterval = 2f; // Time between spawn checks
    public float playerCheckDistance = 10f; // Distance from player to allow spawn
    public float spawnChance = 0.3f; // 30% chance to spawn when conditions are met

    private GameObject activeGremlin; // Track the current gremlin
    private Transform player;
    private Camera mainCamera;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;

        if (!player)
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");

        StartCoroutine(SpawnCheckRoutine());
    }

    IEnumerator SpawnCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            if (!activeGremlin && IsPlayerFarEnough() && IsOffScreen())
            {
                TrySpawnGremlin();
            }
        }
    }

    private bool IsPlayerFarEnough()
    {
        return Vector3.Distance(transform.position, player.position) > playerCheckDistance;
    }

    private bool IsOffScreen()
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
        return screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1;
    }

    private void TrySpawnGremlin()
    {
        if (Random.value <= spawnChance) // Random chance to spawn
        {
            activeGremlin = Instantiate(gremlinPrefab, transform.position, Quaternion.identity);
        }
    }

    public void NotifyGremlinDespawn()
    {
        activeGremlin = null; // Reset so it can spawn a new one later
    }
}
