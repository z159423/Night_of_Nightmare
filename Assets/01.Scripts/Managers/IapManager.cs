using System;
using System.Collections;
using System.Collections.Generic;
using LongriverSDKNS;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Networking;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

public class IapManager : MonoBehaviour, IStoreListener, IPurchaseItemsListener
{
    public bool init = false;
    IStoreController m_StoreController; // The Unity Purchasing system.
    IExtensionProvider m_Extension;

    public string environment = "production";


    // IStoreListener required methods
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"IAP Initialization Failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP Initialization Failed: {error}, Message: {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log($"ProcessPurchase called: {args.purchasedProduct.definition.id}");
        // OnPurchaseIap(args.purchasedProduct.definition.id);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed: {product.definition.id}, Reason: {failureReason}");
    }

    async void Awake()
    {
        try
        {
#if UNITY_IOS
            var options = new InitializationOptions()
                .SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);
#endif

            InitializePurchasing();

            StartCoroutine(InitIAP());
        }
        catch (Exception e)
        {
            Debug.LogError("Error initializing Unity Services: " + e.Message);
        }
    }

    public void InitializePurchasing()
    {
        Debug.Log("Initializing IAP...");

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct("ad_ticket_1", ProductType.Consumable, new IDs
        {
            { "ad_ticket_1", AppleAppStore.Name },
            { "ad_ticket_1", GooglePlay.Name }
        });

        builder.AddProduct("ad_ticket_2", ProductType.Consumable, new IDs
        {
            { "ad_ticket_2", AppleAppStore.Name },
            { "ad_ticket_2", GooglePlay.Name }
        });

        builder.AddProduct("ad_ticket_3", ProductType.Consumable, new IDs
        {
            { "ad_ticket_3", AppleAppStore.Name },
            { "ad_ticket_3", GooglePlay.Name }
        });

        builder.AddProduct("gem_1", ProductType.Consumable, new IDs
        {
            { "gem_1", AppleAppStore.Name },
            { "gem_1", GooglePlay.Name }
        });

        builder.AddProduct("gem_2", ProductType.Consumable, new IDs
        {
            { "gem_2", AppleAppStore.Name },
            { "gem_2", GooglePlay.Name }
        });

        builder.AddProduct("gem_3", ProductType.Consumable, new IDs
        {
            { "gem_3", AppleAppStore.Name },
            { "gem_3", GooglePlay.Name }
        });

        builder.AddProduct("boost_pack_1", ProductType.Consumable, new IDs
        {
            { "boost_pack_1", AppleAppStore.Name },
            { "boost_pack_1", GooglePlay.Name }
        });

        builder.AddProduct("character_lampgirl", ProductType.Consumable, new IDs
        {
            { "character_lampgirl", AppleAppStore.Name },
            { "character_lampgirl", GooglePlay.Name }
        });

        builder.AddProduct("character_scientist", ProductType.Consumable, new IDs
        {
            { "character_scientist", AppleAppStore.Name },
            { "character_scientist", GooglePlay.Name }
        });

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_Extension != null;
    }

    IEnumerator InitIAP()
    {
        while (!IsInitialized())
        {
            yield return new WaitForSeconds(0.1f);
        }

        init = true;
        Debug.Log("IAP 초기화 완료");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("In-App Purchasing successfully initialized");

        this.m_StoreController = controller;
        this.m_Extension = extensions;

        init = true;

        //UpdateUI();
    }

    public void OnPurchaseIap(string productId, Action onAfterPurchase = null)
    {
        // 구매 완료 후 처리 로직
        Debug.Log($"구매 완료: {productId}");
        // 예: 아이템 지급, UI 업데이트 등

        switch (productId)
        {
            case "ad_ticket_1":
                Managers.LocalData.PlayerRvTicketCount += 5;
                break;

            case "ad_ticket_2":
                Managers.LocalData.PlayerRvTicketCount += 30;
                break;

            case "ad_ticket_3":
                Managers.LocalData.PlayerRvTicketCount += 100;
                break;

            case "gem_1":
                Managers.LocalData.PlayerGemCount += 1500;
                break;

            case "gem_2":
                Managers.LocalData.PlayerGemCount += 5000;
                break;

            case "gem_3":
                Managers.LocalData.PlayerGemCount += 11000;
                break;

            case "boost_pack_1":
                Managers.LocalData.PlayerLampCount += 4;
                Managers.LocalData.playerHammerCount += 4;
                Managers.LocalData.PlayerHolyShieldCount += 4;
                Managers.LocalData.PlayerOverHeatCount += 4;
                break;
        }

        Managers.Audio.PlaySound("snd_get_item");
        onAfterPurchase?.Invoke();
    }

    // Implementing IPurchaseItemsListener interface methods
    public void getPurchaseItems(PurchaseItems purchaseItems)
    {
        foreach (var one in purchaseItems.unconsumeItems)
        {
            Debug.Log("find unconsume item" + one.itemId + " " + one.gameOrderId + " and ready to consume");
            // 订单消耗接口实际情况需要等待查单成功后调用(这里只作接口展示)
            LongriverSDKUserPayment.instance.consumeItem(one.gameOrderId);
            Debug.Log("success to unconsume item" + one.itemId + " " + one.gameOrderId);
        }
    }

    public void getOneTimeItems(OneTimeItemList items)
    {
        // TODO: Add your logic here
        Debug.Log("getOneTimeItems called.");
    }

    public void getSubscriptionItems(SubscriptionData data)
    {
        // TODO: Add your logic here
        Debug.Log("getSubscriptionItems called.");
    }

    public void PurchaseStart(string productId, Action onAfterPurchase = null)
    {
        LongriverSDKUserPayment.instance.startPayment(productId, "", (StartPaymentResult r) =>
        {
            OnPurchaseIap(productId, () =>
            {
                Debug.Log($"Purchase completed for product: {productId}");
                onAfterPurchase?.Invoke();

                SendToDiscord($"결제: 👻 악몽의밤, {productId}, {GetLocalizedPrice(productId)}, 오늘 밤은 치킨이다!!");

                // LongriverSDKUserPayment.instance.consumeItem(r.gameOrderId);
            });
        }, (State s) =>
        {
            Debug.LogError($"Purchase failed for product: {productId}, State: {s}");
        });
    }

    public void RestorePurchase()
    {
        LongriverSDKUserPayment.instance.restorePurchases();
    }

    public string GetLocalizedPrice(string productKey)
    {
        Product product = m_StoreController.products.WithID(productKey);
        if (product != null && product.metadata != null)
        {
            string price = product.metadata.localizedPriceString;  // 현지화된 가격 문자열
            return price;
        }
        else
        {
            Debug.LogError("Product not found or metadata not available.");
            return string.Empty;
        }

        if (init)
        {

        }
        else
        {
            Debug.LogError("IAP not initialized.");

            return string.Empty;
        }
    }

    // Webhook URL
    private string webhookUrl = "https://discord.com/api/webhooks/1360623729884926234/8SbVz9zylaP1Zx2MK3VYjNldb1fZbH-ImMQj4ZB9JNyl-h3S5IPsEyzIV5gqo0DTYjDn";

    public void SendToDiscord(string message)
    {
        StartCoroutine(SendWebhook(message));
    }

    private IEnumerator SendWebhook(string message)
    {
        // JSON 구성
        string jsonPayload = JsonUtility.ToJson(new DiscordMessage { content = message });

        // 요청 생성
        using (UnityWebRequest request = new UnityWebRequest(webhookUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 요청 전송
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Discord Webhook 전송 성공!");
            }
            else
            {
                Debug.LogWarning("Discord Webhook 전송 실패: " + request.error);
            }
        }
    }

    // 메시지 포맷 클래스
    [System.Serializable]
    private class DiscordMessage
    {
        public string content;
    }
}
