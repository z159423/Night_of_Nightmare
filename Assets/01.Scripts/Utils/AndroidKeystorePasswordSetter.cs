#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AndroidKeystorePasswordSetter : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android)
            return;

        Debug.Log("Android keystore password 자동 설정 중...");

        // 여기만 수정하세요
        string keystorePassword = "ojw930420";
        string keyaliasPassword = "ojw930420";

        PlayerSettings.Android.keystorePass = keystorePassword;
        PlayerSettings.Android.keyaliasPass = keyaliasPassword;

        Debug.Log("비밀번호 자동 설정 완료");
    }
}
#endif
