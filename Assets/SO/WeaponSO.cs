using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapon/NewWeapon")]
public class WeaponSO : ScriptableObject
{
    public GameObject weaponPrefab;
    [Range(1, 100)] public int weaponDamage;
    public float fireSpeed = 0f;
    public float lifeTime = 2f;
}
