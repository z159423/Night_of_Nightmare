using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Login : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var loading = Managers.UI.ShowSceneUI<UI_GameScene_Loading>();
        loading.Init();
    }
}
