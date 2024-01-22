using Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNetworking : MonoBehaviour
{
    public int networkID;
    public bool isOwnByThisInstance;
    [SerializeField] List<GameObject> gameObjectsToHideIfNotOwned = new List<GameObject>();
    [Tooltip("To disable")][SerializeField] CinemachineBrain cameraBrain;

    [Header("Nametags")]
    [SerializeField] GameObject nametagCanvas;
    public TMP_Text nameTagText;
    [SerializeField] RectTransform recTrans;

    private void Awake()
    {
        networkID = Random.Range(0, 999999);
        cameraBrain.enabled = isOwnByThisInstance;
    }

    void Start()
    {
        HideGameObjects(isOwnByThisInstance);
    }

    void Update()
    {
        HideGameObjects(isOwnByThisInstance);

        // Canvas target Camera
        if (Camera.main != null)
        {
            recTrans.rotation = Quaternion.LookRotation((Camera.main.transform.position - recTrans.position));
            recTrans.Rotate(Vector3.up, 180);
        }
    }

    void HideGameObjects(bool hide)
    {
        for (int i = 0; i < gameObjectsToHideIfNotOwned.Count; i++)
        {
            gameObjectsToHideIfNotOwned[i].SetActive(hide);
        }

        nametagCanvas.SetActive(!hide);
    }

    public PlayerPackage GetPlayerPck()
    {
        PlayerPackage pPck = new PlayerPackage
        {
            teamTag = GetComponent<PlayerStats>().teamTag,
            running = GetComponent<PlayerMovement>().GetRunInput(),
            jumping = GetComponent<PlayerMovement>().GetJumpInput(),
            camRot = GetComponent<PlayerOrbitCamera>().GetCamRot(),
            position = transform.position,
            rotation = GetComponent<PlayerMovement>().playerBody.transform.rotation,
            wpRNG = GetComponent<PlayerArmament>().weaponRngState,
            netID = networkID,
            inputEnabled = GetComponent<PlayerStats>().playerInputEnabled,
            mainWeapon = GetComponent<PlayerArmament>().currentWeaponId,
            subWeapon = GetComponent<PlayerArmament>().currentSubWeaponId
        };

        if (GetComponent<PlayerArmament>().currentWeapon != null)
        {
            if (GetComponent<PlayerStats>().ink >= GetComponent<PlayerArmament>().currentWeapon.GetComponent<Weapon>().actualBulletCost)
                pPck.shooting = GetComponent<PlayerArmament>().weaponShooting;
            else
                pPck.shooting = false;
        }

        if (GetComponent<PlayerStats>().ink >= GetComponent<PlayerArmament>().subWeapon.GetComponent<SubWeapon>().throwCost)
            pPck.shootingSub = GetComponent<PlayerArmament>().subWeaponShooting;
        else
            pPck.shootingSub = false;

        return pPck;
    }

    public void SetPlayerInfoFromPck(PlayerPackage pck)
    {
        if (GetComponent<PlayerStats>().teamTag != pck.teamTag) GetComponent<PlayerStats>().ChangeTag(pck.teamTag);

        GetComponent<PlayerMovement>().SetRunInput(pck.running);
        GetComponent<PlayerMovement>().SetJumpInput(pck.jumping);
        GetComponent<PlayerArmament>().SetFire(pck.shooting);
        GetComponent<PlayerArmament>().SetSubFire(pck.shootingSub);
        GetComponent<PlayerOrbitCamera>().SetCamRot(pck.camRot);
        GetComponent<PlayerMovement>().SetPosition(pck.position);
        GetComponent<PlayerMovement>().SetRotation(pck.rotation);
        GetComponent<PlayerArmament>().weaponRngState = pck.wpRNG;

        GetComponent<PlayerArmament>().ChangeWeapon(pck.mainWeapon);
        GetComponent<PlayerArmament>().ChangeSubWeapon(pck.subWeapon);

        if (!pck.inputEnabled) nameTagText.color = Color.grey; else nameTagText.color = Color.white;

        nameTagText.text = pck.userName;
    }
}