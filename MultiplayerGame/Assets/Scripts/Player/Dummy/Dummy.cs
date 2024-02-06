using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerStats;

public class Dummy : MonoBehaviour
{
    public bool dummyEnabled = true;

    public float dmgReceived = 100.0f;
    float maxHP;

    float contMax;
    public float timeToRegen = 5;

    public string teamTag;

    [SerializeField] GameObject weaponPrefab;

    public bool weaponShooting;

    void Start()
    {
        contMax = timeToRegen;
        maxHP = dmgReceived;
    }

    void Update()
    {
        dummyEnabled = !ConnectionManager.Instance.IsConnected();   // Dummy Disabled when Online

        if (timeToRegen > 0 && dmgReceived != maxHP)
            timeToRegen -= Time.deltaTime;
        else
            ResetCont(true);

        if (!dummyEnabled) return;

        gameObject.tag = teamTag = SceneManagerScript.Instance.GetRivalTag(SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerStats>().teamTag);
        weaponPrefab.GetComponent<Weapon>().teamTag = teamTag;

        weaponShooting = SceneManagerScript.Instance.GetOwnPlayerInstance().GetComponent<PlayerArmament>().weaponShooting;
    }

    void ResetCont(bool regen = false)
    {
        timeToRegen = contMax;
        if (regen) dmgReceived = maxHP;
    }

    public void OnDMGReceive(string playerDmgDealer, float dmg, string damagerName)
    {
        dmgReceived += dmg;
        ResetCont();
    }
}
