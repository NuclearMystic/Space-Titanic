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
    private float playerCheckDistanceSqr; // Optimized squared distance check

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;

        if (!player)
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");

        playerCheckDistanceSqr = playerCheckDistance * playerCheckDistance; // Precompute squared distance

        StartCoroutine(SpawnCheckRoutine());
    }

    IEnumerator SpawnCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            if (activeGremlin == null && IsPlayerFarEnough() && IsOffScreen())
            {
                TrySpawnGremlin();
            }
        }
    }

    private bool IsPlayerFarEnough()
    {
        return (transform.position - player.position).sqrMagnitude > playerCheckDistanceSqr;
    }

    private bool IsOffScreen()
    {
        if (mainCamera == null) return false; // Prevent errors if camera is missing

        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
        return screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1;
    }

    private void TrySpawnGremlin()
    {
        if (gremlinPrefab == null)
        {
            Debug.LogWarning("Gremlin prefab is not assigned in GremlinSpawnPoint!");
            return;
        }

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
