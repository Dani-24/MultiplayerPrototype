using System.Collections.Generic;
using UnityEngine;

public class SubWeapon : MonoBehaviour
{
    [HideInInspector] public string teamTag;

    #region Propierties

    [Header("Sub Weapon Propierties")]
    [Tooltip("DMG dealt in lethal Radius")]
    public float dmg;
    [Tooltip("DMG dealt in non-lethal Radius")]
    public float splashDmg;
    [Tooltip("Projectile speed")]
    public float speed;
    [Tooltip("Throw range")]
    public float range;
    [Tooltip("Cooldown beetwen this bomb and the last one")]
    public float cooldown;

    [Tooltip("If False an animator is required")]
    public bool instantExplosion = false;

    [Tooltip("Radius affected by this subWeapon where can kill a player")]
    public float lethalRadius = 1f;

    [Tooltip("Radius affected by this subWeapon where deals splash dmg to a player")]
    public float nonLethalRadius = 1.25f;

    [Header("Aim offsets")]
    public float aimYOffset;

    #endregion

    #region Ink & Painting

    [Tooltip("Radius painted by this subWeapon")]
    [SerializeField] protected float paintRadius = 1.1f;
    [SerializeField] protected float strength = 1;
    [SerializeField] protected float hardness = 1;

    [Tooltip("% from the total ink that shooting once costs")]
    public float throwCost;

    #endregion

    #region Other

    [SerializeField] protected List<Renderer> rend = new List<Renderer>();

    [SerializeField] protected float minYaxis = -20;

    protected float customGravity;

    #endregion
}