using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasS_Gear : MonoBehaviour
{
    [SerializeField] ShowThis displayList = ShowThis.weapons;

    [SerializeField] Button[] weaponButtonsList;

    [Header("Current weapon Equipped")]
    [SerializeField] TMP_Text selectedWeaponNameText;
    [SerializeField] TMP_Text selectedWeaponStatsText;
    [SerializeField] Image selectedWeaponImage;

    bool delaySelected = false;
    float delayToShowSelected = 0;

    [SerializeField] int weaponPags = 1;
    [SerializeField] int subPags = 1;
    [SerializeField] int spPags = 1;
    [SerializeField] int actualPage = 1;

    void Start()
    {
        weaponPags = SceneManagerScript.Instance.mainWeapons.Length / weaponButtonsList.Length;
        subPags = SceneManagerScript.Instance.subWeapons.Length / weaponButtonsList.Length;
        // spPags = ....

        weaponPags = Mathf.CeilToInt(weaponPags);
        subPags = Mathf.CeilToInt(subPags);
        spPags = Mathf.CeilToInt(spPags);

        actualPage = 0;

        ChangeDisplayList();
    }

    void Update()
    {
        if (!UI_Manager.Instance.openGear) CloseThis();

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
        for (int i = 0; i < weaponButtonsList.Length; i++)
        {
            weaponButtonsList[i].image.sprite = null;
            weaponButtonsList[i].image.color = new Color(255, 255, 255, 0);
            weaponButtonsList[i].interactable = false;
        }

        switch (displayList)
        {
            case ShowThis.weapons:
                for (int i = 0; i < weaponButtonsList.Length && i < SceneManagerScript.Instance.mainWeapons.Length; i++)
                {
                    if (i + 9 * actualPage == SceneManagerScript.Instance.mainWeapons.Length) break;
                    weaponButtonsList[i].image.sprite = SceneManagerScript.Instance.mainWeapons[i + 9 * actualPage].GetComponent<Weapon>().weaponSprite;
                    weaponButtonsList[i].image.color = new Color(255, 255, 255, 255);
                    weaponButtonsList[i].interactable = true;
                }
                break;
            case ShowThis.subWeapons:
                for (int i = 0; i < weaponButtonsList.Length && i < SceneManagerScript.Instance.subWeapons.Length; i++)
                {
                    if (i + 9 * actualPage == SceneManagerScript.Instance.subWeapons.Length) break;
                    weaponButtonsList[i].image.sprite = SceneManagerScript.Instance.subWeapons[i + 9 * actualPage].GetComponent<SubWeapon>().weaponSprite;
                    weaponButtonsList[i].image.color = new Color(255, 255, 255, 255);
                    weaponButtonsList[i].interactable = true;
                }
                break;
            case ShowThis.specials:
                break;
        }

        UpdateWeaponShowingInfo();
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

    #region Buttons

    public void CloseThisUI()
    {
        UI_Manager.Instance.ToggleNetSettings();
        Destroy(gameObject);
    }

    void CloseThis()
    {
        Destroy(gameObject);
    }

    public void Button_MainW()
    {
        if (displayList != ShowThis.weapons)
        {
            displayList = ShowThis.weapons;
            actualPage = 0;
            ChangeDisplayList();
        }
    }

    public void Button_SubW()
    {
        if (displayList != ShowThis.subWeapons)
        {
            displayList = ShowThis.subWeapons;
            actualPage = 0;
            ChangeDisplayList();
        }
    }

    public void Button_SpecialW()
    {
        if (displayList != ShowThis.specials)
        {
            displayList = ShowThis.specials;
            actualPage = 0;
            ChangeDisplayList();
        }
    }

    public void Button_WeaponSelected(int id)
    {
        id += 9 * actualPage;

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

    public void Change_Page()
    {
        switch (displayList)
        {
            case ShowThis.weapons:
                if (actualPage < weaponPags) actualPage++; else actualPage = 0;
                break;
            case ShowThis.subWeapons:
                if (actualPage < subPags) actualPage++; else actualPage = 0;
                break;
            case ShowThis.specials:
                if (actualPage < spPags) actualPage++; else actualPage = 0;
                break;
        }

        ChangeDisplayList();
    }

    #endregion

    public enum ShowThis
    {
        weapons,
        subWeapons,
        specials
    }
}
