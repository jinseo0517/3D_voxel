using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { Idle, Trace, Attack, RunAway }
    public EnemyState state = EnemyState.Idle;

    public float moveSpeed = 2f;        //이동속도
    public float traceRange = 15f;      //추적시작거리
    public float attackRange = 6f;      //공격시작 거리
    public float attackCooldown = 1.5f;

    public GameObject projectileprefab;     //투사체 프리펩
    public Transform firePoint;             //발사위치

    private Transform player;
    private float lastAttackTime;
    public int maxHP = 5;
    private int currentHP;

    public Slider hpSlider;

    //public int currentHP = 5;      //적 체력

    //private Transform player;           //플레이어 추적용

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        lastAttackTime = -attackCooldown;
        currentHP = maxHP;
        hpSlider.value = 1f;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        

        void TracePlayer()
        {
            //플레이어까지의 방향 구하기
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.LookAt(player.position);
        }

        void AttackPlayer()
        {
            //일정 쿨다운마다 발사
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                ShootProjectile();
            }
        }

        void ShootProjectile()
        {
            if (projectileprefab != null && firePoint != null)
            {
                transform.LookAt(player.position);
                GameObject proj = Instantiate(projectileprefab, firePoint.position, firePoint.rotation);
                EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
                if (ep != null)
                {
                    Vector3 dir = (player.position - firePoint.position).normalized;
                    ep.SetDirection(dir);
                }
            }
        }

        float hpRatio = (float)currentHP / maxHP; // 현재 체력 비율 계산

        switch (state)
        {
            case EnemyState.Idle:
                if (hpRatio <= 0.2f) // 체력이 20% 이하이면 도망
                    state = EnemyState.RunAway;
                else if (dist < traceRange)
                    state = EnemyState.Trace;
                break;

            case EnemyState.Trace:
                if (hpRatio <= 0.2f) // 추적 중에도 체력이 낮으면 도망
                    state = EnemyState.RunAway;
                else if (dist < attackRange)
                    state = EnemyState.Attack;
                else if (dist > traceRange)
                    state = EnemyState.Idle;
                else
                    TracePlayer();
                break;

            case EnemyState.Attack:
                if (hpRatio <= 0.2f) // 공격 중에도 체력이 낮으면 도망
                    state = EnemyState.RunAway;
                else if (dist > attackRange)
                    state = EnemyState.Trace;
                else
                    AttackPlayer();
                break;

            case EnemyState.RunAway:
                RunAwayFromPlayer(); // 도망 실행
                if (dist > traceRange + 5f) // 일정 거리 이상 멀어지면 Idle
                    state = EnemyState.Idle;
                break;
        }

        //Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        //transform.LookAt(lookPos);
    }

    public void TakeDamage( int damage )
    {
        currentHP -= damage;
        hpSlider.value = (float)currentHP / maxHP;
        //Debug.Log($"{currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Destroy(gameObject);
    }
    void RunAwayFromPlayer()
    {
        // 플레이어와 반대 방향으로 이동
        Vector3 dir = (transform.position - player.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.LookAt(transform.position + dir); // 도망 방향 바라보기
    }

   
}
