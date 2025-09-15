using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; // 추가: LINQ 사용을 위해
using UnityEngine.SceneManagement;

public class CheatManager : MonoBehaviour
{
    [Header("Cheat Settings")]
    public bool enableCheat = false;
    public float shakeThreshold = 2.0f;
    public float shakeTimeout = 1.0f;

    private int shakeCount = 0;
    private float lastShakeTime = 0f;
    private Vector3 lastAcceleration = Vector3.zero;

    [Header("UI References")]
    public GameObject cheatPanel;
    public TMP_InputField cheatInputField;
    public Button executeButton;
    public Button closeButton;
    public Transform uiParent;

    private TextMeshProUGUI statusText;

    void Start()
    {
        // Development 모드일 때만 치트 활성화
        enableCheat = true;

        if (enableCheat)
        {
            // UI 생성을 지연시킴
            StartCoroutine(InitializeCheatUI());
        }
    }

    IEnumerator InitializeCheatUI()
    {
        // Managers가 초기화될 때까지 대기
        while (Managers.UI == null || Managers.UI.Root == null)
        {
            yield return null;
        }

        // 추가로 1프레임 더 대기
        yield return null;

        CreateCheatUI();

        if (cheatPanel != null)
        {
            cheatPanel.SetActive(false);
        }
    }

    IEnumerator RetryCreateCheatUI()
    {
        yield return new WaitForSeconds(1f);
        CreateCheatUI();
    }

