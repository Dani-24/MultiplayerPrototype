using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworking : MonoBehaviour
{
    public int networkID;
    public bool isOwnByThisInstance;
    [SerializeField] List<GameObject> gameObjectsToHideIfNotOwned = new List<GameObject>();

    [SerializeField] AudioListener audioListener;

    private void Awake()
    {
        networkID = Random.Range(0, 999999);
    }

    void Start()
    {
        HideGameObjects(isOwnByThisInstance);
    }

    void Update()
    {
        HideGameObjects(isOwnByThisInstance);
    }

    void HideGameObjects(bool hide)
    {
        for (int i = 0; i < gameObjectsToHideIfNotOwned.Count; i++)
        {
            gameObjectsToHideIfNotOwned[i].SetActive(hide);
        }
        audioListener.enabled = hide;
    }

    public PlayerPackage GetPlayerPck()
    {
        PlayerPackage pPck = new PlayerPackage();

        pPck.teamTag = GetComponent<PlayerStats>().teamTag;
        pPck.moveInput = GetComponent<PlayerMovement>().GetMoveInput();
        pPck.running = GetComponent<PlayerMovement>().GetRunInput();
        pPck.jumping = GetComponent<PlayerMovement>().GetJumpInput();
        pPck.shooting = GetComponent<PlayerArmament>().weaponShooting;
        pPck.shootingSub = GetComponent<PlayerArmament>().subWeaponShooting;
        pPck.camRot = GetComponent<PlayerOrbitCamera>().GetCamRot();

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
    }
}