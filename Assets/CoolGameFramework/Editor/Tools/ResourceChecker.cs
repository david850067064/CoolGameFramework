using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoolGameFramework.Editor
{
    /// <summary>
    /// 资源检查工具
    /// </summary>
    public class ResourceChecker : EditorWindow
    {
        private Vector2 _scrollPosition;
        private List<string> _missingScripts = new List<string>();
        private List<string> _unusedAssets = new List<string>();
        private List<string> _duplicateAssets = new List<string>();

        [MenuItem("CoolGameFramework/Resource Checker")]
        public static void ShowWindow()
        {
            GetWindow<ResourceChecker>("Resource Checker");
        }

        private void OnGUI()
        {
            GUILayout.Label("Resource Checker", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Check Missing Scripts", GUILayout.Height(30)))
            {
                CheckMissingScripts();
            }
            if (GUILayout.Button("Check Unused Assets", GUILayout.Height(30)))
            {
                CheckUnusedAssets();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_missingScripts.Count > 0)
            {
                EditorGUILayout.LabelField("Missing Scripts:", EditorStyles.boldLabel);
                foreach (string path in _missingScripts)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(path);
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space();
            }

            if (_unusedAssets.Count > 0)
            {
                EditorGUILayout.LabelField("Unused Assets:", EditorStyles.boldLabel);
                foreach (string path in _unusedAssets)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(path);
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void CheckMissingScripts()
        {
            _missingScripts.Clear();

            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .ToArray();

            foreach (string path in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        _missingScripts.Add(path);
                        break;
                    }
                }
            }

            Debug.Log($"Missing scripts check completed. Found {_missingScripts.Count} prefabs with missing scripts.");
            if (_missingScripts.Count > 0)
            {
                EditorUtility.DisplayDialog("Check Complete",
                    $"Found {_missingScripts.Count} prefabs with missing scripts!",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Check Complete", "No missing scripts found!", "OK");
            }
        }

        private void CheckUnusedAssets()
        {
            _unusedAssets.Clear();

            string[] allAssets = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/") && !path.EndsWith(".cs") && !path.EndsWith(".meta"))
                .ToArray();

            string[] dependencies = AssetDatabase.GetDependencies(
                EditorBuildSettings.scenes.Select(s => s.path).ToArray()
            );

            foreach (string asset in allAssets)
            {
                if (!dependencies.Contains(asset) && !AssetDatabase.IsValidFolder(asset))
                {
                    _unusedAssets.Add(asset);
                }
            }

            Debug.Log($"Unused assets check completed. Found {_unusedAssets.Count} unused assets.");
            EditorUtility.DisplayDialog("Check Complete",
                $"Found {_unusedAssets.Count} potentially unused assets!",
                "OK");
        }
    }
}
