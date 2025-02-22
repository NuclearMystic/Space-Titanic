using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public GameObject optionsPanel; // Assign in Inspector

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider ambienceSlider;

    private void Start()
    {
        // Ensure AudioManager is available
        if (AudioManager.Instance != null)
        {
            // Sync sliders with saved values
            masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            ambienceSlider.value = PlayerPrefs.GetFloat("AmbienceVolume", 1f);

            // Assign listeners to update AudioManager
            masterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
            musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
            ambienceSlider.onValueChanged.AddListener(AudioManager.Instance.SetAmbienceVolume);
        }
    }

    public void ToggleOptionsMenu()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void CloseOptionsMenu()
    {
        optionsPanel.SetActive(false);
    }
}
