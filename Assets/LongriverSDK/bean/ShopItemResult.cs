using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace LongriverSDKNS
{
    [Serializable]
    public class ShopItemResult
    {
        [UnityEngine.SerializeField]
        public List<ShopItem> items = new List<ShopItem>();

        public string GetFormattedPrice(string itemId)
        {
            foreach (var item in items)
            {
                if (item.itemId == itemId)
                {
                    return item.formattedPrice;
                }
            }
            return string.Empty;
        }
    }
}

