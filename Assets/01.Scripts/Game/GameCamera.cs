using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Define;

public class GameCamera : MonoBehaviour
{
    public Define.GameMode cameraMode;

    void Start()
    {
        Managers.Camera.cameras.Add(cameraMode, GetComponent<Cinemachine.CinemachineVirtualCamera>());
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0) && Managers.UI._currentPopup is CharactorSelect_Popup charactorSelect_Popup1 && charactorSelect_Popup1 != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray);

            var charactorSelectIcon = hits.Where(w => w.transform.GetComponent<HomeCharactor>() != null);

            if (charactorSelectIcon.Count() > 0 && Managers.UI._currentPopup is CharactorSelect_Popup charactorSelect_Popup && charactorSelect_Popup1 != null)
            {
                var charactor = charactorSelectIcon.FirstOrDefault().transform.GetComponent<HomeCharactor>();

                if (Managers.UI._currentPopup is CharactorSelect_Popup popup)
                {
                    popup.SelectCharactorIcon(charactor.type);
                }
            }
        }
    }
}
