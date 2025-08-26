using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Attendance_Popup : UI_Popup
{
    enum Buttons
    {
        BGExit,
        CloseBtn
    }

    enum Images
    {
        TouchGuard
    }

    enum Texts
    {
        RemainTimeText
    }

    AttendanceSlotUI[] attendanceSlotUI;

    public override void Init()
    {
        base.Init();

        OpenAnimation();

        this.SetListener(GameObserverType.Game.Timer, UpdateRemainTimeText);
        UpdateRemainTimeText();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        attendanceSlotUI = GetComponentsInChildren<AttendanceSlotUI>();

        for (int i = 0; i < attendanceSlotUI.Length; i++)
        {
            attendanceSlotUI[i].day = i + 1;
            attendanceSlotUI[i].Init();
        }

        GetButton(Buttons.BGExit).AddButtonEvent(() =>
        {
            ClosePopupUI();
        });

        GetButton(Buttons.CloseBtn).AddButtonEvent(() =>
        {
            ClosePopupUI();
        });
    }

    public override void Reset()
    {

    }

    public void UpdateUI()
    {
        for (int i = 0; i < attendanceSlotUI.Length; i++)
        {
            attendanceSlotUI[i].UpdateUI();
        }
    }

    void UpdateRemainTimeText()
    {
        GetTextMesh(Texts.RemainTimeText).text = Managers.Attendance.GetTimeUntilNextClaimString();
    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
