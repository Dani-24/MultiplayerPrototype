using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    public float HP = 100.0f;
    float maxHP;

    float contMax = 5;
    float cont;

    void Start()
    {
        maxHP = HP;
        ResetCont();
    }

    void Update()
    {
        if (HP < 0) HP = 0;

        if (cont > 0)
        {
        cont -= Time.deltaTime;
        }
        else
        {
            ResetCont();
        }
    }

    void ResetCont()
    {
        cont = contMax;
        HP = maxHP;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet") && HP > 0)
        {
            HP -= other.gameObject.GetComponent<Bullet>().DMG;
        }
    }
}
