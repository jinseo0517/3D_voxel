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
    public GameObject[] grenades;
    public int hasGrenades;

    public int ammo;
    public int coin;
    public int health;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

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
    public GameObject equipWeapon;
    int equipWeaponIndex = -1;

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
        //iDown = Input.GetButtonDown("Interation");  <- 왜안됨??ㅜㅜㅜ
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

        transform.position += moveVec * speed * (wDown ? 2.5f : 1f) * Time.deltaTime;   //삼항연산자

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

            // 살짝 띄우기
            rigid.AddForce(Vector3.up * 10f, ForceMode.Impulse);


            Invoke("DodgeOut", 0.6f); // Invoke() 함수로 시간차 함수 호출
        }
       
    }
    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }
    
    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
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
                //Debug.Log("E키 눌림");


            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
                    break;
            }
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)  // 무기 콜라이더에 닿아 있는 동안 실행되는 함수
    {
        if (other.tag == "Weapon")  // 닿은 오브젝트의 태그가 "Weapon"일 경우
        {
            nearObject = other.gameObject;  // 해당 무기 오브젝트를 nearObject에 저장

            if (nearObject != null) // nearObject가 null이 아니면 이름을 콘솔에 출력
                Debug.Log(nearObject.name);
        }

    }
    private void OnTriggerExit(Collider other)  // 무기 콜라이더에서 벗어났을 때 실행되는 함수
    {
        if (other.tag == "Weapon")  // 벗어난 오브젝트의 태그가 "Weapon"일 경우
            nearObject = null;  // nearObject를 null로 초기화 (무기와 더 이상 가까이 있지 않음)
    }


}