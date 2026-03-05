using UnityEditor;
using UnityEngine;

namespace CoolGameFramework.Editor
{
    /// <summary>
    /// 框架设置窗口
    /// </summary>
    public class FrameworkSettingsWindow : EditorWindow
    {
        private Vector2 _scrollPosition;

        [MenuItem("CoolGameFramework/Framework Settings")]
        public static void ShowWindow()
        {
            FrameworkSettingsWindow window = GetWindow<FrameworkSettingsWindow>("Framework Settings");
            window.minSize = new Vector2(400, 500);
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawResourceSettings();
            EditorGUILayout.Space(10);

            DrawLogSettings();
            EditorGUILayout.Space(10);

            DrawNetworkSettings();
            EditorGUILayout.Space(10);

            DrawHotUpdateSettings();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            GUILayout.Label("CoolGameFramework Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure framework settings here.", MessageType.Info);
        }

        private void DrawResourceSettings()
        {
            EditorGUILayout.LabelField("Resource Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Load Mode:", EditorPrefs.GetString("CGF_ResourceLoadMode", "Resources"));
            if (GUILayout.Button("Set to Resources Mode"))
            {
                EditorPrefs.SetString("CGF_ResourceLoadMode", "Resources");
                Debug.Log("Resource load mode set to: Resources");
            }
            if (GUILayout.Button("Set to AssetBundle Mode"))
            {
                EditorPrefs.SetString("CGF_ResourceLoadMode", "AssetBundle");
                Debug.Log("Resource load mode set to: AssetBundle");
            }

            EditorGUI.indentLevel--;
        }

        private void DrawLogSettings()
        {
            EditorGUILayout.LabelField("Log Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            bool enableLog = EditorGUILayout.Toggle("Enable Log", EditorPrefs.GetBool("CGF_EnableLog", true));
            EditorPrefs.SetBool("CGF_EnableLog", enableLog);

            bool enableFileLog = EditorGUILayout.Toggle("Enable File Log", EditorPrefs.GetBool("CGF_EnableFileLog", false));
            EditorPrefs.SetBool("CGF_EnableFileLog", enableFileLog);

            int logLevel = EditorGUILayout.Popup("Min Log Level", EditorPrefs.GetInt("CGF_LogLevel", 0),
                new string[] { "Debug", "Info", "Warning", "Error" });
            EditorPrefs.SetInt("CGF_LogLevel", logLevel);

            EditorGUI.indentLevel--;
        }

        private void DrawNetworkSettings()
        {
            EditorGUILayout.LabelField("Network Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            string serverUrl = EditorGUILayout.TextField("Server URL", EditorPrefs.GetString("CGF_ServerURL", ""));
            EditorPrefs.SetString("CGF_ServerURL", serverUrl);

            int serverPort = EditorGUILayout.IntField("Server Port", EditorPrefs.GetInt("CGF_ServerPort", 8080));
            EditorPrefs.SetInt("CGF_ServerPort", serverPort);

            EditorGUI.indentLevel--;
        }

        private void DrawHotUpdateSettings()
        {
            EditorGUILayout.LabelField("Hot Update Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            string versionUrl = EditorGUILayout.TextField("Version URL", EditorPrefs.GetString("CGF_VersionURL", ""));
            EditorPrefs.SetString("CGF_VersionURL", versionUrl);

            string assetUrl = EditorGUILayout.TextField("Asset URL", EditorPrefs.GetString("CGF_AssetURL", ""));
            EditorPrefs.SetString("CGF_AssetURL", assetUrl);

            bool enableHotUpdate = EditorGUILayout.Toggle("Enable Hot Update", EditorPrefs.GetBool("CGF_EnableHotUpdate", false));
            EditorPrefs.SetBool("CGF_EnableHotUpdate", enableHotUpdate);

            EditorGUI.indentLevel--;
        }
    }
}
