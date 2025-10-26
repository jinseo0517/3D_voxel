using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // enum : 열거형 타입 (타입 이름 지정 필요)
    // enum 선언은 중괄호 안에 데이터를 열거하듯이 작성.
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon };
    public Type type;   // 아이템 종류와 값을 저장할 변수 선언
    public int value;

    Rigidbody rigid;
    SphereCollider sphereCollider;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);     // Rotate() 함수로 계속 회전하도록 효과 내기
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }
    }
}