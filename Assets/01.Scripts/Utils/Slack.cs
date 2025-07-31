using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

using Newtonsoft.Json;
using UnityEngine;

public class Slack : SingletonStatic<Slack>
{
    // Slack Webhook URL (복사한 Webhook URL로 대체)
    private const string Slack_Url = "https://hooks.slack.com/services/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";

    public class SlackBlock
    {
        public string type { get; set; }
        public SlackText text { get; set; }
    }

    public class SlackText
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    public class SlackMessage
    {
        public string channel { get; set; }
        public string username { get; set; }
        public string text { get; set; }
        public List<SlackBlock> blocks { get; set; }
    }

    public static async void ThrowSlackMessage(string title, string e)
    {
        var message = new SlackMessage();
        message.channel = "client_error";
        message.username = "Client-Bot";
        message.text = "Client Error";
        message.blocks = new List<SlackBlock>
        {

            new SlackBlock()
            {
                type = "section",
                text = new SlackText()
                {
                    type = "mrkdwn",
                    text = $"*Platform* - {Application.platform} *Version* - {Application.version}"
                }
            },

            new SlackBlock()
            {
                type = "divider"
            },

            new SlackBlock()
            {
                type = "section",
                text = new SlackText()
                {
                    type = "mrkdwn",
                    text = $"*URL* - {title}"
                }
            },


            new SlackBlock()
            {
                type = "divider"
            },

            new SlackBlock()
            {
                type = "section",
                text = new SlackText()
                {
                    type = "mrkdwn",
                    text = $"```{e}```"
                }
            }
        };

        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var json = JsonConvert.SerializeObject(message, settings);

        Debug.Log(json);

        var _httpClient = new HttpClient(
            new HttpClientHandler()
            {
                Proxy = null,
                UseProxy = false
            }
        );
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(Slack_Url, content);
    }

}