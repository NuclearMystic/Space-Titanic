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

    }

    public void ActivateZeroG()
    {
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
            StartCoroutine(FloatAndDespawnGremlin(gremlin));
        }
    }

    private IEnumerator FloatAndDespawnGremlin(Gremlin gremlin)
    {
        // Disable the Gremlin's normal AI behavior
        gremlin.enabled = false;

        // Play floating animation
        Animator anim = gremlin.GetComponent<Animator>();
        if (anim)
        {
            anim.Play("Gremlin_Floating"); // Replace with your actual animation name
        }

        // Play giggling sound effect
        AudioSource audio = gremlin.GetComponent<AudioSource>();
        if (audio)
        {
            audio.Play(); // Assumes you attached the giggle sound to their AudioSource
        }

        // Make the Gremlin slowly float up
        float startTime = Time.time;
        while (Time.time - startTime < despawnTime)
        {
            gremlin.transform.position += Vector3.up * floatAwaySpeed * Time.deltaTime;
            yield return null;
        }

        // Destroy the Gremlin after floating away
        Destroy(gremlin.gameObject);
    }

    public void DeactivateZeroG()
    {
        // Re-enable spawners
        foreach (var spawner in gremlinSpawners)
        {
            spawner.gameObject.SetActive(true);
        }
    }
}
