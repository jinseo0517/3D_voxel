using Cinemachine; // 시네머신 카메라 기능 사용
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // 현재 이동 속도
    private float currentSpeed;

    // 이동 속도 설정값들
    private float stopSpeed = 0f;     // 멈춤 상태 속도
    private float walkSpeed = 5f;     // 걷기 속도
    private float runSpeed = 12f;     // 달리기 속도

    // 점프 파워 및 중력 값
    private float jumpPower = 10f;
    public float gravity = -9.81f;

    // TPS 카메라 및 회전 속도
    public CinemachineVirtualCamera virtualCam;
    public float rotationSpeed = 10f;

    // 카메라의 POV 컴포넌트 (Yaw 값 얻기 위해 사용)
    private CinemachinePOV pov;

    // 캐릭터 컨트롤러 및 속도 벡터
    private CharacterController controller;
    private Vector3 velocity;

    // 바닥에 닿아 있는지 여부
    public bool isGrounded;

    // 현재 달리기 상태인지 여부
    public bool isRunning;

    // 카메라 모드 확인용 스크립트 참조
    public CinemachineSwitcher cinemachineSwitcher;

    public int maxHP = 100;
    private int currentHP;

    public Slider hpSlider;

    void Start()
    {
        // 캐릭터 컨트롤러와 POV 컴포넌트 초기화
        controller = GetComponent<CharacterController>();
        pov = virtualCam.GetCinemachineComponent<CinemachinePOV>();

        currentHP = maxHP;
        hpSlider.value = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            pov.m_HorizontalAxis.Value = transform.eulerAngles.y;
            pov.m_VerticalAxis.Value = 0f;
        }

        // FreeLook 모드가 아닐 때만 이동 속도 설정
        if (cinemachineSwitcher.usingFreeLook == false)
        {
            // 왼쪽 쉬프트 키를 누르면 달리기 모드
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = runSpeed;
                isRunning = true;
            }
            else
            {
                currentSpeed = walkSpeed;
                isRunning = false;
            }
        }

        // 달리기 상태일 때 카메라 FOV 확대 (속도감 연출)
        if (isRunning)
        {
            virtualCam.m_Lens.FieldOfView = Mathf.Lerp(virtualCam.m_Lens.FieldOfView, 65f, Time.deltaTime * 5f);
        }
        else
        {
            virtualCam.m_Lens.FieldOfView = Mathf.Lerp(virtualCam.m_Lens.FieldOfView, 40f, Time.deltaTime * 5f);
        }

        // 바닥 체크 및 낙하 속도 초기화
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 키보드 입력값 가져오기
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 카메라 기준 방향 계산
        Vector3 camForward = virtualCam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = virtualCam.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        // 입력 방향에 따라 이동 방향 계산
        Vector3 move = (camForward * z + camRight * x).normalized;

        // 이동 처리
        controller.Move(move * currentSpeed * Time.deltaTime);

        // FreeLook 모드일 때 이동 및 점프 제한
        if (cinemachineSwitcher.usingFreeLook == true)
        {
            currentSpeed = stopSpeed;
            jumpPower = 0f;
        }
        else
        {
            currentSpeed = walkSpeed;
            jumpPower = 5f;
        }

        // 카메라 Yaw 값 기준으로 플레이어 회전 처리
        float cameraYaw = pov.m_HorizontalAxis.Value;
        Quaternion targetRot = Quaternion.Euler(0f, cameraYaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        // 점프 입력 처리
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpPower;
        }

        // 중력 적용
        velocity.y += gravity * Time.deltaTime;

        // 수직 방향 이동 처리
        controller.Move(velocity * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        hpSlider.value = (float)currentHP / maxHP;

        if (currentHP <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Destroy(gameObject);
    }
}