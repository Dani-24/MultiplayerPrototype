using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    public float HP = 100.0f;
    float maxHP;

    float contMax;
    public float timeToRegen = 5;

    void Start()
    {
        contMax = timeToRegen;
        maxHP = HP;
    }

    void Update()
    {
        if (HP < 0) HP = 0;

        if (timeToRegen > 0 && HP != 100)
        {
            timeToRegen -= Time.deltaTime;
        }
        else
        {
            ResetCont(true);
        }
    }

    void ResetCont(bool regen = false)
    {
        timeToRegen = contMax;
        if(regen) HP = maxHP;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet") && HP > 0)
        {
            HP -= other.gameObject.GetComponent<DefaultBullet>().DMG;
            ResetCont();
        }
    }
}
