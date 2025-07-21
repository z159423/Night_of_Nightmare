using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IapSetter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _costText;
    [SerializeField] Button _purcahseBtn;


    public string key;

    public System.Action onAfterPurchaseAction = null;

    private void OnValidate()
    {
        _costText = GetComponentInChildren<TextMeshProUGUI>();
        _purcahseBtn = GetComponentInChildren<Button>();
    }

    private void Start()
    {
        _purcahseBtn.AddButtonEvent(() =>
        {
            // #if !UNITY_EDITOR
            Managers.IAP.PurchaseStart(key, onAfterPurchaseAction);
            // #endif

            // #if UNITY_EDITOR
            //             IAPManager.instance.OnSuccessPurchased(key);
            // #endif
        });

        // #if !UNITY_EDITOR
        // if (_costText != null)
        //     _costText.text = IAPManager.instance.GetLocalizedPrice(key);
        // #endif
    }
}
