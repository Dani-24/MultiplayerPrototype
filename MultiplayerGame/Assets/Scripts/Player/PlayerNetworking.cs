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
    [SerializeField] TMP_Text nameTagText;
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
        PlayerPackage pPck = new PlayerPackage();

        pPck.teamTag = GetComponent<PlayerStats>().teamTag;
        pPck.moveInput = GetComponent<PlayerMovement>().GetMoveInput();
        pPck.running = GetComponent<PlayerMovement>().GetRunInput();
        pPck.jumping = GetComponent<PlayerMovement>().GetJumpInput();

        if (GetComponent<PlayerStats>().ink >= GetComponent<PlayerArmament>().currentWeapon.GetComponent<Weapon>().shootCost)
            pPck.shooting = GetComponent<PlayerArmament>().weaponShooting;
        else
            pPck.shooting = false;

        if (GetComponent<PlayerStats>().ink >= GetComponent<PlayerArmament>().subWeapon.GetComponent<SubWeapon>().throwCost)
            pPck.shootingSub = GetComponent<PlayerArmament>().subWeaponShooting;
        else
            pPck.shootingSub = false;

        pPck.camRot = GetComponent<PlayerOrbitCamera>().GetCamRot();
        pPck.position = transform.position;
        pPck.rotation = GetComponent<PlayerMovement>().playerBody.transform.rotation;
        pPck.wpRNG = GetComponent<PlayerArmament>().weaponRngState;

        pPck.netID = networkID;

        return pPck;
    }

    public void SetPlayerInfoFromPck(PlayerPackage pck)
    {
        if (GetComponent<PlayerStats>().teamTag != pck.teamTag) GetComponent<PlayerStats>().ChangeTag(pck.teamTag);

        GetComponent<PlayerMovement>().SetMoveInput(pck.moveInput);
        GetComponent<PlayerMovement>().SetRunInput(pck.running);
        GetComponent<PlayerMovement>().SetJumpInput(pck.jumping);
        GetComponent<PlayerArmament>().SetFire(pck.shooting);
        GetComponent<PlayerArmament>().SetSubFire(pck.shootingSub);
        GetComponent<PlayerOrbitCamera>().SetCamRot(pck.camRot);
        GetComponent<PlayerMovement>().SetPosition(pck.position);
        GetComponent<PlayerMovement>().SetRotation(pck.rotation);
        GetComponent<PlayerArmament>().weaponRngState = pck.wpRNG;

        nameTagText.text = pck.userName;
    }
}