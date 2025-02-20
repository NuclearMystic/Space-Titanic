using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    private Light fireLight;

    [Header("Flicker Settings")]
    public float minIntensity = 0.8f; // Minimum light intensity
    public float maxIntensity = 1.2f; // Maximum light intensity
    public float flickerSpeed = 0.1f; // Speed of flickering

    [Header("Color Variation (Optional)")]
    public bool useColorVariation = true;
    public Color baseColor = new Color(1f, 0.5f, 0.2f); // Warm orange
    public float colorVariance = 0.1f; // Small color shifts

    private float targetIntensity;
    private float smoothFlickerTime;

    void Start()
    {
        fireLight = GetComponent<Light>();
        targetIntensity = Random.Range(minIntensity, maxIntensity);
    }

    void Update()
    {
        // Smoothly adjust the intensity over time
        smoothFlickerTime += Time.deltaTime * flickerSpeed;
        if (smoothFlickerTime >= 1f)
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);
            smoothFlickerTime = 0f;
        }
        fireLight.intensity = Mathf.Lerp(fireLight.intensity, targetIntensity, Time.deltaTime * flickerSpeed * 10f);

        // Slightly shift the light color for a more dynamic effect
        if (useColorVariation)
        {
            float r = baseColor.r + Random.Range(-colorVariance, colorVariance);
            float g = baseColor.g + Random.Range(-colorVariance, colorVariance);
            float b = baseColor.b + Random.Range(-colorVariance, colorVariance);
            fireLight.color = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b));
        }
    }
}
