using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttendanceSlotUI : UI_Base
{
    enum Buttons
    {
        AttendenceBtn
    }

    enum Images
    {
        Reddot,
        CheckImage,
        Black
    }

    enum Texts
    {
        DayText
    }

    bool isInit;

    public int day = 0;

    public override void Init()
    {
        FirstSetting();

        this.SetListener(GameObserverType.Game.Timer, UpdateUI);

        GetTextMesh(Texts.DayText).text = "Day " + day;
        UpdateUI();
    }

    public void FirstSetting()
    {
        if (isInit) return;

        isInit = true;

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton(Buttons.AttendenceBtn).AddButtonEvent(() =>
        {
            // 출석체크 팝업
            if (Managers.Attendance.TryClaimToday(out var items))
            {
                // 출석체크 성공
                foreach (var item in items)
                {
                    Managers.UI.GenerateUIParticle(transform, item.Item2, item.Item1);
                }
            }

            UpdateUI();
        }, false);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (Managers.Attendance.CanClaimDay(day, out var reason))
        {
            GetImage(Images.Black).gameObject.SetActive(false);
            GetImage(Images.CheckImage).gameObject.SetActive(false);
            GetImage(Images.Reddot).gameObject.SetActive(true);

        }
        else
        {
            if (reason == "already claimed")
            {
                GetImage(Images.Black).gameObject.SetActive(true);
                GetImage(Images.CheckImage).gameObject.SetActive(true);
                GetImage(Images.Reddot).gameObject.SetActive(false);
            }
            else if (reason == "not yet")
            {
                GetImage(Images.Black).gameObject.SetActive(false);
                GetImage(Images.CheckImage).gameObject.SetActive(false);
                GetImage(Images.Reddot).gameObject.SetActive(false);
            }
        }
    }
}
