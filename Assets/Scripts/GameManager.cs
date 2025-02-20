using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float timer = 600f; // 10 minutes (600 seconds)
    private int shipHealth = 200; // Starting ship health
    private int maxShipHealth = 200; // Max health cap
    private bool gameOver = false;

    [Header("Ship Systems")]
    public GameObject[] boilers; // Array for Boilers
    public GameObject[] furnaces; // Array for Furnaces
    public GameObject gravityGenerator; // Single Gravity Generator

    private List<BreakableObject> boilerObjects;
    private List<BreakableObject> furnaceObjects;
    private BreakableObject gravityObject;

    [Header("UI References")]
    public GameObject boilerIconNormal;
    public GameObject boilerIconBroken;
    public GameObject furnaceIconNormal;
    public GameObject furnaceIconBroken;
    public GameObject gravityIconNormal;
    public GameObject gravityIconBroken;

    private float damageTickRate = 1.0f; // How often damage is applied
    private float nextDamageTime = 0f;

    private float healTickRate = 2.0f; // Healing happens every 2 seconds
    private float nextHealTime = 0f;
    private int healAmount = 1; // Regeneration amount (half damage)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persistent singleton
        }
        else
        {
            Destroy(gameObject);
        }

        CacheBreakableObjects();
    }

    void Update()
    {
        if (gameOver) return;

        if (timer > 0)
        {
            timer -= Time.deltaTime;
            UIController.Instance.UpdateTimer(GetTimer());

            if (timer < 0) timer = 0; // Prevent negative timer
        }

        UpdateSystemIcons();
        ApplyStackingDamage();
        ApplyHealthRegeneration();
        CheckGameOver();
    }

    private void CacheBreakableObjects()
    {
        boilerObjects = new List<BreakableObject>();
        furnaceObjects = new List<BreakableObject>();

        foreach (GameObject boiler in boilers)
        {
            BreakableObject breakable = boiler.GetComponent<BreakableObject>();
            if (breakable != null) boilerObjects.Add(breakable);
        }

        foreach (GameObject furnace in furnaces)
        {
            BreakableObject breakable = furnace.GetComponent<BreakableObject>();
            if (breakable != null) furnaceObjects.Add(breakable);
        }

        if (gravityGenerator != null)
        {
            gravityObject = gravityGenerator.GetComponent<BreakableObject>();
        }
    }

    private void UpdateSystemIcons()
    {
        bool isAnyBoilerBroken = boilerObjects.Exists(b => b.state == BreakableObject.ObjectState.Broken);
        bool isAnyFurnaceBroken = furnaceObjects.Exists(f => f.state == BreakableObject.ObjectState.Broken);
        bool isGravityBroken = gravityObject != null && gravityObject.state == BreakableObject.ObjectState.Broken;

        // Update boiler icons
        boilerIconNormal.SetActive(!isAnyBoilerBroken);
        boilerIconBroken.SetActive(isAnyBoilerBroken);

        // Update furnace icons
        furnaceIconNormal.SetActive(!isAnyFurnaceBroken);
        furnaceIconBroken.SetActive(isAnyFurnaceBroken);

        // Update gravity generator icon
        gravityIconNormal.SetActive(!isGravityBroken);
        gravityIconBroken.SetActive(isGravityBroken);
    }

    private void ApplyStackingDamage()
    {
        int brokenSystems = GetBrokenSystemCount();
        if (brokenSystems > 0 && Time.time >= nextDamageTime)
        {
            TakeDamage(brokenSystems); // More broken systems = more damage
            nextDamageTime = Time.time + damageTickRate;
        }
    }

    private void ApplyHealthRegeneration()
    {
        if (GetBrokenSystemCount() == 0 && shipHealth < maxShipHealth && Time.time >= nextHealTime)
        {
            HealShip(healAmount);
            nextHealTime = Time.time + healTickRate;
        }
    }

    private int GetBrokenSystemCount()
    {
        int count = 0;
        count += boilerObjects.FindAll(b => b.state == BreakableObject.ObjectState.Broken).Count;
        count += furnaceObjects.FindAll(f => f.state == BreakableObject.ObjectState.Broken).Count;
        if (gravityObject != null && gravityObject.state == BreakableObject.ObjectState.Broken) count++;
        return count;
    }

    private void CheckGameOver()
    {
        if (shipHealth <= 0)
        {
            gameOver = true;
            UIController.Instance.ShowGameOverScreen(false); // Lose condition
        }
        else if (timer <= 0)
        {
            gameOver = true;
            UIController.Instance.ShowGameOverScreen(true); // Win condition
        }
    }

    public void TakeDamage(int damage)
    {
        if (gameOver) return;

        shipHealth -= damage;
        if (shipHealth < 0) shipHealth = 0;

        UIController.Instance.UpdateHealth(shipHealth); // UIController updates health display
    }

    public void HealShip(int amount)
    {
        if (gameOver) return;

        shipHealth += amount;
        if (shipHealth > maxShipHealth) shipHealth = maxShipHealth;

        UIController.Instance.UpdateHealth(shipHealth);
    }

    public string GetTimer()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
