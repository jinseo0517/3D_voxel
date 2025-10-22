using UnityEngine;           // Unity 기본 기능 사용
using Cinemachine;          // Cinemachine 카메라 기능 사용

public class CinemachineSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCam;   // TPS용 시네머신 카메라
    public CinemachineFreeLook freeLookCam;       // FreeLook용 시네머신 카메라
    public bool usingFreeLook = false;            // 현재 FreeLook 모드인지 여부

    // 게임 시작 시 호출되는 초기화 함수
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 잠금

        // 기본 카메라 우선순위 설정: TPS 카메라가 활성화됨
        virtualCam.Priority = 10;
        freeLookCam.Priority = 0;
    }

    // 매 프레임마다 호출되는 업데이트 함수
    void Update()
    {

        // ESC 키를 누르면 커서 잠금/해제 토글
        if (Input.GetKeyDown(KeyCode.Escape)) { ToggleCursor(); }

        // 커서가 잠금 상태가 아니면 카메라 전환을 막음
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // 마우스 오른쪽 버튼 클릭 시 카메라 모드 전환
        if (Input.GetMouseButtonDown(1))
        {
            usingFreeLook = !usingFreeLook; // FreeLook 상태 토글

            if (usingFreeLook)
            {
                // FreeLook 카메라 활성화
                freeLookCam.Priority = 20;
                virtualCam.Priority = 0;
            }
            else
            {
                // TPS 카메라 활성화
                virtualCam.Priority = 20;
                freeLookCam.Priority = 0;
            }
        }
    }

    // 커서 잠금/해제를 전환하는 함수
    void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // 커서 해제 및 표시
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 커서 잠금 및 숨김
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}