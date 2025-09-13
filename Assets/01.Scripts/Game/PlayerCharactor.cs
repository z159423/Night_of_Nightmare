using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;
using System.Linq;


public class PlayerCharactor : PlayerableCharactor
{
    private GameObject tutorialArrow;
    private LineRenderer pathLineRenderer;

    // 경로 부드러운 전환을 위한 변수들
    private Vector3[] currentPathPositions;
    private Vector3[] targetPathPositions;
    private Vector3 lastTargetPosition; // 마지막 목표 지점 저장
    private float pathTransitionTimer = 0f;
    private float pathTransitionDuration = 0.15f; // 전환 시간 (더 빠르게)
    private bool isTransitioningPath = false;
    private float pathUpdateCooldown = 0f;
    private float pathUpdateInterval = 0.3f; // 경로 업데이트 간격 (더 빠르게)

    private void InitializePathLineRenderer()
    {
        // LineRenderer 컴포넌트 추가 또는 기존 것 사용
        pathLineRenderer = GetComponent<LineRenderer>();
        if (pathLineRenderer == null)
        {
            pathLineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // LineRenderer 설정
        pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pathLineRenderer.startColor = new Color32(255, 194, 117, 180);
        pathLineRenderer.endColor = new Color32(255, 194, 117, 180);
        pathLineRenderer.textureMode = LineTextureMode.Tile;
        pathLineRenderer.materials = new Material[] { Managers.Resource.Load<Material>("TutorialRenderer") };
        pathLineRenderer.startWidth = 0.65f;
        pathLineRenderer.endWidth = 0.65f;
        pathLineRenderer.sortingOrder = 1;
        pathLineRenderer.useWorldSpace = true;
        pathLineRenderer.enabled = false;
    }

    private void UpdatePathLineRenderer(Vector3 targetPosition)
    {
        if (pathLineRenderer == null || agent == null)
            return;

        // 전환 중이면 전환만 업데이트하고 새로운 경로 계산은 하지 않음
        if (isTransitioningPath)
        {
            UpdatePathTransition();
            return;
        }

        // 쿨다운 체크 - 너무 자주 업데이트하지 않도록
        if (pathUpdateCooldown > 0)
        {
            pathUpdateCooldown -= Time.deltaTime;
            // 전환 중이 아니면서 쿨다운 중일 때는 기존 경로의 시작점만 업데이트
            if (targetPathPositions != null)
            {
                UpdateStaticPath(targetPathPositions);
            }
            return;
        }

        // NavMeshPath 계산 (목표지점에서 플레이어로)
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(targetPosition, transform.position, NavMesh.AllAreas, path))
        {
            if (path.corners.Length > 0)
            {
                // 새 경로 포지션 배열 생성 (보간 포인트 추가로 더 부드럽게)
                Vector3[] basicPath = new Vector3[path.corners.Length];
                for (int i = 0; i < path.corners.Length; i++)
                {
                    Vector3 point = path.corners[i];
                    point.z = -0.1f;
                    basicPath[i] = point;
                }

                // 경로를 보간해서 더 많은 포인트 생성
                Vector3[] newPathPositions = InterpolatePath(basicPath);

                // 도착점이 변경되었는지 확인 (즉시 재설정)
                bool destinationChanged = HasDestinationChanged(targetPosition);

                if (destinationChanged)
                {
                    // 도착점이 변하면 즉시 재설정
                    SetPathImmediately(newPathPositions);
                    lastTargetPosition = targetPosition;
                    pathUpdateCooldown = pathUpdateInterval;
                }
                else if (HasPathChanged(newPathPositions))
                {
                    // 경로만 변경된 경우 부드럽게 전환
                    StartPathTransition(newPathPositions);
                    pathUpdateCooldown = pathUpdateInterval;
                }
                else
                {
                    // 경로 변경이 없으면 시작점만 업데이트
                    UpdateStaticPath(newPathPositions);
                }

                pathLineRenderer.enabled = true;
            }
            else
            {
                pathLineRenderer.enabled = false;
            }
        }
        else
        {
            pathLineRenderer.enabled = false;
        }
    }

    private bool HasDestinationChanged(Vector3 targetPosition)
    {
        // 첫 번째 호출이거나 도착점이 크게 변경된 경우
        if (lastTargetPosition == Vector3.zero)
        {
            return true;
        }

        return Vector3.Distance(lastTargetPosition, targetPosition) > 1.0f; // 도착점 변경 임계값
    }

    private void SetPathImmediately(Vector3[] newPath)
    {
        // 즉시 경로 설정 (전환 없이)
        isTransitioningPath = false;
        currentPathPositions = (Vector3[])newPath.Clone();
        targetPathPositions = (Vector3[])newPath.Clone();

        pathLineRenderer.positionCount = newPath.Length;
        for (int i = 0; i < newPath.Length; i++)
        {
            if (i == newPath.Length - 1)
            {
                Vector3 playerPos = transform.position;
                playerPos.z = -0.1f;
                pathLineRenderer.SetPosition(i, playerPos);
            }
            else
            {
                pathLineRenderer.SetPosition(i, newPath[i]);
            }
        }
    }

