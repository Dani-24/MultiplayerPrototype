using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class CanvasS_Settings : MonoBehaviour
{
    [SerializeField][Tooltip("Button selected once the menu is open")] Button selectedButton;

    [SerializeField] SettingsOption option = SettingsOption.Graphics;

    [SerializeField] List<SettingsMenu> menus = new List<SettingsMenu>();

    [SerializeField] TMP_Dropdown screenModeDropdown;
    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] TMP_Dropdown resolutionDropdown;

    public Resolution[] resolutions;
    public List<string> resolutionOptions = new List<string>();

    [Header("UI Elements to update")]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider masterVSlider;
    [SerializeField] Slider musicVSlider;
    [SerializeField] Slider sfxVSlider;

    [SerializeField] Slider mouseXSlider;
    [SerializeField] Slider mouseYSlider;
    [SerializeField] Slider gamepadXSlider;
    [SerializeField] Slider gamepadYSlider;

    [SerializeField] TMP_Text mouseXText;
    [SerializeField] TMP_Text mouseYText;
    [SerializeField] TMP_Text gamepadXText;
    [SerializeField] TMP_Text gamepadYText;

    [SerializeField] TMP_InputField changeName;
    [SerializeField] TMP_Text namePlaceholder;

    void Start()
    {
        selectedButton.Select();

        // --- Quality ---
        qualityDropdown.value = QualitySettings.GetQualityLevel();

        // --- Screen Mode ---
        screenModeDropdown.value = ((int)Screen.fullScreenMode);

        // --- Resolution ---
        resolutions = Screen.resolutions;

        int currentOption = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " at " + resolutions[i].refreshRateRatio + "Hz";
            resolutionOptions.Add(option);

            if (resolutions[i].width == 1280 && resolutions[i].height == 720)
            {
                currentOption = i;
            }
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentOption;
        resolutionDropdown.RefreshShownValue();

        // --- Sens ---
        mouseXSlider.value = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().mouseSens.x;
        mouseYSlider.value = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().mouseSens.y;
        gamepadXSlider.value = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().gamepadSens.x;
        gamepadYSlider.value = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().gamepadSens.y;

        mouseXText.text = mouseXSlider.value.ToString("F3");
        mouseYText.text = (mouseYSlider.value * 100).ToString("F3");
        gamepadXText.text = gamepadXSlider.value.ToString("F3");
        gamepadYText.text = (gamepadYSlider.value * 100).ToString("F3");

        // --- Audio ---
        // Con un Save Manager guardar el volumen y aqui aplicarlo al Slider.value

        // --- User ---
        namePlaceholder.text = UI_Manager.Instance.userName;
    }

    void Update()
    {
        foreach (var menu in menus)
        {
            if (menu.option == option) menu.panel.SetActive(true); else menu.panel.SetActive(false);
        }

        if (!UI_Manager.Instance.openSettings) CloseThis();
    }

    public void CloseSettings()
    {
        if(changeName.text != UI_Manager.Instance.userName && changeName.text != "") UI_Manager.Instance.userName = changeName.text;

        UI_Manager.Instance.ToggleNetSettings();
        Destroy(gameObject);
    }

    void CloseThis()
    {
        if (changeName.text != UI_Manager.Instance.userName && changeName.text != "") UI_Manager.Instance.userName = changeName.text;
        Destroy(gameObject);
    }

    #region Settings

    public void ChangeMenu(int _option)
    {
        option = menus[_option].option;
    }

    public void SetQuality(int quality)
    {
        QualitySettings.SetQualityLevel(quality);
    }

    public void SetWindowMode(int mode)
    {
        switch (mode)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }

    public void SetResolution(int res)
    {
        Resolution resolution = resolutions[res];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    public void ChangeMasterVolume(float volume)
    {
        audioMixer.SetFloat("masterV", Mathf.Log10(volume) * 20);
    }
    public void ChangeMusicVolume(float volume)
    {
        audioMixer.SetFloat("musicV", Mathf.Log10(volume) * 20);
    }
    public void ChangeSFXVolume(float volume)
    {
        audioMixer.SetFloat("sfxV", Mathf.Log10(volume) * 20);
    }

    public void SetMouseXSens(float value)
    {
        SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().ChangeSens(true, true, value);
        mouseXText.text = mouseXSlider.value.ToString("F3");
    }
    public void SetMouseYSens(float value)
    {
        SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().ChangeSens(true, false, value);
        mouseYText.text = (mouseYSlider.value * 100).ToString("F3");
    }
    public void SetGamepadXSens(float value)
    {
        SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().ChangeSens(false, true, value);
        gamepadXText.text = gamepadXSlider.value.ToString("F3");
    }
    public void SetGamepadYSens(float value)
    {
        SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerOrbitCamera>().ChangeSens(false, false, value);
        gamepadYText.text = (gamepadYSlider.value * 100).ToString("F3");
    }

    public void SaveButton()
    {
        SceneManagerScript.Instance.SaveData();
    }

    public void LoadButton()
    {
        SceneManagerScript.Instance.LoadData();
    }

    public void DeleteDataButton()
    {
        SceneManagerScript.Instance.DeleteSavedData();
    }

    #endregion

    public enum SettingsOption
    {
        Graphics,
        Audio,
        Input,
        User
    }

    [System.Serializable]
    public class SettingsMenu
    {
        public SettingsMenu(SettingsOption option, GameObject panel)
        {
            this.option = option;
            this.panel = panel;
        }

        public SettingsOption option;
        public GameObject panel;
    }
}