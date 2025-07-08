using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IapManager : MonoBehaviour
{
    public void OnPurchaseIap(string productId, Action onAfterPurchase = null)
    {
        // 구매 완료 후 처리 로직
        Debug.Log($"구매 완료: {productId}");
        // 예: 아이템 지급, UI 업데이트 등

        switch (productId)
        {
            case "RvTicket_5":
                Managers.LocalData.PlayerRvTicketCount += 5;
                break;

            case "RvTicket_30":
                Managers.LocalData.PlayerRvTicketCount += 30;
                break;

            case "RvTicket_100":
                Managers.LocalData.PlayerRvTicketCount += 100;
                break;

            case "Gem_1500":
                Managers.LocalData.PlayerGemCount += 1500;
                break;

            case "Gem_5000":
                Managers.LocalData.PlayerGemCount += 5000;
                break;

            case "Gem_11000":
                Managers.LocalData.PlayerGemCount += 11000;
                break;

            case "BoostPack":
                Managers.LocalData.PlayerLampCount += 4;
                Managers.LocalData.playerHammerCount += 4;
                Managers.LocalData.PlayerHolyShieldCount += 4;
                Managers.LocalData.PlayerOverHeatCount += 4;
                break;
        }

        Managers.Audio.PlaySound("snd_get_item");
        onAfterPurchase?.Invoke();
    }

    // public string GetLocalizedPrice(string productKey)
    // {
    //     if (initialized)
    //     {
    //         Product product = m_StoreController.products.WithID(productKey);
    //         if (product != null && product.metadata != null)
    //         {
    //             string price = product.metadata.localizedPriceString;  // 현지화된 가격 문자열
    //             print(price);
    //             return price;
    //         }
    //         else
    //         {
    //             Debug.LogError("Product not found or metadata not available.");
    //             return string.Empty;
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("IAP not initialized.");

    //         return string.Empty;
    //     }
    // }
}