    private Vector3[] InterpolatePath(Vector3[] originalPath)
    {
        if (originalPath == null || originalPath.Length < 2)
            return originalPath;

        List<Vector3> interpolatedPoints = new List<Vector3>();

        // 각 세그먼트 사이에 보간 포인트 추가
        for (int i = 0; i < originalPath.Length; i++)
        {
            interpolatedPoints.Add(originalPath[i]);

            // 마지막 포인트가 아닌 경우, 다음 포인트와의 사이에 보간 포인트들 추가
            if (i < originalPath.Length - 1)
            {
                Vector3 current = originalPath[i];
                Vector3 next = originalPath[i + 1];
                float distance = Vector3.Distance(current, next);

                // 거리가 충분히 긴 경우에만 보간 포인트 추가 (0.5f 이상)
                if (distance > 0.2f)
                {
                    int interpolationCount = Mathf.Clamp(Mathf.RoundToInt(distance / 0.1f), 1, 5); // 최대 5개 보간점

                    for (int j = 1; j <= interpolationCount; j++)
                    {
                        float t = (float)j / (interpolationCount + 1);
                        Vector3 interpolatedPoint = Vector3.Lerp(current, next, t);
                        interpolatedPoints.Add(interpolatedPoint);
                    }
                }
            }
        }

        return interpolatedPoints.ToArray();
    }

    private bool HasPathChanged(Vector3[] newPath)
    {
        if (targetPathPositions == null || targetPathPositions.Length != newPath.Length)
            return true;

        // 마지막 포인트(플레이어 위치)는 항상 변하므로 비교에서 제외
        for (int i = 0; i < newPath.Length - 1; i++)
        {
            if (Vector3.Distance(targetPathPositions[i], newPath[i]) > 0.2f) // 임계값을 낮춰서 더 민감하게
                return true;
        }
        return false;
    }

    private void StartPathTransition(Vector3[] newPath)
    {
        // 현재 LineRenderer의 포지션을 저장하되, 시작점은 현재 플레이어 위치로 업데이트
        if (currentPathPositions == null || !isTransitioningPath)
        {
            currentPathPositions = GetCurrentLineRendererPositions();
        }

        // 기존 경로의 끝점을 현재 플레이어 위치로 업데이트
        if (currentPathPositions != null && currentPathPositions.Length > 0)
        {
            Vector3 playerPos = transform.position;
            playerPos.z = -0.1f;
            currentPathPositions[currentPathPositions.Length - 1] = playerPos;
        }

        targetPathPositions = newPath;
        pathTransitionTimer = 0f;
        isTransitioningPath = true;
    }

    private Vector3[] GetCurrentLineRendererPositions()
    {
        if (pathLineRenderer == null || pathLineRenderer.positionCount == 0)
            return new Vector3[0];

        Vector3[] positions = new Vector3[pathLineRenderer.positionCount];
        pathLineRenderer.GetPositions(positions);
        return positions;
    }

    private void UpdatePathTransition()
    {
        if (!isTransitioningPath || targetPathPositions == null || targetPathPositions.Length == 0)
            return;

        // 현재 플레이어 위치
        Vector3 currentPlayerPos = transform.position;
        currentPlayerPos.z = -0.1f;

        // 기존 경로와 새 경로의 끝점을 모두 현재 플레이어 위치로 업데이트
        if (currentPathPositions != null && currentPathPositions.Length > 0)
        {
            currentPathPositions[currentPathPositions.Length - 1] = currentPlayerPos;
        }
        if (targetPathPositions.Length > 0)
        {
            targetPathPositions[targetPathPositions.Length - 1] = currentPlayerPos;
        }

        pathTransitionTimer += Time.deltaTime;
        float t = Mathf.Clamp01(pathTransitionTimer / pathTransitionDuration);

        // Ease out 커브 적용으로 더 부드러운 전환
        t = 1f - (1f - t) * (1f - t);

        pathLineRenderer.positionCount = targetPathPositions.Length;

        for (int i = 0; i < targetPathPositions.Length; i++)
        {
            Vector3 targetPos = targetPathPositions[i];

            // 마지막 포인트(끝점)는 항상 플레이어의 현재 위치로 설정
            if (i == targetPathPositions.Length - 1)
            {
                pathLineRenderer.SetPosition(i, currentPlayerPos);
            }
            else
            {
                Vector3 startPos = targetPos;
                if (currentPathPositions != null && i < currentPathPositions.Length)
                {
                    startPos = currentPathPositions[i];
                }
                else if (currentPathPositions != null && currentPathPositions.Length > 0)
                {
                    // 기존 경로보다 새 경로가 더 길 경우, 가장 가까운 유효한 포인트 사용
                    int clampedIndex = Mathf.Min(i, currentPathPositions.Length - 1);
                    startPos = currentPathPositions[clampedIndex];
                }

                Vector3 lerpedPos = Vector3.Lerp(startPos, targetPos, t);
                pathLineRenderer.SetPosition(i, lerpedPos);
            }
        }

        // 전환 완료
        if (t >= 1f)
        {
            isTransitioningPath = false;
            currentPathPositions = (Vector3[])targetPathPositions.Clone();
            // 전환 완료 후 약간의 쿨다운을 줘서 즉시 다른 전환이 시작되지 않도록 함
            pathUpdateCooldown = 0; // 더 짧은 쿨다운
        }
    }

