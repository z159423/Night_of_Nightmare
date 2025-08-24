using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attendance_Popup : UI_Popup
{
    enum Buttons
    {
    }

    enum Images
    {
        TouchGuard
    }

    enum Texts
    {

    }

    AttendanceSlotUI[] attendanceSlotUI;

    public override void Init()
    {
        base.Init();

        OpenAnimation();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        attendanceSlotUI = GetComponentsInChildren<AttendanceSlotUI>();
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

    private void Exit()
    {
        ClosePopupUI();
    }
}
