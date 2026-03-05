using UnityEditor;
using UnityEngine;
using System.IO;

namespace CoolGameFramework.Editor
{
    /// <summary>
    /// AssetBundle打包工具
    /// </summary>
    public class AssetBundleBuilder : EditorWindow
    {
        private static string _outputPath = "AssetBundles";
        private BuildTarget _buildTarget = BuildTarget.StandaloneWindows64;

        [MenuItem("CoolGameFramework/AssetBundle/Build Window")]
        public static void ShowWindow()
        {
            GetWindow<AssetBundleBuilder>("AssetBundle Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("AssetBundle Build Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _outputPath = EditorGUILayout.TextField("Output Path:", _outputPath);
            _buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target:", _buildTarget);

            EditorGUILayout.Space();

            if (GUILayout.Button("Build AssetBundles", GUILayout.Height(30)))
            {
                BuildAssetBundles();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Clear AssetBundle Names", GUILayout.Height(30)))
            {
                ClearAssetBundleNames();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Output Folder", GUILayout.Height(30)))
            {
                OpenOutputFolder();
            }
        }

        private void BuildAssetBundles()
        {
            string outputPath = Path.Combine(Application.dataPath, "..", _outputPath, _buildTarget.ToString());

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            BuildPipeline.BuildAssetBundles(
                outputPath,
                BuildAssetBundleOptions.None,
                _buildTarget
            );

            AssetDatabase.Refresh();
            Debug.Log($"AssetBundles built successfully to: {outputPath}");
            EditorUtility.DisplayDialog("Success", "AssetBundles built successfully!", "OK");
        }

        private void ClearAssetBundleNames()
        {
            string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            foreach (string name in assetBundleNames)
            {
                AssetDatabase.RemoveAssetBundleName(name, true);
            }

            AssetDatabase.Refresh();
            Debug.Log("All AssetBundle names cleared.");
            EditorUtility.DisplayDialog("Success", "All AssetBundle names cleared!", "OK");
        }

        private void OpenOutputFolder()
        {
            string outputPath = Path.Combine(Application.dataPath, "..", _outputPath);
            if (Directory.Exists(outputPath))
            {
                EditorUtility.RevealInFinder(outputPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Output folder does not exist!", "OK");
            }
        }

        [MenuItem("Assets/CoolGameFramework/Set AssetBundle Name")]
        private static void SetAssetBundleName()
        {
            Object[] selectedAssets = Selection.objects;
            if (selectedAssets.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Please select at least one asset!", "OK");
                return;
            }

            string bundleName = EditorUtility.SaveFilePanel("Set AssetBundle Name", "", "bundle", "");
            if (string.IsNullOrEmpty(bundleName))
                return;

            bundleName = Path.GetFileNameWithoutExtension(bundleName).ToLower();

            foreach (Object obj in selectedAssets)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                AssetImporter importer = AssetImporter.GetAtPath(path);
                if (importer != null)
                {
                    importer.assetBundleName = bundleName;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"AssetBundle name set to: {bundleName}");
        }
    }
}