    private void UpdateStaticPath(Vector3[] pathPositions)
    {
        if (pathPositions == null || pathPositions.Length == 0)
            return;

        pathLineRenderer.positionCount = pathPositions.Length;

        for (int i = 0; i < pathPositions.Length; i++)
        {
            if (i == pathPositions.Length - 1)
            {
                // 마지막 포인트는 항상 플레이어의 현재 위치
                Vector3 playerPos = transform.position;
                playerPos.z = -0.1f;
                pathLineRenderer.SetPosition(i, playerPos);
            }
            else
            {
                pathLineRenderer.SetPosition(i, pathPositions[i]);
            }
        }
    }

    public void Setting()
    {
        charactorType = (Define.CharactorType)Managers.LocalData.SelectedCharactor;

        Managers.Game.playerCharactor = this;

        if (Managers.UI._currentScene is UI_GameScene_Map)
            Managers.UI._currentScene.GetComponent<UI_GameScene_Map>().SetPlayerIcon(this);

        GetComponentInParent<NavMeshAgent>(true).enabled = true;

        Managers.Game.charactors.Add(this);

        playerData = new PlayerData(charactorType);
        Managers.Game.playerData = playerData;

        SetBodySkin();

        tutorialArrow = gameObject.FindRecursive("TutorialArrow");

        // LineRenderer 초기화
        InitializePathLineRenderer();

        if (Managers.LocalData.PlayerTutorialStep == 0)
        {
            tutorialArrow.SetActive(true);
            pathLineRenderer.enabled = true;
        }
        else
        {
            tutorialArrow.SetActive(false);
            pathLineRenderer.enabled = false;
        }
    }

    public override void SetBodySkin()
    {
        if (bodySpriteRenderer == null)
        {
            bodySpriteRenderer = gameObject.FindRecursive("Icon").GetComponent<SpriteRenderer>();
        }
        bodySpriteRenderer.sprite = Managers.Resource.GetCharactorImage((int)charactorType + 1);
    }

    protected override void Start()
    {
        base.Start();
    }

    private Vector3 lastPosition;

    protected override void Update()
    {
        // NavMeshAgent가 할당되어 있는지 확인
        if (agent != null)
        {
            Vector3 velocity;

            // agent.Move를 사용할 때는 velocity가 자동으로 갱신되지 않으므로 직접 계산
            velocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;

            // 직접 계산한 velocity가 0이 아니면 방향을 계산
            if (velocity.sqrMagnitude > 0.01f)
            {
                float dir = velocity.x;
                icon.transform.localRotation = Quaternion.Euler(0, dir >= 0 ? 0 : 180, 0);
            }
        }

        if (tutorialArrow.activeSelf)
        {
            var nearestBed = Managers.Game.beds
                .Where(b => !b.active)
                .OrderBy(b => Vector3.Distance(transform.position, b.transform.position))
                .FirstOrDefault();

            if (nearestBed != null)
            {
                Vector3 direction = (nearestBed.transform.position - tutorialArrow.transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                tutorialArrow.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

                // NavMesh 경로 그리기 (부드러운 전환 포함)
                UpdatePathLineRenderer(nearestBed.transform.position);
            }
        }
        else
        {
            // 튜토리얼이 비활성화되면 LineRenderer도 비활성화
            if (pathLineRenderer != null && pathLineRenderer.enabled)
            {
                pathLineRenderer.enabled = false;
                isTransitioningPath = false;
            }
        }
    }


    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Bed>(out Bed bed))
        {
            if (!bed.active)
            {
                GameObserver.Call(GameObserverType.Game.OnActivePlayerBed);
                currentActiveRoom = bed.OnActive(this);
                gameObject.SetActive(false);

                playerData.room = currentActiveRoom;

                playerData.structures.Add(currentActiveRoom.bed);
                currentActiveRoom.bed.playerData = playerData;
                playerData.structures.Add(currentActiveRoom.door);
                currentActiveRoom.door.playerData = playerData;

                GameObserver.Call(GameObserverType.Game.OnPlayerTutorialActing);

                tutorialArrow.gameObject.SetActive(false);

                // LineRenderer와 전환 상태 초기화
                if (pathLineRenderer != null)
                {
                    pathLineRenderer.enabled = false;
                    isTransitioningPath = false;
                    currentPathPositions = null;
                    targetPathPositions = null;
                }

                Managers.Camera.FocusToTarget(transform);
            }
        }
    }

    public override void Hit(int damage)
    {
        // AI 캐릭터는 Hit 메서드를 구현하지 않음
        // 필요시 AI 캐릭터의 행동을 정의할 수 있음

        Die();
    }

    public override void Die()
    {
        base.Die();

        Managers.Game.GameOver();
    }
}
