using System.Collections.Generic;
using UnityEngine;
using static UI_Manager;

public class CanvasS_Settings : MonoBehaviour
{
    [SerializeField] SettingsOption option = SettingsOption.Graphics;

    [SerializeField] List<SettingsMenu> menus = new List<SettingsMenu>();

    void Update()
    {
        foreach (var menu in menus)
        {
            if(menu.option == option) menu.panel.SetActive(true); else menu.panel.SetActive(false);
        }

        if (!UI_Manager.Instance.openSettings) CloseSettings();
    }

    void CloseSettings()
    {
        UI_Manager.Instance.openNetSettings = false;
        UI_Manager.Instance.currentCanvasMenu = UI_Manager.GameUIs.Gameplay;
        Destroy(gameObject);
    }

    public void ChangeMenu(int _option)
    {
        option = menus[_option].option;
    }

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

        [HideInInspector]
        public bool activated;
    }
}