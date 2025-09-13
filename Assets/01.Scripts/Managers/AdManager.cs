using System;
using System.Collections;
using System.Collections.Generic;
using LongriverSDKNS;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public Action rewardAdCompleteCallback;

    // 리워드 비디오 리스너 클래스
    public class AdListener : LongriverSDKRewardedVideoListener, LongriverSDKInterstitialAdListener
    {
        private AdManager adManager;

        public AdListener(AdManager adManager)
        {
            this.adManager = adManager;
        }

        public void onRewardedVideoAdPlayStart(string adEntry, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: reward video start");
            // 광고 시작 시 게임 정지
            Time.timeScale = 0.0001f;
            Managers.Audio.InGameSoundPause(true);
        }

        public void onRewardedVideoAdPlayFail(string adEntry, string code, string message)
        {
            Debug.Log($"AdManager: reward video play fail - code: {code}, message: {message}");
            // 광고 실패 시 게임 재개
            Time.timeScale = 1;
            Managers.Audio.InGameSoundPause(false);
        }

        public void onRewardedVideoAdPlayClicked(string adEntry, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: reward video click");
        }

        public void onRewardedVideoAdPlayClosed(string unitId, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: reward video close");
            // 광고 종료 시 게임 재개
            Time.timeScale = 1;
            Managers.Audio.InGameSoundPause(false);
        }

        public void onReward(string unitId, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: give reward in onReward");
            adManager.rewardAdCompleteCallback?.Invoke();

            Managers.Firebase.GameEvent("RewardAd_Claim", adManager.adKeyword);
        }

        public void onRewardedVideoAdLoaded(string unitId, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: reward video loaded");
        }

        public void onRewardedVideoAdLoadFailed(string unitId, int errorCode, string errorMsg)
        {
            Debug.Log($"AdManager: reward video load failed - code: {errorCode}, message: {errorMsg}");
        }

        public void onInterstitialAdShow(string adEntry, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: interstitial ad shown");
            // 인터스티셜 광고 시작 시 게임 정지
            Time.timeScale = 0.0001f;
            Managers.Audio.InGameSoundPause(true);
        }
        /***
         * 广告关闭
         * @param unitId 广告位id
         */
        public void onInterstitialAdClose(string adEntry, CallbackInfo callbackInfo)
        {
            Debug.Log("AdManager: interstitial ad closed");
            // 인터스티셜 광고 종료 시 게임 재개
            Time.timeScale = 1;
            Managers.Audio.InGameSoundPause(false);
        }
        /***
         * 广告点击
         * @param unitId 广告位id
         */
        public void onInterstitialAdClick(string adEntry, CallbackInfo callbackInfo)
        {
        }

        public void onInterstitialAdLoaded(string unitId, CallbackInfo callbackInfo)
        {
        }

        public void onInterstitialAdLoadFailed(string unitId, int errorCode, string errorMsg)
        {

        }

        public void onInterstitialAdPlayFail(string unitId, int errorCode, string errorMsg)
        {
        }
    }

    private string adKeyword = "";

    void Start()
    {
        // 리워드 비디오 리스너 설정
        LongriverSDKAd.instance.SetLongriverSDKRewardedVideoListener(new AdListener(this));
        LongriverSDKAd.instance.SetLongriverSDKInterstitialAdListener(new AdListener(this));
        Debug.Log("AdManager: Reward video listener set");
    }

    public void ShowRewardAd(Action onComplete, string adKeyword = "")
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
                rewardAdCompleteCallback = null;
                rewardAdCompleteCallback = onComplete;
                Debug.Log("AdManager: Calling ShowReward");
                LongriverSDKAd.instance.ShowReward("");

                this.adKeyword = adKeyword;
            }
            else
            {
                Debug.Log("AdManager: No reward ad available");
                // 광고가 없을 때의 처리 (선택사항)
                onComplete?.Invoke(); // 또는 에러 처리
            }
        }
    }

    public void ShowInterstitialAd()
    {
        return;

        // 인터스티셜 광고 호출
        Debug.Log("AdManager: Interstitial ad shown");
        LongriverSDKAd.instance.ShowInterstitial("");
        // 광고 시작 시 게임 정지
        Time.timeScale = 0.0001f;
        Managers.Audio.InGameSoundPause(true);
    }
}