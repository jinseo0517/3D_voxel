using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // enum : ������ Ÿ�� (Ÿ�� �̸� ���� �ʿ�)
    // enum ������ �߰�ȣ �ȿ� �����͸� �����ϵ��� �ۼ�.
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon };
    public Type type;   // ������ ������ ���� ������ ���� ����
    public int value;

    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);     // Rotate() �Լ��� ��� ȸ���ϵ��� ȿ�� ����
    }

}