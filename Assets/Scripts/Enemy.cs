using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;
    public Transform target;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;

    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        nav.angularSpeed = 120f;
        nav.stoppingDistance = 1.0f;
        nav.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        nav.autoBraking = false;
    }


    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.isKinematic = true; // NavMeshAgent가 움직임을 제어하도록 설정

        //mat = GetComponentsInChildren<MeshRenderer>().material;
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            nav = GetComponent<NavMeshAgent>();
            mat = renderer.material;
            // 만약 material이 없으면 새로 생성해서 넣기
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                renderer.material = mat;
            }
        }

    }
    void Update()
    {
        if(target == null || nav == null) return;

        if (!nav.isOnNavMesh)
        {
            Debug.LogWarning(" 적이 NavMesh 위에 있지 않음!");
            return;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(target.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            nav.SetDestination(hit.position); // NavMesh 위 좌표로 보정해서 이동
            Debug.DrawLine(transform.position, hit.position, Color.red);
        }
        else
        {
            Debug.LogWarning(" 플레이어가 NavMesh 위에 없음!");
        }



    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon Weapon = other.GetComponent<Weapon>();
            curHealth -= Weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            
            StartCoroutine(OnDamage(reactVec, false));

        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec, false));
        }
    }
    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));

    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in renderers)
        {
            if (r.material == null)
            {
                r.material = new Material(Shader.Find("Standard"));
            }
            r.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            foreach (MeshRenderer r in renderers)
            {
                r.material.color = Color.white;
            }
        }
        else
        {
            foreach (MeshRenderer r in renderers)
            {
                r.material.color = Color.gray;
            }

            gameObject.layer = 14;
        
            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);

            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);

            }

            Destroy(gameObject, 4);
        }
    }
}
