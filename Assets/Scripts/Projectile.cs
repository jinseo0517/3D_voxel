using UnityEngine;

public class Projectile : MonoBehaviour
{
    public WeaponSO weapon;
    public float speed;
    public float lifeTime;
    public int damage;


    void Start()
    {
        Destroy(gameObject, lifeTime);
        damage = weapon.weaponDamage;
        speed = weapon.fireSpeed;
        lifeTime = weapon.lifeTime;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}