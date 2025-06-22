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

    private CinemachineVirtualCamera mapVirtualCamera;

    private void Start()
    {
        // 처음에는 조이스틱 UI를 숨김
        joystickBackground.gameObject.SetActive(false);

        this.SetListener(GameObserverType.Game.OnActivePlayerBed, () =>
        {
            isDragging = false;
            joystickBackground.gameObject.SetActive(false);
            playerCharactor.OnMoveStop();
        });

        mapVirtualCamera = Managers.Camera.cameras[Define.GameMode.Map];
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
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
            if (Input.GetMouseButton(0))
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

            if (Input.GetMouseButtonUp(0))
            {
                if (!isDragging)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray);
                    var tiles = hits.Where(w => w.transform.GetComponent<Tile>() != null).FirstOrDefault();
                    var structures = hits.Where(w => w.transform.GetComponent<Structure>() != null && w.transform.GetComponent<Structure>().playerData == Managers.Game.playerData).FirstOrDefault();

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
                        if (structure.type == Define.StructureType.MoneySack || structure.type == Define.StructureType.Battery)
                        {
                            return;
                        }
                        else if (structure.type == Define.StructureType.Grave || structure.type == Define.StructureType.Frog || structure.type == Define.StructureType.PoisonFrog)
                        {
                            var popup = Managers.Resource.Instantiate("Notification_Popup", Managers.UI.Root.transform);
                            popup.GetComponent<Notification_Popup>().Init();
                            popup.GetComponent<Notification_Popup>().Setting(Managers.Localize.GetText("global.str_build_max_toast"));
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
