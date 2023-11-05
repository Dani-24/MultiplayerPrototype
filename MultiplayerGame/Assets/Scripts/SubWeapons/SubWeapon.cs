using System.Collections.Generic;
using UnityEngine;

public class SubWeapon : MonoBehaviour
{
    public subWeaponType weaponType;

    [Header("Sub Weapon Propierties")]
    public float dmg;
    public float splashDmg;
    public float range;
    public float cooldown;

    [Tooltip("Radius affected by this subWeapon")]
    public float radius;

    [Tooltip("% from the total ink that shooting once costs")]
    public float throwCost;

    [Header("Debug Info")]
    public bool isThrowingSubWeapon = false;

    List<GameObject> subWeapons = new List<GameObject>();

    [Header("SubWeapon Prefab")]
    public GameObject subWeaponGO;

    bool chargingSub = false;

    void Start()
    {
        
    }

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

            // Costar tinta
            gameObject.GetComponent<PlayerStats>().ink -= throwCost;
        }
        chargingSub = false;
    }

    public enum subWeaponType
    {
        Bomb,
        FastBomb
    }
}
