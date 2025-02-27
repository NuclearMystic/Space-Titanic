using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroGManager : MonoBehaviour
{
    public static ZeroGManager Instance { get; private set; }

    private GremlinSpawnPoint[] gremlinSpawners;
    private Gremlin[] activeGremlins;

    public float floatAwaySpeed = 2f;  // Speed at which gremlins float away
    public float despawnTime = 3f;  // How long before they despawn

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Debug.Log("Zero G manager active");
    }

    public void ActivateZeroG()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) player.ActivateZeroG();

        // Find all Gremlins and Spawners
        activeGremlins = FindObjectsOfType<Gremlin>(); // Change this to whatever script controls Gremlins
        gremlinSpawners = FindObjectsOfType<GremlinSpawnPoint>(); // Change this to your actual Gremlin spawner script

        // Disable all spawners so no new Gremlins appear
        foreach (var spawner in gremlinSpawners)
        {
            spawner.gameObject.SetActive(false);
        }

        // Make all Gremlins float away and despawn
        foreach (var gremlin in activeGremlins)
        {
            FloatAndDespawnGremlin(gremlin);
        }
    }

    private void FloatAndDespawnGremlin(Gremlin gremlin)
    {
        // Disable the Gremlin's normal AI behavior
        gremlin.enabled = false;

        // Play floating animation
        Animator anim = gremlin.GetComponent<Animator>();
        if (anim)
        {
            anim.SetTrigger("Float"); // Replace with your actual animation name
        }

    }

    public void DeactivateZeroG()
    {
        Debug.Log("ZeroGManager.DeactivateZeroG() CALLED!");

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Debug.Log("Found Player - Restoring Gravity");
            player.DeactivateZeroG();
        }
        else
        {
            Debug.LogError("No Player Found! Gravity Restoration Failed");
        }

        // Re-enable Gremlin spawners and restart their spawn loops
        if (gremlinSpawners != null)
        {
            foreach (var spawner in gremlinSpawners)
            {
                spawner.gameObject.SetActive(true);
                spawner.RestartSpawner(); // Restart the coroutine
                Debug.Log("Gremlin Spawner Reactivated: " + spawner.gameObject.name);
            }
        }
        else
        {
            Debug.LogError("No Gremlin Spawners Found!");
        }
    }


}
