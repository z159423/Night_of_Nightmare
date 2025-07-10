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
        get => PlayerPrefs.GetInt("OwnedCharactorFlags", 1);
        set { PlayerPrefs.SetInt("OwnedCharactorFlags", value); IsSave = true; }
    }

    public int SelectedCharactor
    {
        get => PlayerPrefs.GetInt("SelectedCharactor", (int)Define.CharactorType.Farmer);
        set { PlayerPrefs.SetInt("SelectedCharactor", value); IsSave = true; GameObserver.Call(GameObserverType.Game.OnChangeCharactor); }
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

        GameObserver.Call(GameObserverType.Game.OnChangeCharactor);
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

    public int PlayerRvTicketCount
    {
        get => PlayerPrefs.GetInt("PlayerRvTicketCount", 0);
        set { PlayerPrefs.SetInt("PlayerRvTicketCount", value); IsSave = true; GameObserver.Call(GameObserverType.Game.OnChangeTicketCount); }
    }

    public int PlayerLampCount
    {
        get => PlayerPrefs.GetInt("PlayerLampCount", 4);
        set { PlayerPrefs.SetInt("PlayerLampCount", value); IsSave = true; GameObserver.Call(GameObserverType.Game.OnChangeLampCount); }
    }

    public int playerHammerCount
    {
        get => PlayerPrefs.GetInt("PlayerHammerCount", 2);
        set { PlayerPrefs.SetInt("PlayerHammerCount", value); IsSave = true; GameObserver.Call(GameObserverType.Game.OnChangeHammerCount); }
    }

    public int PlayerHolyShieldCount
    {
        get => PlayerPrefs.GetInt("PlayerHolyShieldCount", 2);
        set { PlayerPrefs.SetInt("PlayerHolyShieldCount", value); IsSave = true; GameObserver.Call(GameObserverType.Game.OnChangeHolyShieldCount); }
    }

    public int PlayerOverHeatCount
    {
        get => PlayerPrefs.GetInt("PlayerOverHeatCount", 2);
        set { PlayerPrefs.SetInt("PlayerOverHeatCount", value); IsSave = true; GameObserver.Call(GameObserverType.Game.OnChangeOverHeatCount); }
    }

    public int ChallengeModeLevel
    {
        get => PlayerPrefs.GetInt("ChallengeModeLevel", 1);
        set { PlayerPrefs.SetInt("ChallengeModeLevel", value); IsSave = true; }
    }

    public void AddBoostItem(Define.BoostType type, int count)
    {
        switch (type)
        {
            case Define.BoostType.Lamp:
                PlayerLampCount += count;
                break;
            case Define.BoostType.HammerThrow:
                playerHammerCount += count;
                break;
            case Define.BoostType.HolyProtection:
                PlayerHolyShieldCount += count;
                break;
            case Define.BoostType.Overheat:
                PlayerOverHeatCount += count;
                break;
            default:
                Debug.LogWarning($"Unknown boost type: {type}");
                break;
        }

        GameObserver.Call(GameObserverType.Game.OnChangeBoostItemCount);
    }

    public int GetBoostItemCount(Define.BoostType type)
    {
        return type switch
        {
            Define.BoostType.Lamp => PlayerLampCount,
            Define.BoostType.HammerThrow => playerHammerCount,
            Define.BoostType.HolyProtection => PlayerHolyShieldCount,
            Define.BoostType.Overheat => PlayerOverHeatCount,
            _ => 0
        };
    }

    public int PlayerTutorialStep
    {
        get => PlayerPrefs.GetInt("PlayerTutorialStep", 0);
        set { PlayerPrefs.SetInt("PlayerTutorialStep", value); IsSave = true; }
    }

    public int FirstUseLamp
    {
        get => PlayerPrefs.GetInt("FirstUseLamp", 0);
        set { PlayerPrefs.SetInt("FirstUseLamp", value); IsSave = true; }
    }

    public int CheatMode
    {
        get => PlayerPrefs.GetInt("CheatMode", 0);
        set { PlayerPrefs.SetInt("CheatMode", value); IsSave = true; GameObserver.Call(GameObserverType.Game.OnCheatModeOn); }
    }
}
