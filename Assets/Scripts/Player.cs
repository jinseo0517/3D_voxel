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
        anim = GetComponentInChildren<Animator>();  // Animator ������ GetComponentInChildren() ���� �ʱ�ȭ.
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
        //iDown = Input.GetButtonDown("Interation");  <- �־ȵ�??�̤̤�
        fDown = Input.GetButton("Fire1");   //���콺����
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
        {
            moveVec = Vector3.zero;
        }

        transform.position += moveVec * speed * (wDown ? 2.5f : 1f) * Time.deltaTime;   //���׿�����

        anim.SetBool("isWalk", moveVec != Vector3.zero);
        anim.SetBool("isRun", moveVec != Vector3.zero && wDown);

    }
    void Turn()
    {
        //1. Ű���忡 ���� ȸ��
        transform.LookAt(transform.position + moveVec);
        //2. ���콺�� ���� ȸ��
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
            anim.SetTrigger("doReload");  //Reload�ִϸ��̼� ���� �Ф���
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

    private void OnTriggerStay(Collider other)  // ���� �ݶ��̴��� ��� �ִ� ���� ����Ǵ� �Լ�
    {
        if (other.tag == "Weapon")  // ���� ������Ʈ�� �±װ� "Weapon"�� ���
        {
            nearObject = other.gameObject;  // �ش� ���� ������Ʈ�� nearObject�� ����

            if (nearObject != null) // nearObject�� null�� �ƴϸ� �̸��� �ֿܼ� ���
                Debug.Log(nearObject.name);
        }

    }
    private void OnTriggerExit(Collider other)  // ���� �ݶ��̴����� ����� �� ����Ǵ� �Լ�
    {
        if (other.tag == "Weapon")  // ��� ������Ʈ�� �±װ� "Weapon"�� ���
            nearObject = null;  // nearObject�� null�� �ʱ�ȭ (����� �� �̻� ������ ���� ����)
    }


}