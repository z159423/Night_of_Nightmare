using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class NameGenerator
{
    static List<string> englishSentences = new List<string>();
    static List<string> koreanSentences = new List<string>();
    static List<string> chineseSentences = new List<string>();
    static List<string> englishTwoWords = new List<string>();
    static List<string> koreanTwoWords = new List<string>();
    static List<string> chineseOneChars = new List<string>();
    static List<string> englishChars = new List<string>();
    static List<string> chineseTwoWords = new List<string>();

    static bool isLoaded = false;

    static void LoadNamesFromCsv()
    {
        TextAsset nameData = Resources.Load<TextAsset>("NameData");
        if (nameData != null)
        {
            using (StringReader reader = new StringReader(nameData.text))
            {
                // 헤더 스킵
                string header = reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] columns = line.Split(',');
                    if (columns.Length >= 8)
                    {
                        englishSentences.Add(columns[0].Trim());
                        koreanSentences.Add(columns[1].Trim());
                        chineseSentences.Add(columns[2].Trim());
                        englishTwoWords.Add(columns[3].Trim());
                        koreanTwoWords.Add(columns[4].Trim());
                        chineseOneChars.Add(columns[5].Trim());
                        englishChars.Add(columns[6].Trim());
                        chineseTwoWords.Add(columns[7].Trim());
                    }
                }
            }
        }
        else
        {
            Debug.LogError("NameData.csv 파일을 Resources 폴더에 넣어주세요!");
        }

        isLoaded = true;
    }

    public static string GetRandomName()
    {
        if (!isLoaded)
            LoadNamesFromCsv();

        float langRand = Random.value;
        if (langRand < 0.7f)
            return GenerateEnglishName();
        else if (langRand < 0.92f)
            return GenerateKoreanName();
        else
            return GenerateChineseName();
    }

    public static string GenerateEnglishName()
    {
        float rand = Random.value;
        if (rand < 0.19f)
        {
            // 1문장 영어 랜덤
            return englishSentences[Random.Range(0, englishSentences.Count)];
        }
        else if (rand < 0.19f + 0.4f)
        {
            // 2단어 영어 조합
            string first = englishTwoWords[Random.Range(0, englishTwoWords.Count)];
            string second = englishTwoWords[Random.Range(0, englishTwoWords.Count)];
            bool space = Random.value < 0.5f;
            return space ? $"{first} {second}" : $"{first}{second}";
        }
        else
        {
            // 랜덤 알파벳/숫자/공백 조합 (최대 6자)
            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
            int len = Random.Range(1, 7);
            char[] result = new char[len];
            for (int i = 0; i < len; i++)
                result[i] = chars[Random.Range(0, chars.Length)];
            return new string(result).Trim();
        }
    }

    public static string GenerateKoreanName()
    {
        float rand = Random.value;
        if (rand < 0.3f)
        {
            // 1문장 한글 랜덤
            return koreanSentences[Random.Range(0, koreanSentences.Count)];
        }
        else
        {
            // 2단어 한글 조합
            string first = koreanTwoWords[Random.Range(0, koreanTwoWords.Count)];
            string second = koreanTwoWords[Random.Range(0, koreanTwoWords.Count)];
            bool space = Random.value < 0.5f;
            return space ? $"{first} {second}" : $"{first}{second}";
        }
    }

    public static string GenerateChineseName()
    {
        float rand = Random.value;
        if (rand < 0.3f)
        {
            // 1문장 중국어 랜덤
            return chineseSentences[Random.Range(0, chineseSentences.Count)];
        }
        else if (rand < 0.6f)
        {
            // 2단어 중국어 조합
            string first = chineseTwoWords[Random.Range(0, chineseTwoWords.Count)];
            string second = chineseTwoWords[Random.Range(0, chineseTwoWords.Count)];
            return $"{first}{second}";
        }
        else
        {
            // 1글자 중국어열에서 5글자 랜덤 조합
            string allChars = string.Concat(chineseOneChars);
            char[] result = new char[5];
            for (int i = 0; i < 5; i++)
                result[i] = allChars[Random.Range(0, allChars.Length)];
            return new string(result);
        }
    }
}
