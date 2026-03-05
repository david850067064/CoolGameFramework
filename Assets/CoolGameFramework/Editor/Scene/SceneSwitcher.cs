using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace CoolGameFramework.Editor
{
    /// <summary>
    /// 场景快速切换工具
    /// </summary>
    public class SceneSwitcher : EditorWindow
    {
        private Vector2 _scrollPosition;
        private List<SceneAsset> _sceneList = new List<SceneAsset>();

        [MenuItem("CoolGameFramework/Scene Switcher")]
        public static void ShowWindow()
        {
            GetWindow<SceneSwitcher>("Scene Switcher");
        }

        private void OnEnable()
        {
            RefreshSceneList();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Scene Switcher", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                RefreshSceneList();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var scene in _sceneList)
            {
                if (scene == null) continue;

                EditorGUILayout.BeginHorizontal();

                string scenePath = AssetDatabase.GetAssetPath(scene);
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                GUILayout.Label(sceneName, GUILayout.Width(200));

                if (GUILayout.Button("Open", GUILayout.Width(60)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scenePath);
                    }
                }

                if (GUILayout.Button("Add", GUILayout.Width(60)))
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Build Settings", GUILayout.Height(30)))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }
        }

        private void RefreshSceneList()
        {
            _sceneList.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (scene != null)
                {
                    _sceneList.Add(scene);
                }
            }

            _sceneList.Sort((a, b) => a.name.CompareTo(b.name));
        }
    }
}
