using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocalDataManager
{
    public bool IsSave = false;

    // 캐릭터 소유 정보 (비트 플래그)
    public int OwnedCharactorFlags
    {
        get => PlayerPrefs.GetInt("OwnedCharactorFlags", 0);
        set { PlayerPrefs.SetInt("OwnedCharactorFlags", value); IsSave = true; }
    }

    // 특정 캐릭터를 소유하고 있는지 확인
    public bool HasCharactor(Define.CharactorType type)
    {
        int flags = OwnedCharactorFlags;
        int bit = 1 << (int)type;
        return (flags & bit) != 0;
    }

    // 특정 캐릭터를 소유 상태로 변경
    public void SetCharactorOwned(Define.CharactorType type, bool owned)
    {
        int flags = OwnedCharactorFlags;
        int bit = 1 << (int)type;
        if (owned)
            flags |= bit;
        else
            flags &= ~bit;
        OwnedCharactorFlags = flags;
    }

    // 소유한 캐릭터 개수 반환
    public int OwnedCharactorCount
    {
        get
        {
            int count = 0;
            int flags = OwnedCharactorFlags;
            for (int i = 0; i < System.Enum.GetValues(typeof(Define.CharactorType)).Length; i++)
            {
                if ((flags & (1 << i)) != 0)
                    count++;
            }
            return count;
        }
    }

    public bool UseHaptic
    {
        get => PlayerPrefs.GetInt("UseHaptic", 0) == 0;
        set { PlayerPrefs.SetInt("UseHaptic", value ? 0 : 1); IsSave = true; }
    }

    public bool UseSound
    {
        get => PlayerPrefs.GetInt("UseSound", 0) == 0;
        set { PlayerPrefs.SetInt("UseSound", value ? 0 : 1); IsSave = true; }
    }

    public int LanguageIndex
    {
        get => PlayerPrefs.GetInt("LanguageIndex", -1);
        set { PlayerPrefs.SetInt("LanguageIndex", value); IsSave = true; }
    }

    public int PlayerWinCount
    {
        get => PlayerPrefs.GetInt("PlayerWinCount", 0);
        set { PlayerPrefs.SetInt("PlayerWinCount", value); IsSave = true; }
    }

    public int PlayerRankingPoint
    {
        get => PlayerPrefs.GetInt("PlayerRankingPoint", 0);
        set { PlayerPrefs.SetInt("PlayerRankingPoint", value); IsSave = true; }
    }

    public int PlayerGemCount
    {
        get => PlayerPrefs.GetInt("PlayerGemCount", 0);
        set { PlayerPrefs.SetInt("PlayerGemCount", value); IsSave = true; GameObserver.Call(GameObserverType.Game.OnChangeGemCount); }
    }

    public int PlayerTicketCount
    {
        get => PlayerPrefs.GetInt("PlayerTicketCount", 0);
        set { PlayerPrefs.SetInt("PlayerTicketCount", value); IsSave = true; GameObserver.Call(GameObserverType.Game.OnChangeTicketCount); }
    }
}
