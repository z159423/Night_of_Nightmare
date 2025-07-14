using System;
using System.Collections;
using System.Collections.Generic;
using LongriverSDKNS;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public Action rewardAdCompleteCallback;

    // 리워드 비디오 리스너 클래스
    public class RewardVideoListener : LongriverSDKRewardedVideoListener
    {
        private AdManager adManager;
        
        public RewardVideoListener(AdManager adManager)
        {
            this.adManager = adManager;
        }
        
        public void onRewardedVideoAdPlayStart(string adEntry, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: reward video start");
        }
        
        public void onRewardedVideoAdPlayFail(string adEntry, string code, string message)
        {
            Debug.Log($"AdManager: reward video play fail - code: {code}, message: {message}");
        }
        
        public void onRewardedVideoAdPlayClicked(string adEntry, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: reward video click");
        }

        public void onRewardedVideoAdPlayClosed(string unitId, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: reward video close");
        }

        public void onReward(string unitId, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: give reward in onReward");
            adManager.rewardAdCompleteCallback?.Invoke();
        }

        public void onRewardedVideoAdLoaded(string unitId, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: reward video loaded");
        }

        public void onRewardedVideoAdLoadFailed(string unitId, int errorCode, string errorMsg)
        {
            Debug.Log($"AdManager: reward video load failed - code: {errorCode}, message: {errorMsg}");
        }
    }

    void Start()
    {
        // 리워드 비디오 리스너 설정
        LongriverSDKAd.instance.SetLongriverSDKRewardedVideoListener(new RewardVideoListener(this));
        Debug.Log("AdManager: Reward video listener set");
    }

    public void ShowRewardAd(Action onComplete)
    {
        if (Managers.LocalData.PlayerRvTicketCount > 0)
        {
            Managers.LocalData.PlayerRvTicketCount--;
            onComplete?.Invoke();
        }
        else
        {
            // 광고가 준비되었는지 먼저 확인
            bool hasReward = LongriverSDKAd.instance.HasReward("");
            Debug.Log($"AdManager: HasReward check: {hasReward}");
            
            if (hasReward)
            {
                // 광고 호출하고 광고가 끝나면 onComplete 호출
                rewardAdCompleteCallback = onComplete;
                Debug.Log("AdManager: Calling ShowReward");
                LongriverSDKAd.instance.ShowReward("");
            }
            else
            {
                Debug.Log("AdManager: No reward ad available");
                // 광고가 없을 때의 처리 (선택사항)
                onComplete?.Invoke(); // 또는 에러 처리
            }
        }
    }
}