    void Update()
    {
        if (!enableCheat) return;

        // 치트 패널이 생성되지 않았으면 업데이트 로직 실행 안함
        if (cheatPanel == null) return;

        // 흔들기 감지 (모바일)
        if (Application.isMobilePlatform)
        {
            DetectShake();
        }

        // PC에서는 F12 키로 치트 패널 열기
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ToggleCheatPanel();
        }
    }

    void DetectShake()
    {
        if (cheatPanel != null && cheatPanel.activeSelf)
            return;

        Vector3 acceleration = Input.acceleration;
        Vector3 deltaAcceleration = acceleration - lastAcceleration;
        lastAcceleration = acceleration;

        if (deltaAcceleration.magnitude > shakeThreshold)
        {
            if (Time.time - lastShakeTime > shakeTimeout)
            {
                shakeCount = 0;
            }

            shakeCount++;
            lastShakeTime = Time.time;

            Debug.Log($"Shake detected! Count: {shakeCount}");

            if (shakeCount >= 2)
            {
                ToggleCheatPanel();
                shakeCount = 0;
            }
        }
    }

    void CreateCheatUI()
    {
        // UI 부모 찾기 (Canvas)
        if (uiParent == null)
        {
            if (Managers.UI != null && Managers.UI.Root != null)
            {
                uiParent = Managers.UI.Root.transform;
            }
            else
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    uiParent = canvas.transform;
                }
            }
        }

        if (uiParent == null)
        {
            Debug.LogError("CheatManager: UI Parent (Canvas) not found! Retrying in 1 second...");
            StartCoroutine(RetryCreateCheatUI());
            return;
        }

        // 치트 패널 생성
        cheatPanel = new GameObject("CheatPanel");
        cheatPanel.transform.SetParent(uiParent, false);
        cheatPanel.transform.SetAsLastSibling(); // 최상단에 위치

        // RectTransform이 자동으로 추가되는지 확인, 없으면 추가
        RectTransform panelRect = cheatPanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            panelRect = cheatPanel.AddComponent<RectTransform>();
        }

        Image panelImage = cheatPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);

        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 내용 컨테이너 (더 큰 크기로 조정)
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(cheatPanel.transform, false);

        // RectTransform 명시적 추가
        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(1200, 1800); // 높이 증가: 1000 -> 1800 (전광판이 커진 만큼)
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;

        Image contentImage = contentPanel.AddComponent<Image>();
        contentImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // 제목
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(contentPanel.transform, false);

        // RectTransform 명시적 추가
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(1150, 60); // 높이 줄임: 80 -> 60
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -40); // 위치 조정: -60 -> -40

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "CHEAT CONSOLE";
        titleText.fontSize = 50; // 크기 줄임: 60 -> 50
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // 전광판 추가 (제목 아래)
        CreateStatusBoard(contentPanel);

        // 스크롤 뷰 생성 (위치 조정)
        GameObject scrollView = CreateScrollView(contentPanel);

        // 입력 필드 섹션 (위치 조정)
        CreateInputSection(contentPanel);

        // 닫기 버튼 (위치 조정)
        CreateButton(contentPanel, "Close", new Vector2(0, -780), () => CloseCheatPanel(), new Vector2(300, 120)); // -470 -> -510

        Debug.Log("CheatManager: UI created successfully!");
    }

    void CreateStatusBoard(GameObject parent)
    {
        // 전광판 컨테이너 (크기 증가)
        GameObject statusBoard = new GameObject("StatusBoard");
        statusBoard.transform.SetParent(parent.transform, false);

        RectTransform statusRect = statusBoard.AddComponent<RectTransform>();
        statusRect.sizeDelta = new Vector2(1150, 665); // 높이 증가: 120 -> 500
        statusRect.anchorMin = new Vector2(0.5f, 1f);
        statusRect.anchorMax = new Vector2(0.5f, 1f);
        statusRect.anchoredPosition = new Vector2(0, -430); // 위치 조정: -120 -> -140

        // 전광판 배경
        Image statusBg = statusBoard.AddComponent<Image>();
        statusBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // 스크롤바 생성 (오른쪽에 배치)
        GameObject scrollbar = CreateVerticalScrollbar(statusBoard);

        // 스크롤 가능한 텍스트 영역 (스크롤바 공간 확보)
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(statusBoard.transform, false);

        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(10, 10); // 왼쪽, 아래 여백
        viewportRect.offsetMax = new Vector2(-30, -10); // 오른쪽(스크롤바 공간), 위 여백

        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color32(26, 26, 26, 204);

        // 텍스트 컨텐츠
        GameObject textContent = new GameObject("TextContent");
        textContent.transform.SetParent(viewport.transform, false);

        RectTransform textContentRect = textContent.AddComponent<RectTransform>();
        textContentRect.anchorMin = new Vector2(0, 1);
        textContentRect.anchorMax = new Vector2(1, 1);
        textContentRect.pivot = new Vector2(0.5f, 1);
        textContentRect.anchoredPosition = Vector2.zero;

        textContentRect.sizeDelta = new Vector2(0, 0); // 너비는 부모에 맞추고 높이는 텍스트에 따라 자동 조정

        // 상태 텍스트
        // GameObject statusTextObj = new GameObject("StatusText");
        // statusTextObj.transform.SetParent(textContent.transform, false);

        // RectTransform statusTextRect = statusTextObj.AddComponent<RectTransform>();
        // statusTextRect.anchorMin = Vector2.zero;
        // statusTextRect.anchorMax = Vector2.one;
        // statusTextRect.offsetMin = Vector2.zero;
        // statusTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI statusText = textContent.AddComponent<TextMeshProUGUI>();
        statusText.fontSize = 32; // 크기 살짝 줄임: 32 -> 32 (더 많은 정보 표시)
        statusText.alignment = TextAlignmentOptions.TopLeft;
        statusText.color = new Color(0.8f, 1f, 0.8f); // 연한 녹색
        statusText.fontStyle = FontStyles.Bold;
        statusText.overflowMode = TextOverflowModes.Overflow; // 텍스트 오버플로우 허용

        this.statusText = statusText;

        // ContentSizeFitter 추가 (텍스트 크기에 맞춰 높이 조정)
        ContentSizeFitter textFitter = textContent.AddComponent<ContentSizeFitter>();
        textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 스크롤 컴포넌트 추가
        ScrollRect statusScroll = statusBoard.AddComponent<ScrollRect>();
        statusScroll.content = textContentRect;
        statusScroll.viewport = viewportRect;
        statusScroll.horizontal = false;
        statusScroll.vertical = true;
        statusScroll.verticalScrollbar = scrollbar.GetComponent<Scrollbar>();
        statusScroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        statusScroll.verticalScrollbarSpacing = -3f; // 스크롤바와 컨텐츠 간격

        // 마스크 추가 (스크롤 영역을 벗어난 텍스트 숨김)
        Mask viewportMask = statusScroll.gameObject.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        // 스크롤 감도 설정
        statusScroll.scrollSensitivity = 20f;

        // 상태 업데이트 코루틴 시작
        StartCoroutine(UpdateStatusBoard(statusText));
    }

    IEnumerator UpdateStatusBoard(TextMeshProUGUI statusText)
    {
        while (cheatPanel != null && cheatPanel.activeSelf)
        {
            UpdateStatusText(statusText);
            yield return new WaitForSecondsRealtime(0.5f); // 0.5초마다 업데이트
        }
    }

    void UpdateStatusText(TextMeshProUGUI statusText)
    {
        if (statusText == null) return;

        // null 체크 추가
        if (Managers.LocalData == null)
        {
            statusText.text = "<color=#FF0000>LocalData Manager is NULL</color>";
            return;
        }

        var localData = Managers.LocalData;
        var gameManager = Managers.Game;

        string statusInfo = "";

        try
        {
            // === 게임 통계 ===
            statusInfo += "<color=#FFD700>[ GAME STATS ]</color>\n";
            statusInfo += $"Win: {localData.PlayerWinCount} | Game: {localData.PlayerGameCount} | ";
            statusInfo += $"Win Rate: {(localData.PlayerGameCount > 0 ? ((float)localData.PlayerWinCount / localData.PlayerGameCount * 100f).ToString("F1") : "0")}%\n";
            statusInfo += $"Ranking Point: {localData.PlayerRankingPoint} | Highest Tier: {localData.PlayerHighestTier}\n";

            // === 재화 정보 ===
            statusInfo += "\n<color=#90EE90>[ CURRENCY ]</color>\n";
            statusInfo += $"Gem: {localData.PlayerGemCount} | RV Ticket: {localData.PlayerRvTicketCount}\n";

            // === 부스트 아이템 ===
            statusInfo += "\n<color=#87CEEB>[ BOOST ITEMS ]</color>\n";
            statusInfo += $"Lamp: {localData.PlayerLampCount} | Hammer: {localData.playerHammerCount} | ";
            statusInfo += $"Shield: {localData.PlayerHolyShieldCount} | Overheat: {localData.PlayerOverHeatCount}\n";

            // === 캐릭터 정보 ===
            statusInfo += "\n<color=#DDA0DD>[ CHARACTER ]</color>\n";
            statusInfo += $"Selected: {(Define.CharactorType)localData.SelectedCharactor} | ";
            statusInfo += $"Owned Count: {localData.OwnedCharactorCount}\n";

            // 소유한 캐릭터 목록
            var ownedChars = new List<string>();
            for (int i = 0; i < System.Enum.GetValues(typeof(Define.CharactorType)).Length; i++)
            {
                if (localData.HasCharactor((Define.CharactorType)i))
                {
                    ownedChars.Add(((Define.CharactorType)i).ToString());
                }
            }
            statusInfo += $"Owned: [{string.Join(", ", ownedChars)}]\n";

            // === 게임 모드 ===
            statusInfo += "\n<color=#FFB6C1>[ GAME MODE ]</color>\n";
            if (gameManager != null)
            {
                statusInfo += $"Enemy God Mode: <color={(gameManager.enemyGodMode ? "#00FF00>ON" : "#FF0000>OFF")}</color> | ";
                statusInfo += $"Player Door God Mode: <color={(gameManager.playerDoorGodMode ? "#00FF00>ON" : "#FF0000>OFF")}</color>\n";
            }
            else
            {
                statusInfo += "Game Manager: <color=#FF0000>NULL</color>\n";
            }

            // === 시스템 설정 ===
            statusInfo += "\n<color=#F0E68C>[ SETTINGS ]</color>\n";
            statusInfo += $"Haptic: <color={(localData.UseHaptic ? "#00FF00>ON" : "#FF0000>OFF")}</color> | ";
            statusInfo += $"Sound: <color={(localData.UseSound ? "#00FF00>ON" : "#FF0000>OFF")}</color> | ";
            statusInfo += $"Volume: <color={(localData.Volume ? "#00FF00>ON" : "#FF0000>OFF")}</color>\n";
            statusInfo += $"Language Index: {localData.LanguageIndex} | Tutorial Step: {localData.PlayerTutorialStep}\n";

            // === 기타 정보 ===
            statusInfo += "\n<color=#FFA07A>[ MISC ]</color>\n";
            statusInfo += $"Challenge Level: {localData.ChallengeModeLevel} | ";
            statusInfo += $"Random Box RV Count: {localData.RandomBoxRvCount}\n";
            statusInfo += $"First Use Lamp: {localData.FirstUseLamp} | ";
            statusInfo += $"Is Open Random Box RV: <color={(localData.IsOpenRandomBoxRv ? "#00FF00>YES" : "#FF0000>NO")}</color>\n";

            // === 출석체크 & 세션 보상 (매니저가 있는 경우만) ===
            try
            {
                // null 체크 추가
                if (Managers.Attendance != null)
                {
                    var attendanceManager = Managers.Attendance;
                    statusInfo += "\n<color=#98FB98>[ ATTENDANCE ]</color>\n";
                    var attendanceStatus = attendanceManager.GetStatus();
                    statusInfo += $"Days Claimed: {attendanceStatus.daysClaimed}/7 | ";
                    statusInfo += $"Today: <color={(attendanceStatus.claimedToday ? "#00FF00>CLAIMED" : "#FFFF00>AVAILABLE")}</color> | ";
                    statusInfo += $"Finished: <color={(attendanceStatus.finished ? "#00FF00>YES" : "#FF0000>NO")}</color>\n";

                    if (!attendanceStatus.finished)
                    {
                        string timeLeft = attendanceManager.GetTimeUntilNextClaimString();
                        statusInfo += $"Next Claim: {(string.IsNullOrEmpty(timeLeft) ? "Available Now" : timeLeft)}\n";
                    }
                }

                // null 체크 추가
                if (Managers.SessionReward != null)
                {
                    var sessionManager = Managers.SessionReward;
                    statusInfo += "\n<color=#B0E0E6>[ SESSION REWARD ]</color>\n";
                    statusInfo += $"Time Today: {sessionManager.AccumulatedSecondsToday}s ({sessionManager.AccumulatedSecondsToday / 60}m)\n";

                    var rewardStatuses = sessionManager.GetAllRewardStatus().ToList();
                    var claimedCount = rewardStatuses.Count(r => r.claimed);
                    var availableCount = rewardStatuses.Count(r => r.reached && !r.claimed);

                    statusInfo += $"Claimed: {claimedCount}/6 | Available: {availableCount} | ";
                    statusInfo += $"Next Reset: {sessionManager.SecondsToDailyReset()}\n";
                }
            }
            catch (System.Exception e)
            {
                statusInfo += $"\n<color=#FF0000>[ ERROR ]</color>\n";
                statusInfo += $"Failed to load attendance/session data: {e.Message}\n";
            }
        }
        catch (System.Exception e)
        {
            statusInfo = $"<color=#FF0000>[ ERROR ]</color>\n";
            statusInfo += $"Failed to update status: {e.Message}\n";
        }

        statusText.text = statusInfo;
    }

    GameObject CreateVerticalScrollbar(GameObject parent)
    {
        // 스크롤바 컨테이너
        GameObject scrollbar = new GameObject("Scrollbar");
        scrollbar.transform.SetParent(parent.transform, false);

        RectTransform scrollbarRect = scrollbar.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);
        scrollbarRect.offsetMin = new Vector2(-20f, 5f); // 왼쪽에서 20px, 아래에서 5px
        scrollbarRect.offsetMax = new Vector2(-5f, -5f); // 오른쪽에서 5px, 위에서 5px

        Scrollbar scrollbarComponent = scrollbar.AddComponent<Scrollbar>();
        scrollbarComponent.direction = Scrollbar.Direction.BottomToTop;

        // 스크롤바 배경
        Image scrollbarBg = scrollbar.AddComponent<Image>();
        scrollbarBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        scrollbarComponent.targetGraphic = scrollbarBg;

        // 스크롤바 핸들 영역
        GameObject slidingArea = new GameObject("Sliding Area");
        slidingArea.transform.SetParent(scrollbar.transform, false);

        RectTransform slidingAreaRect = slidingArea.AddComponent<RectTransform>();
        slidingAreaRect.anchorMin = Vector2.zero;
        slidingAreaRect.anchorMax = Vector2.one;
        slidingAreaRect.offsetMin = new Vector2(2f, 2f);
        slidingAreaRect.offsetMax = new Vector2(-2f, -2f);

        // 스크롤바 핸들
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(slidingArea.transform, false);

        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;

        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.6f, 0.6f, 0.6f, 0.9f);

        // 스크롤바 컴포넌트 설정
        scrollbarComponent.handleRect = handleRect;
        scrollbarComponent.targetGraphic = handleImage;

        // 호버 효과
        var colors = scrollbarComponent.colors;
        colors.highlightedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        scrollbarComponent.colors = colors;

        return scrollbar;
    }

    GameObject CreateScrollView(GameObject parent)
    {
        // 스크롤 뷰
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(parent.transform, false);

        // RectTransform 명시적 추가
        RectTransform scrollRect_RT = scrollView.AddComponent<RectTransform>();
        scrollRect_RT.sizeDelta = new Vector2(1150, 630); // 높이 줄임: 400 -> 320 (전광판이 커진 만큼)
        scrollRect_RT.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRect_RT.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRect_RT.anchoredPosition = new Vector2(0, -225); // 위치 조정: -30 -> -70

        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        Image scrollImage = scrollView.AddComponent<Image>();
        scrollImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // 뷰포트
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);

        // RectTransform 명시적 추가
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.clear;

        // 콘텐츠
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);

        // RectTransform 명시적 추가
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;

        GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(350, 80);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3; // 3열

        ContentSizeFitter contentSizeFitter = content.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        // 치트 버튼들 생성
        CreateCheatButtons(content);

        return scrollView;
    }

    void CreateCheatButtons(GameObject parent)
    {
        // 치트 버튼 데이터 정의
        var cheatButtons = new[]
        {
            new { text = "Gem +10000", code = "gem10000", color = new Color(0.65f, 1f, 0.75f) }, // 연한 녹색
            new { text = "Ticket +1", code = "ticket1", color = new Color(1f, 1f, 0.7f) }, // 연한 노랑

            new { text = "Reset Gem", code = "resetgem", color = new Color(1f, 0.7f, 0.7f) }, // 연한 빨강

            new { text = "Clear Data", code = "cleardata", color = new Color(0.95f, 0.85f, 1f) }, // 연보라
            new { text = "Enemy God Mode", code = "enemy_god_mode", color = new Color(1f, 0.85f, 0.65f) }, // 연한 오렌지
            new { text = "Player Door God Mode", code = "player_door_god_mode", color = new Color(0.7f, 0.8f, 1f) }, // 연한 파랑

            new { text = "Add Win Count", code = "add_win_count", color = new Color(0.7f, 1f, 1f) }, // 연한 청록
            new { text = "Add Game Count", code = "add_game_count", color = new Color(0.7f, 1f, 1f) }, // 연한 청록

            new { text = "Add Attendance Day", code = "add_attendance_day", color = new Color(1f, 1f, 0.7f) }, // 연한 노랑
            new { text = "Add Session Reward Min", code = "add_session_reward_min", color = new Color(1f, 1f, 0.7f) }, // 연한 노랑

            new { text = "Reset Attendance", code = "reset_attendance", color = new Color(0.8f, 0.8f, 1f) }, // 연파랑
            new { text = "Reset Session Reward", code = "reset_session_reward", color = new Color(0.8f, 0.8f, 1f) }, // 연파랑

            new { text = "Reset Random Box Count", code = "reset_random_box_count", color = new Color(1f, 0.8f, 0.8f) }, // 연빨강
        };

        foreach (var cheat in cheatButtons)
        {
            GameObject buttonObj = new GameObject($"CheatBtn_{cheat.code}");
            buttonObj.transform.SetParent(parent.transform, false);

            // RectTransform 명시적 추가
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();

            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = cheat.color;

            // 버튼 텍스트
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);

            // TextOutliner 제거 (에러 원인일 수 있음)
            // buttonTextObj.AddComponent<TextOutliner>();

            // RectTransform 명시적 추가
            RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = new Vector2(5, 5);
            buttonTextRect.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = cheat.text;
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.black;
            buttonText.fontStyle = FontStyles.Bold;

            // 버튼 클릭 이벤트
            if (!string.IsNullOrEmpty(cheat.code))
            {
                button.onClick.AddListener(() =>
                {
                    ProcessCheatCode(cheat.code);
                    Debug.Log($"Cheat button '{cheat.text}' pressed!");
                });
            }
            else
            {
                // Custom 버튼은 입력 필드에 포커스
                button.onClick.AddListener(() =>
                {
                    if (cheatInputField != null)
                    {
                        cheatInputField.Select();
                        cheatInputField.ActivateInputField();
                    }
                });
            }

            // 호버 효과 추가
            var colors = button.colors;
            colors.highlightedColor = cheat.color * 1.2f;
            colors.pressedColor = cheat.color * 0.8f;
            button.colors = colors;
        }
    }

    TextMeshProUGUI CreatePlaceholder(GameObject parent)
    {
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(parent.transform, false);

        // RectTransform 명시적 추가
        RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(15, 8);
        placeholderRect.offsetMax = new Vector2(-15, -8);

        TextMeshProUGUI placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholder.text = "Enter custom cheat code...";
        placeholder.fontSize = 30;
        placeholder.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholder.fontStyle = FontStyles.Italic;

        return placeholder;
    }

    void CreateInputSection(GameObject parent)
    {
        // 입력 섹션 컨테이너
        GameObject inputSection = new GameObject("InputSection");
        inputSection.transform.SetParent(parent.transform, false);

        // RectTransform 명시적 추가
        RectTransform inputSectionRect = inputSection.AddComponent<RectTransform>();
        inputSectionRect.sizeDelta = new Vector2(1150, 120);
        inputSectionRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputSectionRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputSectionRect.anchoredPosition = new Vector2(0, -650); // 위치 조정: -330 -> -370

        // 입력 필드
        GameObject inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(inputSection.transform, false);

        // RectTransform 명시적 추가
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(800, 80);
        inputRect.anchorMin = new Vector2(0, 0.5f);
        inputRect.anchorMax = new Vector2(0, 0.5f);
        inputRect.anchoredPosition = new Vector2(400, 0);

        cheatInputField = inputObj.AddComponent<TMP_InputField>();
        Image inputImage = inputObj.AddComponent<Image>();
        inputImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        // 입력 필드 텍스트
        GameObject inputTextObj = new GameObject("Text");
        inputTextObj.transform.SetParent(inputObj.transform, false);

        // RectTransform 명시적 추가
        RectTransform inputTextRect = inputTextObj.AddComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.offsetMin = new Vector2(15, 8);
        inputTextRect.offsetMax = new Vector2(-15, -8);

        TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
        inputText.text = "";
        inputText.fontSize = 30;
        inputText.color = Color.white;

        cheatInputField.textComponent = inputText;
        cheatInputField.placeholder = CreatePlaceholder(inputObj);

        // 실행 버튼
        CreateButton(inputSection, "Execute", new Vector2(950, 0), ExecuteCheat, new Vector2(200, 80));
    }

    void CreateButton(GameObject parent, string text, Vector2 position, System.Action onClick, Vector2 size)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        buttonObj.transform.SetParent(parent.transform, false);

        // RectTransform 명시적 추가
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = size;
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;

        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // 버튼 텍스트
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);

        // RectTransform 명시적 추가
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 28;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        button.onClick.AddListener(() => onClick?.Invoke());
    }

    // 기존의 CreateButton 오버로드도 유지
    void CreateButton(GameObject parent, string text, Vector2 position, System.Action onClick)
    {
        CreateButton(parent, text, position, onClick, new Vector2(300, 87));
    }

    void ToggleCheatPanel()
    {
        // 치트 패널이 없으면 생성 시도
        if (cheatPanel == null)
        {
            Debug.LogWarning("CheatManager: Panel not created yet. Trying to create...");
            CreateCheatUI();
            return;
        }

        bool isActive = !cheatPanel.activeSelf;
        cheatPanel.SetActive(isActive);

        cheatPanel.transform.SetAsLastSibling(); // 최상단에 위치

        StartCoroutine(UpdateStatusBoard(statusText));

        if (isActive)
        {
            // 게임 일시정지
            Time.timeScale = 0f;

            if (cheatInputField != null)
            {
                cheatInputField.text = "";
                cheatInputField.Select();
                cheatInputField.ActivateInputField();
            }

            Debug.Log("Cheat panel opened - Game paused");
        }

        Debug.Log($"Cheat panel {(isActive ? "opened" : "closed")}");
    }

    void CloseCheatPanel()
    {
        if (cheatPanel != null)
        {
            cheatPanel.SetActive(false);
            Time.timeScale = 1f;
            Debug.Log("Cheat panel closed - Game resumed");
        }
    }

    void ExecuteCheat()
    {
        if (cheatInputField != null)
        {
            string cheatCode = cheatInputField.text.Trim();
            ProcessCheatCode(cheatCode);
        }
    }

    void ProcessCheatCode(string code)
    {
        // null 체크 추가
        if (Managers.LocalData == null)
        {
            ShowMessage("LocalData Manager is NULL!");
            return;
        }

        // 기본 치트 코드 처리 (예: gem1000)
        switch (code.ToLower())
        {
            case "gem1000":
                // 보석 1000개 추가
                Managers.LocalData.PlayerGemCount += 1000;
                ShowMessage("Gems +1000");
                break;
            case "gem10000":
                // 보석 10000개 추가
                Managers.LocalData.PlayerGemCount += 10000;
                ShowMessage("Gems +10000");
                break;
            case "ticket1":
                // RV 티켓 1개 추가
                Managers.LocalData.PlayerRvTicketCount += 1;
                ShowMessage("Ticket +1");
                break;
            case "resetgem":
                // 보석 리셋
                Managers.LocalData.PlayerGemCount = 0;
                ShowMessage("Gems reset");
                break;
            case "resetrv":
                // RV 리셋
                ShowMessage("RV reset");
                break;
            case "resetsession":
                // 세션 데이터 리셋
                ShowMessage("Session data reset");
                break;
            case "maxboost":
                // 최대 부스트
                ShowMessage("Max boost activated");
                break;
            case "cleardata":
                // 데이터 초기화
                PlayerPrefs.DeleteAll();
                ShowMessage("All data cleared");
                break;
            case "enemy_god_mode":
                if (Managers.Game != null)
                {
                    Managers.Game.enemyGodMode = !Managers.Game.enemyGodMode;
                    ShowMessage($"Enemy God Mode: {(Managers.Game.enemyGodMode ? "ON" : "OFF")}");
                }
                else
                {
                    ShowMessage("Game Manager is NULL!");
                }
                break;
            case "player_door_god_mode":
                if (Managers.Game != null)
                {
                    Managers.Game.playerDoorGodMode = !Managers.Game.playerDoorGodMode;
                    ShowMessage($"Player Door God Mode: {(Managers.Game.playerDoorGodMode ? "ON" : "OFF")}");
                }
                else
                {
                    ShowMessage("Game Manager is NULL!");
                }
                break;
            case "add_win_count":
                Managers.LocalData.PlayerWinCount += 1;
                ShowMessage("Win Count Added");
                break;
            case "add_game_count":
                Managers.LocalData.PlayerGameCount += 1;
                ShowMessage("Game Count Added");
                break;
            case "add_attendance_day":
                if (Managers.Attendance != null)
                {
                    Managers.Attendance.TestSetNextDayAvailable();
                    ShowMessage("Attendance Day Added");
                }
                else
                {
                    ShowMessage("Attendance Manager is NULL!");
                }
                break;
            case "add_session_reward_min":
                if (Managers.SessionReward != null)
                {
                    Managers.SessionReward.TestAddOneMinute();
                    ShowMessage("Session Reward Min Added");
                }
                else
                {
                    ShowMessage("Session Reward Manager is NULL!");
                }
                break;
            case "reset_attendance":
                if (Managers.Attendance != null)
                {
                    Managers.Attendance.TestResetAttendance();
                    ShowMessage("Attendance Reset");
                }
                else
                {
                    ShowMessage("Attendance Manager is NULL!");
                }
                break;
            case "reset_session_reward":
                if (Managers.SessionReward != null)
                {
                    Managers.SessionReward.TestResetSessionReward();
                    ShowMessage("Session Reward Reset");
                }
                else
                {
                    ShowMessage("Session Reward Manager is NULL!");
                }
                break;
            case "reset_random_box_count":
                Managers.LocalData.RandomBoxRvCount = 0;
                ShowMessage("Random Box Count Reset");
                break;
            default:
                // 사용자 정의 치트 코드 처리
                ProcessCustomCheatCode(code);
                break;
        }
    }

    void ShowMessage(string message)
    {
        Debug.Log($"CheatManager: {message}");
        // TODO: 메시지 표시 UI 구현
    }

    void ShowHelp()
    {
        string helpMessage = "Available cheats:\n" +
                             "- Gem +1000: Add 1000 gems\n" +
                             "- Gem +10000: Add 10000 gems\n" +
                             "- Reset Gem: Reset gems to 0\n" +
                             "- Reset RV: Reset RV\n" +
                             "- Reset Session: Reset session data\n" +
                             "- Max Boost: Activate max boost\n" +
                             "- Clear Data: Clear all data\n" +
                             "- Help: Show this help message\n" +
                             "- Custom: Enter custom cheat code";
        ShowMessage(helpMessage);
    }

    void ProcessCustomCheatCode(string code)
    {
        // TODO: 사용자 정의 치트 코드 처리 로직 구현
        Debug.Log($"Custom cheat code processed: {code}");
    }

    // 외부에서 치트 패널을 여는 메서드
    public void OpenCheatPanel()
    {
        if (enableCheat)
        {
            ToggleCheatPanel();
        }
    }
}
