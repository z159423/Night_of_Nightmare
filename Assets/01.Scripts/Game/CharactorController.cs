using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;
using UnityEditor;

public class CharactorController : MonoBehaviour
{
    [Header("Joystick UI")]
    public RectTransform joystickBackground; // 배경
    public RectTransform joystickHandle; // 핸들

    [Header("Player")]
    public NavMeshAgent player; // 움직일 플레이어 오브젝트
    public PlayerCharactor playerCharactor;
    public float maxSpeed = 5f; // 최대 속도

    private Vector2 startTouchPosition;
    private bool isDragging;

    private bool touch = false;

    private CinemachineVirtualCamera mapVirtualCamera;

    private void Start()
    {
        // 처음에는 조이스틱 UI를 숨김
        joystickBackground.gameObject.SetActive(false);

        this.SetListener(GameObserverType.Game.OnActivePlayerBed, () =>
        {
            touch = false;
            if (isDragging)
            {
                // 드래그 중이면 강제로 드래그 종료 처리
                isDragging = false;
                joystickBackground.gameObject.SetActive(false);
                joystickHandle.anchoredPosition = Vector2.zero;
                playerCharactor.OnMoveStop();
            }
            else
            {
                // 드래그 중이 아니어도 UI 숨김 및 이동 중지
                joystickBackground.gameObject.SetActive(false);
                playerCharactor.OnMoveStop();
            }
        });

        mapVirtualCamera = Managers.Camera.cameras[Define.GameMode.Map];
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            touch = true;
        }

        if (Managers.Camera.currentMapCameraMode == CameraManager.MapCameraMode.Player)
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 currentTouchPosition = Input.mousePosition;
                Vector2 direction = currentTouchPosition - startTouchPosition;

                // 드래그 거리가 임계값 이상일 때만 isDragging 활성화
                float dragThreshold = 10f;
                if (!isDragging && direction.magnitude > dragThreshold)
                {
                    isDragging = true;
                    joystickBackground.position = startTouchPosition;
                    joystickBackground.gameObject.SetActive(true);
                }

                if (isDragging)
                {
                    float maxDistance = joystickBackground.sizeDelta.x * 0.5f;
                    Vector2 clampedDirection = Vector2.ClampMagnitude(direction, maxDistance);
                    joystickHandle.anchoredPosition = clampedDirection;

                    float distanceRatio = clampedDirection.magnitude / maxDistance;
                    Vector2 moveDir = clampedDirection.normalized;

                    Vector3 move = new Vector3(moveDir.x, moveDir.y, 0) * (maxSpeed * distanceRatio) * Time.deltaTime;

                    if (player != null)
                    {
                        player.Move(move);
                        playerCharactor.OnMove();
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                joystickBackground.gameObject.SetActive(false);
                joystickHandle.anchoredPosition = Vector2.zero;

                playerCharactor.OnMoveStop();
            }
        }
        else if (Managers.Camera.currentMapCameraMode == CameraManager.MapCameraMode.Room)
        {
            if (Input.GetMouseButton(0) && touch)
            {
                Vector2 currentTouchPosition = Input.mousePosition;
                Vector2 dragDelta = currentTouchPosition - startTouchPosition;

                float dragThreshold = 10f;
                if (!isDragging && dragDelta.magnitude > dragThreshold && Managers.UI._currentPopup == null)
                {
                    isDragging = true;
                }

                if (isDragging)
                {
                    float cameraMoveSpeed = 0.8f;
                    Vector3 move = new Vector3(-dragDelta.x, -dragDelta.y, 0) * cameraMoveSpeed * Time.deltaTime;

                    if (mapVirtualCamera != null)
                    {
                        mapVirtualCamera.transform.Translate(move, Space.World);

                        Vector3 pos = mapVirtualCamera.transform.position;
                        pos.x = Mathf.Clamp(pos.x, 188f, 222f);
                        pos.y = Mathf.Clamp(pos.y, -29f, 3f);
                        mapVirtualCamera.transform.position = pos;
                    }

                    startTouchPosition = currentTouchPosition;
                }
            }

            if (Input.GetMouseButtonUp(0) && touch)
            {
                touch = false;

                if (!isDragging)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray);
                    var tiles = hits.Where(w => w.transform.GetComponent<Tile>() != null && w.transform.GetComponent<Tile>().playerTile).FirstOrDefault();
                    var structures = hits.Where(w => w.transform.GetComponent<Structure>() != null && w.transform.GetComponent<Structure>().playerData == Managers.Game.playerData
                     && w.transform.GetComponent<Structure>().playerData == Managers.Game.playerData).FirstOrDefault();

                    if (tiles != default(RaycastHit2D) && Managers.UI._currentPopup == null)
                    {
                        if (tiles.transform.GetComponent<Tile>().currentStructure != null)
                        {
                            var structure = tiles.transform.GetComponent<Tile>().currentStructure;

                            Upgrade_Popup(structure);
                        }
                        else
                        {
                            var popup = Managers.UI.ShowPopupUI<Structure_Popup>();
                            popup.Setting(tiles.transform.GetComponent<Tile>());
                        }

                        Managers.Game.selectedTile = tiles.transform.GetComponent<Tile>();
                    }

                    if (structures != default(RaycastHit2D) && Managers.UI._currentPopup == null)
                    {
                        Upgrade_Popup(structures.transform.GetComponent<Structure>());
                        //구조물 클릭시 업그레이드 팝업
                    }

                    void Upgrade_Popup(Structure structure)
                    {
                        if (structure.activeEffects.Any(a => a is CreepylaughterEffect))
                        {
                            Managers.Audio.PlaySound("snd_stage_unlock", minRangeVolumeMul: -1f);
                            return;
                        }

                        if (structure.type == Define.StructureType.MoneySack || structure.type == Define.StructureType.Battery || structure.type == Define.StructureType.Lamp)
                        {
                            return;
                        }
                        else if (structure.type == Define.StructureType.Grave || structure.type == Define.StructureType.Frog || structure.type == Define.StructureType.PoisonFrog)
                        {
                            Managers.UI.ShowNotificationPopup("global.str_build_max_toast");
                        }
                        else
                        {
                            var popup = Managers.UI.ShowPopupUI<Upgrade_Popup>();
                            popup.Setting(structure.transform.GetComponent<Structure>());
                        }
                    }
                }
                isDragging = false;
            }
        }
    }
}
