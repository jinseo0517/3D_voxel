using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("Referencess")]
    public Transform orientation;   // 플레이어의 방향 기준이 되는 Transform
    public Transform player;        // 플레이어의 위치를 참조하는 Transform
    public Transform playerOBJ;     // 실제 회전시킬 플레이어 오브젝트
    public Rigidbody rb;            // 플레이어의 Rigidbody (현재 코드에서는 사용되지 않음)

    public float rotationSpeed;     // 플레이어 회전 속도

    private void Start()
    {
        // 게임 시작 시 마우스 커서를 잠그고 숨김
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // 카메라 기준으로 플레이어 방향 벡터 계산 (Y축 고정)
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);

        // orientation 오브젝트를 플레이어 방향으로 정렬
        orientation.forward = viewDir.normalized;

        // 키보드 입력값 가져오기
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 입력 방향 벡터 계산 (카메라 기준)
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // 입력 방향이 있을 경우, 플레이어 오브젝트를 해당 방향으로 부드럽게 회전
        if (inputDir != Vector3.zero)
            playerOBJ.forward = Vector3.Slerp(playerOBJ.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
    }
}