using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private RectTransform interactionImage;
    private Transform target;
    private bool isVisible = false;

    public Slider shipHPSlider;
    public TMP_Text shipTimerText;
    public TMP_Text shipTemp;

    public GameObject repairMeter;
    private Slider repairSlider; // Cache for better performance

    public GameObject gameOverScreen; // Game Over UI Object
    public TMP_Text gameOverText; // Text for Win/Lose Message

    public GameObject healthUpArrow;  // Green Up Arrow
    public GameObject healthDownArrow; // Red Down Arrow

    private float lastHealth = 200f; // Store the last health value (initialize as needed)


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ensure UI persists across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        // Cache slider reference to avoid repeated calls
        if (repairMeter != null)
        {
            repairSlider = repairMeter.GetComponentInChildren<Slider>();
        }
    }

    void Update()
    {
        if (isVisible && target != null)
        {
            UpdatePromptPosition();
        }
    }

    public void ShowInteractionPrompt(Transform playerTransform)
    {
        if (interactionImage == null) return;

        target = playerTransform;
        interactionImage.gameObject.SetActive(true);
        isVisible = true;
    }

    public void HideInteractionPrompt()
    {
        if (interactionImage == null) return;

        interactionImage.gameObject.SetActive(false);
        isVisible = false;
        target = null;
    }

    private void UpdatePromptPosition()
    {
        if (target == null || interactionImage == null) return;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 1.5f);
        interactionImage.position = screenPosition;
    }

    public void UpdateRepairMeter(float value)
    {
        if (repairSlider != null)
        {
            repairSlider.value = value;
        }
    }

    public void ShowRepairMeter(bool visible)
    {
        if (repairMeter != null)
        {
            repairMeter.SetActive(visible);
        }
    }

    public void UpdateTimer(string timer)
    {
        if (shipTimerText != null)
        {
            shipTimerText.text = timer;
        }
    }

    public void UpdateHealth(float shipHealth)
    {
        if (shipHPSlider != null)
        {
            shipHPSlider.value = shipHealth;
        }

        // Check if health went up or down
        if (shipHealth > lastHealth)
        {
            healthUpArrow.SetActive(true);
            healthDownArrow.SetActive(false);
        }
        else if (shipHealth < lastHealth)
        {
            healthUpArrow.SetActive(false);
            healthDownArrow.SetActive(true);
        }

        lastHealth = shipHealth; // Store the new value for next check
    }

    public void ShowGameOverScreen(bool playerWon)
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);

            if (gameOverText != null)
            {
                gameOverText.text = playerWon ? "Mission Success! The ship survived!" : "Mission Failed! The ship was lost.";
            }
        }
    }
}
