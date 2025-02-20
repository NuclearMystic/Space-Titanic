using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float timer = 600f; // 10 minutes (600 seconds)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this persistent if needed
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetTimer()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime; // Countdown
            UIController.Instance.UpdateTimer(GetTimer());
            if (timer < 0)
            {
                timer = 0; // Ensure timer never goes negative
            }
        }
    }
}
