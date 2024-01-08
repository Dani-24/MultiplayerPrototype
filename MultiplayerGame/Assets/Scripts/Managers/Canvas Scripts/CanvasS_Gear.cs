using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasS_Gear : MonoBehaviour
{
    [SerializeField] ShowThis displayList = ShowThis.weapons;

    [SerializeField] Button[] weaponsList;

    [Header("Current weapon Equipped")]
    [SerializeField] TMP_Text selectedWeaponNameText;
    [SerializeField] TMP_Text selectedWeaponStatsText;
    [SerializeField] Image selectedWeaponImage;

    bool delaySelected = false;
    float delayToShowSelected = 0;

    void Start()
    {
        ChangeDisplayList();
    }

    void Update()
    {
        if (!UI_Manager.Instance.openGear) CloseThisUI();

        if (delaySelected)
        {
            if (delayToShowSelected < 0)
            {
                UpdateWeaponShowingInfo();
                delaySelected = false;
            }
            delayToShowSelected -= 1;
        }
    }

    void ChangeDisplayList()
    {
        for (int i = 0; i < weaponsList.Length; i++)
        {
            weaponsList[i].image.sprite = null;
        }

        switch (displayList)
        {
            case ShowThis.weapons:
                for (int i = 0; i < weaponsList.Length && i < SceneManagerScript.Instance.mainWeapons.Length; i++)
                {
                    weaponsList[i].image.sprite = SceneManagerScript.Instance.mainWeapons[i].GetComponent<Weapon>().weaponSprite;
                }
                break;
            case ShowThis.subWeapons:
                for (int i = 0; i < weaponsList.Length && i < SceneManagerScript.Instance.subWeapons.Length; i++)
                {
                    weaponsList[i].image.sprite = SceneManagerScript.Instance.subWeapons[i].GetComponent<SubWeapon>().weaponSprite;
                }
                break;
            case ShowThis.specials:
                break;
        }

        UpdateWeaponShowingInfo();
    }

    public void CloseThisUI()
    {
        UI_Manager.Instance.ToggleNetSettings();
        Destroy(gameObject);
    }

    public void Button_MainW()
    {
        if (displayList != ShowThis.weapons)
        {
            displayList = ShowThis.weapons;
            ChangeDisplayList();
        }
    }

    public void Button_SubW()
    {
        if (displayList != ShowThis.subWeapons)
        {
            displayList = ShowThis.subWeapons;
            ChangeDisplayList();
        }
    }

    public void Button_SpecialW()
    {
        if (displayList != ShowThis.specials)
        {
            displayList = ShowThis.specials;
            ChangeDisplayList();
        }
    }

    public void Button_WeaponSelected(int id)
    {
        switch (displayList)
        {
            case ShowThis.weapons:
                if (id < SceneManagerScript.Instance.mainWeapons.Length)
                {
                    SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerArmament>().ChangeWeapon(id);
                }
                break;
            case ShowThis.subWeapons:
                if (id < SceneManagerScript.Instance.subWeapons.Length)
                {
                    SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerArmament>().ChangeSubWeapon(id);
                }
                break;
            case ShowThis.specials:
                break;
        }

        delaySelected = true;
        delayToShowSelected = 10;

        //UpdateWeaponShowingInfo();
    }

    void UpdateWeaponShowingInfo()
    {
        switch (displayList)
        {
            case ShowThis.weapons:

                Weapon info = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerArmament>().currentWeapon.GetComponent<Weapon>();
                selectedWeaponNameText.text = "Current Weapon: " + info.weaponName;
                selectedWeaponImage.sprite = info.weaponSprite;

                selectedWeaponStatsText.text = "Stats \n Damage: " + info.shootDMG + "\n Range: " + info.weaponRange + "\n Cadence: " + info.cadence + "\n RNG: " + info.rng + "\n Cost per bullet (%): " + info.shootCost;

                break;
            case ShowThis.subWeapons:

                SubWeapon _info = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerArmament>().subWeapon.GetComponent<SubWeapon>();
                selectedWeaponNameText.text = "Current Weapon: " + _info.weaponName;
                selectedWeaponImage.sprite = _info.weaponSprite;

                selectedWeaponStatsText.text = "Stats \n Damage: " + _info.dmg + "\n Splash Damage " + _info.splashDmg + "\n Cost per throw (%): " + _info.throwCost;

                break;
            case ShowThis.specials:
                break;
        }

    }

    public enum ShowThis
    {
        weapons,
        subWeapons,
        specials
    }
}
