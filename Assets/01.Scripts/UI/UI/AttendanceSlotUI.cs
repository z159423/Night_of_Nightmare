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

    }

    bool isInit;

    public int day = 0;

    public override void Init()
    {
        FirstSetting();

        this.SetListener(GameObserverType.Game.Timer, CheckReddot);
        UpdateUI();
    }

    public void FirstSetting()
    {
        if (!isInit) return;

        isInit = true;

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton(Buttons.AttendenceBtn).AddButtonEvent(() =>
        {
            // 출석체크 팝업
            Managers.Attendance.TryClaimToday(out var reason);
            UpdateUI();
        }, false);
    }

    public void UpdateUI()
    {
        if (Managers.Attendance.CanClaimDay(day, out var reason))
        {
            GetImage(Images.Black).gameObject.SetActive(false);
            GetImage(Images.CheckImage).gameObject.SetActive(false);
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

    public void CheckReddot()
    {
        if (Managers.Attendance.CanClaimToday(out var reason))
        {
            GetImage(Images.Reddot).gameObject.SetActive(true);
        }
        else
        {
            GetImage(Images.Reddot).gameObject.SetActive(false);
        }
    }
}
