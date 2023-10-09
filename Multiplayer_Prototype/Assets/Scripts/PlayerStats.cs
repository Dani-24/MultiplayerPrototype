using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float HP = 100.0f;
    float maxHP;

    public bool isDead = false;

    private CharacterController controller;

    [SerializeField]
    Vector3 spawnPos = Vector3.zero;

    void Start()
    {
        maxHP = HP;

        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if(transform.position.y < -20 || HP <= 0) isDead = true;

        if (isDead)
        {
            controller.enabled = false;
            transform.position = spawnPos;
            transform.rotation = Quaternion.Euler(Vector3.zero);
            controller.enabled = true;

            isDead = false;
            HP = maxHP;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            HP -= other.gameObject.GetComponent<Bullet>().DMG;
        }
    }
}
