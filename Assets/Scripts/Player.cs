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
    public Camera followCamera;

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
    bool fDown;
    bool rDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;
    bool isBorder;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;

    GameObject nearObject;
    public Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay;

    void Awake()
    {
        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("BulletCase"), true);
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();  // Animator 변수를 GetComponentInChildren() 으로 초기화.
    }

    private void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Reload();
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
        fDown = Input.GetButton("Fire1");   //마우스왼쪽
        rDown = Input.GetKeyDown(KeyCode.R);
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

        if (isSwap || isReload || !isFireReady)
            moveVec = Vector3.zero;

        if (!isBorder)
        transform.position += moveVec * speed * (wDown ? 2.5f : 1f) * Time.deltaTime;   //삼항연산자

        anim.SetBool("isWalk", moveVec != Vector3.zero);
        anim.SetBool("isRun", moveVec != Vector3.zero && wDown);

    }
    void Turn()
    {
        //1. 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);
        //2. 마우스에 의한 회전
        if (fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
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

    void Attack()
    {
        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap)
        { 
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;

        if (ammo == 0)
            return;

        if (rDown && !isJump && !isDodge && !isSwap && isFireReady) 
        {
            anim.SetTrigger("doReload");  //Reload애니메이션 없음 ㅠㅁㅠ
            isReload = true;

            Invoke("ReloadOut", 1f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
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
                equipWeapon.gameObject.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

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

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, moveVec, 5, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
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