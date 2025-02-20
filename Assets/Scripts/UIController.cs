using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private RectTransform interactionImage; // UI image to show above player
    private Transform target;
    private bool isVisible = false;

    public Slider shipHPSlider;
    public TMP_Text shipTimerText;
    public TMP_Text shipTemp;

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

    void Update()
    {
        if (isVisible && target != null)
        {
            UpdatePromptPosition();
        }
    }

    public void ShowInteractionPrompt(Transform playerTransform)
    {
        target = playerTransform;
        interactionImage.gameObject.SetActive(true);
        isVisible = true;
    }

    public void HideInteractionPrompt()
    {
        interactionImage.gameObject.SetActive(false);
        isVisible = false;
        target = null;
    }

    private void UpdatePromptPosition()
    {
        if (target == null) return;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 1.5f);
        interactionImage.position = screenPosition;
    }

    internal void UpdateRepairMeter(float v)
    {
        throw new NotImplementedException();
    }
}
