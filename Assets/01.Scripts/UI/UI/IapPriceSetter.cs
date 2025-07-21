using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IapPriceSetter : MonoBehaviour
{
    public string productID;

    private Text text;
    private TextMeshProUGUI tmp;

    private void Start()
    {
        if (GetComponent<Text>() != null)
            text = GetComponent<Text>();

        if (GetComponent<TextMeshProUGUI>() != null)
            tmp = GetComponent<TextMeshProUGUI>();

        if (text != null)
            text.text = Managers.IAP.GetLocalizedPrice(productID);

        if (tmp != null)
            tmp.text = Managers.IAP.GetLocalizedPrice(productID);
    }
}
