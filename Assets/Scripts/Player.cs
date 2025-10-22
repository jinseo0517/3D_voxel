using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;

    bool isJump;
    bool isDodge;

    Vector3 moveVec;

    Rigidbody rigid;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();  // Animator 변수를 GetComponentInChildren() 으로 초기화.
    }

    private void Update()
    {
        GetInput();
        Move();
        Turn();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Run");
        jDown = Input.GetButtonDown("Jump");
    }
    void Move()
    {

        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        transform.position += moveVec * speed * (wDown ? 2.5f : 1f) * Time.deltaTime;   //삼항연산자

        anim.SetBool("isWalk", moveVec != Vector3.zero);
        anim.SetBool("isRun", moveVec != Vector3.zero && wDown);

    }
    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }
}