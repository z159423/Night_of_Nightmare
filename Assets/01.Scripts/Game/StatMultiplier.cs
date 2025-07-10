using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatMultiplier
{
    private List<float> multipliers = new List<float>();

    public float BaseValue { get; set; } = 1f;

    public Action<float> onValueChanged;

    public float Value
    {
        get
        {
            float result = BaseValue;
            foreach (var m in multipliers)
                result *= m;
            return result;
        }
    }

    public void AddMultiplier(float multiplier)
    {
        multipliers.Add(multiplier);

        if (onValueChanged != null)
            onValueChanged(Value);
    }

    public void RemoveMultiplier(float multiplier)
    {
        multipliers.Remove(multiplier);

        if (onValueChanged != null)
            onValueChanged(Value);
    }

    public void ClearMultipliers()
    {
        multipliers.Clear();

        if (onValueChanged != null)
            onValueChanged(Value);
    }
}
