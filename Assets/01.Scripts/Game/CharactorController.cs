using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharactorController : MonoBehaviour
{
    [Header("Joystick UI")]
    public RectTransform joystickBackground; // 배경
    public RectTransform joystickHandle; // 핸들

    [Header("Player")]
    public NavMeshAgent player; // 움직일 플레이어 오브젝트
    public float maxSpeed = 5f; // 최대 속도

    private Vector2 startTouchPosition;
    private bool isDragging;

    private void Start()
    {
        // 처음에는 조이스틱 UI를 숨김
        joystickBackground.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 터치 시작 지점
            startTouchPosition = Input.mousePosition;
            joystickBackground.position = startTouchPosition;
            joystickBackground.gameObject.SetActive(true);
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentTouchPosition = Input.mousePosition;
            Vector2 direction = currentTouchPosition - startTouchPosition;

            // 최대 이동 반경 (배경 이미지 크기의 절반)
            float maxDistance = joystickBackground.sizeDelta.x * 0.5f;

            // 핸들 이동 제한
            Vector2 clampedDirection = Vector2.ClampMagnitude(direction, maxDistance);
            joystickHandle.anchoredPosition = clampedDirection;

            // 캐릭터 이동 속도 (조이스틱 거리 비례)
            float distanceRatio = clampedDirection.magnitude / maxDistance;
            Vector2 moveDir = clampedDirection.normalized;

            // 이동
            Vector3 move = new Vector3(moveDir.x, moveDir.y, 0) * (maxSpeed * distanceRatio) * Time.deltaTime;

            if (player != null)
                player.Move(move);
        }

        if (Input.GetMouseButtonUp(0))
        {
            // 터치 끝나면 조이스틱 숨기기
            isDragging = false;
            joystickBackground.gameObject.SetActive(false);
            joystickHandle.anchoredPosition = Vector2.zero;
        }
    }
}
