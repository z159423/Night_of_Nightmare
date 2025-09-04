using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AbilityManager : MonoBehaviour
{
    // 구매한 능력들 (인덱스만 저장 - 각 능력은 한 번만 구매 가능)
    private List<int> purchasedAbilityIndices = new List<int>();

    // 추가 능력들 (티어별 특별 능력)
    private List<int> purchasedAdditionalAbilities = new List<int>();

    private AbilityData abilityData;

    void Start()
    {
        abilityData = Managers.Resource.Load<AbilityData>("AbilityData/AbilityData");
        LoadPlayerAbilities();
    }

    public bool CanPurchaseAbility(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilityData.abilities.Count)
            return false;

        // 이미 구매했는지 체크
        if (purchasedAbilityIndices.Contains(abilityIndex))
        {
            Debug.Log("이미 구매한 능력입니다.");
            return false;
        }

        var ability = abilityData.abilities[abilityIndex];

        // 플레이어 티어 체크
        var playerTier = Define.GetPlayerCurrentTier();
        if (!IsTierHigherOrEqual(playerTier, ability.needTier))
        {
            Debug.Log($"티어가 부족합니다. 필요: {ability.needTier}, 현재: {playerTier}");
            return false;
        }

        // 골드 체크
        if (Managers.LocalData.PlayerGemCount < ability.cost)
        {
            Debug.Log($"골드가 부족합니다. 필요: {ability.cost}, 현재: {Managers.LocalData.PlayerGemCount}");
            return false;
        }

        return true;
    }

    public bool PurchaseAbility(int abilityIndex)
    {
        if (!CanPurchaseAbility(abilityIndex))
            return false;

        var ability = abilityData.abilities[abilityIndex];

        // 골드 차감
        Managers.LocalData.PlayerGemCount -= ability.cost;
        GameObserver.Call(GameObserverType.Game.OnChangeGemCount);

        Managers.Audio.PlaySound("snd_get_item");

        // 능력 추가
        AddAbility(abilityIndex);

        Debug.Log($"능력 구매 완료: {ability.type} (+{ability.value})");
        return true;
    }

    public bool CanPurchaseAdditionalAbility(int additionalAbilityIndex)
    {
        if (additionalAbilityIndex < 0 || additionalAbilityIndex >= abilityData.additionalAbilities.Count)
            return false;

        // 이미 구매했는지 체크
        if (purchasedAdditionalAbilities.Contains(additionalAbilityIndex))
        {
            Debug.Log("이미 구매한 추가 능력입니다.");
            return false;
        }

        var ability = abilityData.additionalAbilities[additionalAbilityIndex];

        // 플레이어 티어 체크
        var playerTier = Define.GetPlayerCurrentTier();
        if (!IsTierHigherOrEqual(playerTier, ability.needTier))
        {
            Debug.Log($"티어가 부족합니다. 필요: {ability.needTier}, 현재: {playerTier}");
            return false;
        }

        // 골드 체크
        if (Managers.LocalData.PlayerGemCount < ability.cost)
        {
            Debug.Log($"골드가 부족합니다. 필요: {ability.cost}, 현재: {Managers.LocalData.PlayerGemCount}");
            return false;
        }

        return true;
    }

    public bool PurchaseAdditionalAbility(int additionalAbilityIndex)
    {
        if (!CanPurchaseAdditionalAbility(additionalAbilityIndex))
            return false;

        var ability = abilityData.additionalAbilities[additionalAbilityIndex];

        // 골드 차감
        Managers.LocalData.PlayerGemCount -= ability.cost;
        GameObserver.Call(GameObserverType.Game.OnChangeGemCount);

        Managers.Audio.PlaySound("snd_get_item");

        // 추가 능력 구매
        purchasedAdditionalAbilities.Add(additionalAbilityIndex);
        SavePlayerAbilities();

        Debug.Log($"추가 능력 구매 완료: {ability.type} (+{ability.value})");
        GameObserver.Call(GameObserverType.Game.OnAbilityChanged);
        return true;
    }

    private void AddAbility(int abilityIndex)
    {
        // 능력 인덱스만 추가 (중복 체크는 CanPurchaseAbility에서 이미 함)
        purchasedAbilityIndices.Add(abilityIndex);

        // 로컬 데이터에 저장
        SavePlayerAbilities();

        // 능력 적용 이벤트 호출
        GameObserver.Call(GameObserverType.Game.OnAbilityChanged);
    }

    // 특정 타입의 능력 총합 값 반환
    public float GetAbilityValue(AbilityType type)
    {
        float totalValue = 0f;

        // 기본 능력들에서 계산
        foreach (var abilityIndex in purchasedAbilityIndices)
        {
            var ability = abilityData.abilities[abilityIndex];
            if (ability.type == type)
            {
                totalValue += ability.value;
            }
        }

        // 추가 능력들에서 계산
        foreach (var additionalIndex in purchasedAdditionalAbilities)
        {
            var ability = abilityData.additionalAbilities[additionalIndex];
            if (ability.type == type)
            {
                totalValue += ability.value;
            }
        }

        return totalValue;
    }

    // 특정 능력의 구매 여부 확인
    public bool HasAbility(int abilityIndex)
    {
        return purchasedAbilityIndices.Contains(abilityIndex);
    }

    // 추가 능력 구매 여부 확인
    public bool HasAdditionalAbility(int additionalAbilityIndex)
    {
        return purchasedAdditionalAbilities.Contains(additionalAbilityIndex);
    }

    // 구매한 모든 능력 정보 반환 (디버그/UI용)
    public List<Ability> GetAllPurchasedAbilities()
    {
        var result = new List<Ability>();

        foreach (var abilityIndex in purchasedAbilityIndices)
        {
            result.Add(abilityData.abilities[abilityIndex]);
        }

        return result;
    }

    // 구매한 모든 추가 능력 반환
    public List<Ability> GetAllPurchasedAdditionalAbilities()
    {
        var result = new List<Ability>();

        foreach (var index in purchasedAdditionalAbilities)
        {
            result.Add(abilityData.additionalAbilities[index]);
        }

        return result;
    }

    // 구매 가능한 능력들 반환 (현재 티어와 골드로 구매 가능한 것들)
    public List<int> GetPurchasableAbilityIndices()
    {
        var result = new List<int>();

        for (int i = 0; i < abilityData.abilities.Count; i++)
        {
            if (CanPurchaseAbility(i))
            {
                result.Add(i);
            }
        }

        return result;
    }

    // 구매 가능한 추가 능력들 반환
    public List<int> GetPurchasableAdditionalAbilityIndices()
    {
        var result = new List<int>();

        for (int i = 0; i < abilityData.additionalAbilities.Count; i++)
        {
            if (CanPurchaseAdditionalAbility(i))
            {
                result.Add(i);
            }
        }

        return result;
    }

    // 능력 초기화 (디버그/치트용)
    public void ResetAllAbilities()
    {
        purchasedAbilityIndices.Clear();
        purchasedAdditionalAbilities.Clear();
        SavePlayerAbilities();
        GameObserver.Call(GameObserverType.Game.OnAbilityChanged);
        Debug.Log("모든 능력이 초기화되었습니다.");
    }

    private void SavePlayerAbilities()
    {
        // 기본 능력들 저장 (인덱스만)
        PlayerPrefs.SetString("PurchasedAbilityIndices", string.Join(",", purchasedAbilityIndices));

        // 추가 능력들 저장 (인덱스만)
        PlayerPrefs.SetString("PurchasedAdditionalAbilities", string.Join(",", purchasedAdditionalAbilities));

        PlayerPrefs.Save();
    }

    private void LoadPlayerAbilities()
    {
        purchasedAbilityIndices.Clear();
        purchasedAdditionalAbilities.Clear();

        try
        {
            // 기본 능력들 로드
            var indicesStr = PlayerPrefs.GetString("PurchasedAbilityIndices", "");
            if (!string.IsNullOrEmpty(indicesStr))
            {
                purchasedAbilityIndices = indicesStr.Split(',').Select(int.Parse).ToList();
            }

            // 추가 능력들 로드
            var additionalStr = PlayerPrefs.GetString("PurchasedAdditionalAbilities", "");
            if (!string.IsNullOrEmpty(additionalStr))
            {
                purchasedAdditionalAbilities = additionalStr.Split(',').Select(int.Parse).ToList();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"능력 데이터 로드 실패: {e.Message}");
            purchasedAbilityIndices.Clear();
            purchasedAdditionalAbilities.Clear();
        }

        Debug.Log($"플레이어 능력 로드 완료: 기본 {purchasedAbilityIndices.Count}개, 추가 {purchasedAdditionalAbilities.Count}개");
    }

    private bool IsTierHigherOrEqual(Define.Tier current, Define.Tier required)
    {
        return (int)current >= (int)required;
    }

    public Ability GetAbility(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilityData.abilities.Count)
            return null;
        return abilityData.abilities[abilityIndex];
    }

    public Ability GetAdditionalAbility(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilityData.additionalAbilities.Count)
            return null;
        return abilityData.additionalAbilities[abilityIndex];
    }

    public float GetHasAbilityValueSum(AbilityType type)
    {
        float sum = 0;

        foreach (var ability in GetAllPurchasedAbilities())
        {
            if (ability.type == type)
            {
                sum += ability.value;
            }
        }

        foreach (var ability in GetAllPurchasedAdditionalAbilities())
        {
            if (ability.type == type)
            {
                sum += ability.value;
            }
        }

        return sum;
    }
}
