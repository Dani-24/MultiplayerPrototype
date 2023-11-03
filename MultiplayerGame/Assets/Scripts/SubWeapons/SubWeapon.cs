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

    void Start()
    {
        
    }

    void Update()
    {
        isThrowingSubWeapon = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().subWeaponShooting;

        if(isThrowingSubWeapon)
        {
            ThrowSub();
        }
    }

    void ThrowSub()
    {
        if (gameObject.GetComponent<PlayerStats>().ink >= throwCost)
        {

            // Funcionalidad de la cosa // Instanciar aqui el weaponType

            // Costar tinta
            gameObject.GetComponent<PlayerStats>().ink -= throwCost;
        }
    }

    public enum subWeaponType
    {
        Bomb,
        FastBomb
    }
}
