using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public WeaponSO[] weapon;
    public WeaponSO currentWeapon;
    public Transform firePoint;         //발사위치 (총구)
    Camera cam;

    void Start()
    {
        cam = Camera.main;      //메인 카메라 가져오기
        currentWeapon = weapon[0];
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))    //좌클릭 발사
        {
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentWeapon != null && currentWeapon == weapon[0])
            {
                currentWeapon = weapon[1];
            }
            else if (currentWeapon != null && currentWeapon == weapon[1])
            {
                currentWeapon = weapon[0];

            }
        }
    }
    void Shoot()
    {
        //화면에 마우스 -> 광선(Ray)쏘기
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint;
        targetPoint = ray.GetPoint(50f);
        Vector3 direction = (targetPoint - firePoint.position).normalized;   //방향벡터

        // Projectile생성
        GameObject proj = Instantiate(currentWeapon.weaponPrefab, firePoint.position, Quaternion.LookRotation(direction));
    }
}
