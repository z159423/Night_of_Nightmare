using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
public class CameraManager : MonoBehaviour
{

    public static CameraManager instance;

    public CinemachineVirtualCamera homeCamera;
    public CinemachineVirtualCamera mapCamera;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // this.SetListener(GameObserverType.Game.StartStage, () =>
        // {
        //     ChangeCamera_PlayCamera();
        // });

        // this.SetListener(GameObserverType.Game.GoMainMenu, () =>
        // {
        //     ChangeCamera_MainMenu();
        // });
    }

    public void ChangeCamera_MainMenu()
    {
        homeCamera.Priority = 2;
        mapCamera.Priority = 1;
    }

    public void ChangeCamera_PlayCamera()
    {
        homeCamera.Priority = 1;
        mapCamera.Priority = 2;
    }


    public void TurnVinettaEffect(bool isOn)
    {
        Camera.main.GetComponent<PostProcessVolume>().enabled = isOn;
    }
}
