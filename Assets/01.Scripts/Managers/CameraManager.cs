using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{

    public static CameraManager instance;

    public CinemachineVirtualCamera mainMenuCamera;
    public CinemachineVirtualCamera playCamera;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        this.SetListener(GameObserverType.Game.StartStage, () =>
        {
            ChangeCamera_PlayCamera();
        });

        this.SetListener(GameObserverType.Game.GoMainMenu, () =>
        {
            ChangeCamera_MainMenu();
        });
    }

    public void ChangeCamera_MainMenu()
    {
        mainMenuCamera.Priority = 2;
        playCamera.Priority = 1;
    }

    public void ChangeCamera_PlayCamera()
    {
        mainMenuCamera.Priority = 1;
        playCamera.Priority = 2;
    }
}
