using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SubWeapon : MonoBehaviour
{
    public subWeaponType weaponType;

    [Header("Sub Weapon Propierties")]
    public float dmg;
    public float splashDmg;
    public float range;
    public float cooldown;

    [Tooltip("Radius affected by this subWeapon where can kill a player")]
    public float lethalRadius = 1f;

    [Tooltip("Radius affected by this subWeapon where deals splash dmg to a player")]
    public float nonLethalRadius = 1.25f;

    [Tooltip("% from the total ink that shooting once costs")]
    public float throwCost;

    [Header("Debug Info")]
    public bool isThrowingSubWeapon = false;

    public Transform aimingRotation;

    [Header("SubWeapon Prefab")]
    public List<SubWeaponPrefabs> subWeaponsPrefabs = new List<SubWeaponPrefabs>();

    bool chargingSub = false;

    void Update()
    {
        isThrowingSubWeapon = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().subWeaponShooting;

        if(isThrowingSubWeapon)
        {
            ChargeSub();
        }

        if(chargingSub && !isThrowingSubWeapon)
        {
            ThrowSub();
        }
    }

    // Cargar/Apuntar la cosa al apretar
    void ChargeSub()
    {
        chargingSub = true;
    }

    // Tirar la cosa al soltar
    void ThrowSub()
    {
        if (gameObject.GetComponent<PlayerStats>().ink >= throwCost)
        {
            // Funcionalidad de la cosa // Instanciar aqui el weaponType

            GameObject bombToThrow = null;

            // Direction & Position for the new gameObject
            Vector3 aimTo = aimingRotation.rotation.eulerAngles;
            aimTo.x -= 20;

            Vector3 pos = transform.position;
            pos.y += 1.5f;

            // Select Bomb
            switch (weaponType)
            {
                case subWeaponType.Bomb:
                    foreach(SubWeaponPrefabs sub in subWeaponsPrefabs)
                    {
                        if(sub.type == subWeaponType.Bomb)
                        {
                            bombToThrow = Instantiate(sub.prefab, pos, Quaternion.Euler(aimTo));

                            bombToThrow.GetComponent<Bomb>().dmg = dmg;
                            bombToThrow.GetComponent<Bomb>().splashDmg = splashDmg;
                            bombToThrow.GetComponent<Bomb>().range = range;
                            bombToThrow.GetComponent<Bomb>().lethalRadius = lethalRadius;
                            bombToThrow.GetComponent<Bomb>().nonLethalRadius = nonLethalRadius;

                            break;
                        }
                    }
                    break;
                case subWeaponType.FastBomb:
                    foreach (SubWeaponPrefabs sub in subWeaponsPrefabs)
                    {
                        if (sub.type == subWeaponType.FastBomb)
                        {
                            bombToThrow = Instantiate(sub.prefab, pos, Quaternion.Euler(aimTo));

                            break;
                        }
                    }
                    break;
            }

            if (gameObject.tag != "Enemy")
            {
                bombToThrow.tag = "AllyBomb";
            }
            else
            {
                bombToThrow.tag = "EnemyBomb";
            }

            gameObject.GetComponent<PlayerStats>().ink -= throwCost;
        }
        chargingSub = false;
    }

    public enum subWeaponType
    {
        Bomb,
        FastBomb
    }

    [System.Serializable]
    public struct SubWeaponPrefabs
    {
        public subWeaponType type;
        public GameObject prefab;

        public SubWeaponPrefabs(subWeaponType wType, GameObject go)
        {
            type = wType;
            prefab = go;
        }
    }
}