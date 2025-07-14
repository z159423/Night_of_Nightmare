﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace LongriverSDKNS
{
    [Serializable]
    public class AttributionInfo
    {
        [UnityEngine.SerializeField]
        public string attrid = "";
        [UnityEngine.SerializeField]
        public string network = "";
        [UnityEngine.SerializeField]
        public string campaign = "";
        [UnityEngine.SerializeField]
        public string adgroup = "";
        [UnityEngine.SerializeField]
        public string creative = "";
        //private string NullOrString(string s)
        //{
        //    if (s != null && s.Length > 0) return s;
        //    else return "";
        //}
        public string ToJson()
        {
            //var d = new Dictionary<string, string>();
            //d.Add("attrid", NullOrString(attrid));
            //d.Add("network", NullOrString(network));
            //d.Add("campaign", NullOrString(campaign));
            //d.Add("adgroup", NullOrString(adgroup));
            //d.Add("creative", NullOrString(creative));

            return JsonUtility.ToJson(this);
        }
        //static public string GetStringFromDict(Dictionary<string, object> d, string key)
        //{
        //    if (d.ContainsKey(key))
        //    {
        //        return (string)d[key];
        //    }
        //    else return "unknow";
        //}
        static public AttributionInfo FromJson(string str)
        {
            //var d = (Dictionary<string, object>)Json.Deserialize(str);
            //var attrid = GetStringFromDict(d, "attrid");
            //var network = GetStringFromDict(d, "network");
            //var campaign = GetStringFromDict(d, "campaign");
            //var adgroup = GetStringFromDict(d, "adgroup");
            //var creative = GetStringFromDict(d, "creative");

            //var info = new AttributionInfo()
            //{
            //    attrid = attrid,
            //    network = network,
            //    campaign = campaign,
            //    adgroup = adgroup,
            //    creative = creative,
            //};
            //return info;
            return JsonUtility.FromJson<AttributionInfo>(str);
        }
    }
    public class AttributionHelper
    {
        protected AttributionInfo info = null;
        private Action<AttributionInfo> attributionInfoDelegate;
        private bool hasCall = false;

        public void SetAttributionInfo(AttributionInfo info)
        {
            this.info = info;
            CallOnce();
        }

        public void SetAttributionInfoListener(Action<AttributionInfo> attributionInfoDelegate)
        {
            this.attributionInfoDelegate = attributionInfoDelegate;
            if (this.info != null)
            {
                CallOnce();
            }
        }

        public AttributionInfo GetAttributionInfo()
        {
            if (null == this.info && PlayerPrefs.HasKey("longriver_unity_attr_on")) 
            {
                string attrInfoJson = PlayerPrefs.GetString("longriver_unity_attr_on", "");
                if (!string.IsNullOrWhiteSpace(attrInfoJson))
                {
                    this.info = AttributionInfo.FromJson(attrInfoJson);
                }
            }
            return this.info;
        }

        public void CallOnce()
        {
            if (this.attributionInfoDelegate != null)
            {
                if (!hasCall)
                {
                    this.attributionInfoDelegate.Invoke(this.info);
                    hasCall = true;
                }
            }
        }

        static private AttributionHelper instance = new AttributionHelper();
        static public AttributionHelper GetInstance()
        {
            return instance;
        }
    }
}

