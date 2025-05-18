using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocalDataManager
{
    public bool IsSave = false;

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
}
