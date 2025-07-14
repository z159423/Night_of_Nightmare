using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LongriverSDKNS
{
    [Serializable]
    public class CallbackInfo
    {
        private double revenue;
        private string network;
        private string placementId;
        private string adsourceId;

        public CallbackInfo()
        {

        }

        public Dictionary<string, string> GetAdditionInfo()
        {
            return new Dictionary<string, string>();
        }

        public double GetRevenue()
        {
            return revenue;
        }

        public string GetAdSourceId()
        {
            return adsourceId;
        }

        public string GetNetworkFirmId()
        {
            return network;
        }

        public string GetNetworkPlacement()
        {
            return placementId;
        }
    }
}

