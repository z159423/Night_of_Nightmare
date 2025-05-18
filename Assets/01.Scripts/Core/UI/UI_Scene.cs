using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public abstract class UI_Scene : UI_Base
{
    GraphicRaycaster _graphicRaycaster;

    public GraphicRaycaster GraphicRaycaster { get => _graphicRaycaster; }

    public override void Init()
	{
        _graphicRaycaster = GetComponent<GraphicRaycaster>();

        Managers.UI.SetCanvas(gameObject, false);

        // this.SetListener(GameObserverType.Data.Fever, OnFeverUpdate);
    }

    public abstract void Show();
    public abstract void Hide();

    /// <summary>
    /// Looting Tutorial 예외상황 방지용
    /// </summary>
    /// <returns></returns>
    public Button GetSceneButton(Enum buttonType)
    {
        return GetButton(buttonType);
    }
}
