using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Walk -> Run / isWalk <-> isRun

    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons; 

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;

    GameObject nearObject;
    GameObject equipWeapon;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();  // Animator ������ GetComponentInChildren() ���� �ʱ�ȭ.
    }

    private void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
        Swap();
        Interation();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Run");
        jDown = Input.GetButtonDown("Jump");
        //iDown = Input.GetButtonDown("Interation");  <- �־ȵ�??�̤̤�
        iDown = Input.GetKeyDown(KeyCode.E);
        sDown1 = Input.GetKeyDown(KeyCode.Alpha1);  //sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetKeyDown(KeyCode.Alpha2);  //sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetKeyDown(KeyCode.Alpha3);  //sDown3 = Input.GetButtonDown("Swap3");

    }
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;

        if (isSwap)
        {
            moveVec = Vector3.zero;
        }

        transform.position += moveVec * speed * (wDown ? 2.5f : 1f) * Time.deltaTime;   //���׿�����

        anim.SetBool("isWalk", moveVec != Vector3.zero);
        anim.SetBool("isRun", moveVec != Vector3.zero && wDown);

    }
    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }
    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap)  
        {              
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }
    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap)  
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            // ��¦ ����
            rigid.AddForce(Vector3.up * 10f, ForceMode.Impulse);


            Invoke("DodgeOut", 0.6f); // Invoke() �Լ��� �ð��� �Լ� ȣ��
        }
       
    }
    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }
    void Swap()
    {
        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);
                    
            equipWeapon = weapons[weaponIndex];
            equipWeapon.SetActive(true);

            anim.SetTrigger("doSwap");
            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }
    void SwapOut()
    {
        isSwap = false;
    }

    void Interation()
    {
        if (iDown && nearObject != null ) //&& !isJump && !isDodge)
        {
            //if (iDown)
                //Debug.Log("EŰ ����");


            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = other.gameObject;

        Debug.Log(nearObject.name);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
        

    }


}