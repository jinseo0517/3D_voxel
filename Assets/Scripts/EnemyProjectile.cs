using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    // 플레이어에게 줄 피해량
    public int damage = 2;

    // 투사체의 이동 속도
    public float speed = 8f;

    // 투사체가 존재할 최대 시간 (초)
    public float lifeTime = 3f;

    // 투사체의 이동 방향
    private Vector3 moveDir;

    // 투사체의 방향을 설정하는 함수
    public void SetDirection(Vector3 dir)
    {
        moveDir = dir.normalized; // 방향 벡터를 정규화하여 저장
    }

    // 오브젝트가 생성될 때 호출됨
    void Start()
    {
        Destroy(gameObject, lifeTime); // 일정 시간이 지나면 오브젝트 제거
    }

    // 매 프레임마다 호출됨
    void Update()
    {
        transform.position += moveDir * speed * Time.deltaTime; // 방향에 따라 위치 이동
    }

    // 다른 콜라이더와 충돌했을 때 호출됨
    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 "Player" 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // PlayerController 컴포넌트를 가져옴
            PlayerController pc = other.GetComponent<PlayerController>();

            // 컴포넌트가 존재하면 피해를 입힘
            if (pc != null) pc.TakeDamage(damage);

            // 투사체 제거
            Destroy(gameObject);
        }
    }
}