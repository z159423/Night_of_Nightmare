using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public void ShowRewardAd(Action onComplete)
    {
        if (Managers.LocalData.PlayerRvTicketCount > 0)
        {
            Managers.LocalData.PlayerRvTicketCount--;
            onComplete?.Invoke();
        }
        else
        {
            //TODO 광고 호출하고 광고가 끝나면 onComplete 호출
            onComplete?.Invoke();
        }
    }
}
