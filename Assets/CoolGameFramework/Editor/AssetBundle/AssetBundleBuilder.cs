using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoolGameFramework.Editor
{
    /// <summary>
    /// AssetBundle 打包配置
    /// </summary>
    [Serializable]
    public class AssetBundleCollectorGroup
    {
        public string GroupName = "Default";
        public bool Enable = true;
        public List<AssetBundleCollector> Collectors = new List<AssetBundleCollector>();
    }

    /// <summary>
    /// AssetBundle 收集器
    /// </summary>
    [Serializable]
    public class AssetBundleCollector
    {
        public string CollectorName = "New Collector";
        public DefaultAsset CollectPath;
        public PackRule PackRule = PackRule.PackByFolder;
        public FilterRule FilterRule = FilterRule.None;
        public string CustomFilter = "";
        public bool Enable = true;
    }

    /// <summary>
    /// 打包规则
    /// </summary>
    public enum PackRule
    {
        /// <summary>
        /// 整个文件夹打成一个包
        /// </summary>
        PackByFolder,

        /// <summary>
        /// 每个子文件夹打成一个包
        /// </summary>
        PackBySubFolder,

        /// <summary>
        /// 每个文件单独打包
        /// </summary>
        PackByFile,

        /// <summary>
        /// 所有文件打成一个包
        /// </summary>
        PackByAll,

        /// <summary>
        /// 按照文件类型打包
        /// </summary>
        PackByFileType
    }

    /// <summary>
    /// 过滤规则
    /// </summary>
    public enum FilterRule
    {
        /// <summary>
        /// 不过滤
        /// </summary>
        None,

        /// <summary>
        /// 只收集场景
        /// </summary>
        Scene,

        /// <summary>
        /// 只收集预制体
        /// </summary>
        Prefab,

        /// <summary>
        /// 只收集图片
        /// </summary>
        Sprite,

        /// <summary>
        /// 只收集材质
        /// </summary>
        Material,

        /// <summary>
        /// 只收集音频
        /// </summary>
        Audio,

        /// <summary>
        /// 只收集视频
        /// </summary>
        Video,

        /// <summary>
        /// 只收集文本
        /// </summary>
        TextAsset,

        /// <summary>
        /// 自定义过滤（通过扩展名）
        /// </summary>
        Custom
    }

    /// <summary>
    /// AssetBundle 打包配置
    /// </summary>
    [CreateAssetMenu(fileName = "AssetBundleConfig", menuName = "CoolGameFramework/AssetBundle Config")]
    public class AssetBundleConfig : ScriptableObject
    {
        public string OutputPath = "AssetBundles";
        public BuildTarget BuildTarget = BuildTarget.StandaloneWindows64;
        public BuildAssetBundleOptions BuildOptions = BuildAssetBundleOptions.ChunkBasedCompression;
        public List<AssetBundleCollectorGroup> CollectorGroups = new List<AssetBundleCollectorGroup>();

        public void AddDefaultGroup()
        {
            var group = new AssetBundleCollectorGroup
            {
                GroupName = "Group_" + (CollectorGroups.Count + 1),
                Enable = true,
                Collectors = new List<AssetBundleCollector>()
            };
            CollectorGroups.Add(group);
        }
    }

    /// <summary>
    /// AssetBundle 打包工具窗口
    /// </summary>
    public class AssetBundleBuilder : EditorWindow
    {
        private AssetBundleConfig _config;
        private Vector2 _scrollPosition;
        private int _selectedGroupIndex = -1;

        [MenuItem("CoolGameFramework/AssetBundle/Build Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetBundleBuilder>("AssetBundle Builder");
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            LoadOrCreateConfig();
        }

        private void LoadOrCreateConfig()
        {
            string configPath = "Assets/CoolGameFramework/Editor/AssetBundle/AssetBundleConfig.asset";
            _config = AssetDatabase.LoadAssetAtPath<AssetBundleConfig>(configPath);

            if (_config == null)
            {
                _config = CreateInstance<AssetBundleConfig>();
                string directory = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                AssetDatabase.CreateAsset(_config, configPath);
                AssetDatabase.SaveAssets();
            }
        }

        private void OnGUI()
        {
            if (_config == null)
            {
                EditorGUILayout.HelpBox("Config not found! Please create a config file.", MessageType.Error);
                return;
            }

            DrawToolbar();
            EditorGUILayout.Space(10);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawBuildSettings();
            EditorGUILayout.Space(10);
            DrawCollectorGroups();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            DrawBuildButtons();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_config);
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("AssetBundle 打包工具", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("保存配置", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                AssetDatabase.SaveAssets();
                ShowNotification(new GUIContent("配置已保存"));
            }

            if (GUILayout.Button("帮助", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Application.OpenURL("https://david850067064.github.io/CoolGameFramework/#editor");
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawBuildSettings()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("构建设置", EditorStyles.boldLabel);

            _config.OutputPath = EditorGUILayout.TextField("输出路径", _config.OutputPath);
            _config.BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup("目标平台", _config.BuildTarget);
            _config.BuildOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField("构建选项", _config.BuildOptions);

            EditorGUILayout.EndVertical();
        }

        private void DrawCollectorGroups()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("收集器组", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                _config.AddDefaultGroup();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            for (int i = 0; i < _config.CollectorGroups.Count; i++)
            {
                DrawCollectorGroup(_config.CollectorGroups[i], i);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCollectorGroup(AssetBundleCollectorGroup group, int index)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            group.Enable = EditorGUILayout.Toggle(group.Enable, GUILayout.Width(20));

            bool foldout = _selectedGroupIndex == index;
            if (GUILayout.Button(foldout ? "▼" : "▶", GUILayout.Width(30)))
            {
                _selectedGroupIndex = foldout ? -1 : index;
            }

            group.GroupName = EditorGUILayout.TextField(group.GroupName);

            if (GUILayout.Button("添加收集器", GUILayout.Width(100)))
            {
                group.Collectors.Add(new AssetBundleCollector());
            }

            if (GUILayout.Button("删除组", GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要删除这个收集器组吗？", "确定", "取消"))
                {
                    _config.CollectorGroups.RemoveAt(index);
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (_selectedGroupIndex == index)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < group.Collectors.Count; i++)
                {
                    DrawCollector(group.Collectors[i], i, group);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCollector(AssetBundleCollector collector, int index, AssetBundleCollectorGroup group)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            collector.Enable = EditorGUILayout.Toggle(collector.Enable, GUILayout.Width(20));
            collector.CollectorName = EditorGUILayout.TextField("名称", collector.CollectorName);

            if (GUILayout.Button("删除", GUILayout.Width(60)))
            {
                group.Collectors.RemoveAt(index);
                return;
            }

            EditorGUILayout.EndHorizontal();

            collector.CollectPath = (DefaultAsset)EditorGUILayout.ObjectField("收集路径", collector.CollectPath, typeof(DefaultAsset), false);
            collector.PackRule = (PackRule)EditorGUILayout.EnumPopup("打包规则", collector.PackRule);
            collector.FilterRule = (FilterRule)EditorGUILayout.EnumPopup("过滤规则", collector.FilterRule);

            if (collector.FilterRule == FilterRule.Custom)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("自定义过滤", GUILayout.Width(100));
                collector.CustomFilter = EditorGUILayout.TextField(collector.CustomFilter);
                EditorGUILayout.LabelField("(例如: .png,.jpg)", GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();
            }

            // 显示规则说明
            EditorGUILayout.HelpBox(GetPackRuleDescription(collector.PackRule), MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        private string GetPackRuleDescription(PackRule rule)
        {
            switch (rule)
            {
                case PackRule.PackByFolder:
                    return "整个文件夹打成一个包";
                case PackRule.PackBySubFolder:
                    return "每个子文件夹打成一个包";
                case PackRule.PackByFile:
                    return "每个文件单独打包";
                case PackRule.PackByAll:
                    return "所有文件打成一个包";
                case PackRule.PackByFileType:
                    return "按照文件类型打包（.prefab, .png等）";
                default:
                    return "";
            }
        }

        private void DrawBuildButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("构建 AssetBundle", GUILayout.Height(40)))
            {
                BuildAssetBundles();
            }

            if (GUILayout.Button("清理构建", GUILayout.Height(40), GUILayout.Width(150)))
            {
                ClearBuild();
            }

            if (GUILayout.Button("打开输出目录", GUILayout.Height(40), GUILayout.Width(150)))
            {
                OpenOutputFolder();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void BuildAssetBundles()
        {
            if (_config.CollectorGroups.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "没有配置收集器组！", "确定");
                return;
            }

            EditorUtility.DisplayProgressBar("构建 AssetBundle", "准备中...", 0);

            try
            {
                // 清理旧的 AssetBundle 名称
                ClearAllAssetBundleNames();

                // 收集并设置 AssetBundle 名称
                var assetBundleBuilds = CollectAssetBundles();

                if (assetBundleBuilds.Count == 0)
                {
                    EditorUtility.DisplayDialog("警告", "没有收集到任何资源！", "确定");
                    return;
                }

                // 创建输出目录
                string outputPath = Path.Combine(_config.OutputPath, _config.BuildTarget.ToString());
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                // 构建 AssetBundle
                EditorUtility.DisplayProgressBar("构建 AssetBundle", "正在构建...", 0.5f);

                var manifest = BuildPipeline.BuildAssetBundles(
                    outputPath,
                    assetBundleBuilds.ToArray(),
                    _config.BuildOptions,
                    _config.BuildTarget
                );

                if (manifest != null)
                {
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("成功", $"AssetBundle 构建完成！\n输出路径: {outputPath}\n包数量: {assetBundleBuilds.Count}", "确定");
                    Debug.Log($"[AssetBundle] 构建成功，共 {assetBundleBuilds.Count} 个包");
                }
                else
                {
                    EditorUtility.DisplayDialog("失败", "AssetBundle 构建失败！", "确定");
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"构建过程中发生错误：\n{e.Message}", "确定");
                Debug.LogError($"[AssetBundle] 构建失败: {e}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private List<AssetBundleBuild> CollectAssetBundles()
        {
            var builds = new Dictionary<string, List<string>>();

            foreach (var group in _config.CollectorGroups)
            {
                if (!group.Enable) continue;

                foreach (var collector in group.Collectors)
                {
                    if (!collector.Enable || collector.CollectPath == null) continue;

                    string collectPath = AssetDatabase.GetAssetPath(collector.CollectPath);
                    CollectAssets(collector, collectPath, builds);
                }
            }

            // 转换为 AssetBundleBuild 数组
            var result = new List<AssetBundleBuild>();
            foreach (var kvp in builds)
            {
                result.Add(new AssetBundleBuild
                {
                    assetBundleName = kvp.Key,
                    assetNames = kvp.Value.ToArray()
                });
            }

            return result;
        }

        private void CollectAssets(AssetBundleCollector collector, string path, Dictionary<string, List<string>> builds)
        {
            if (!Directory.Exists(path)) return;

            string[] allFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            foreach (string file in allFiles)
            {
                if (file.EndsWith(".meta")) continue;

                string assetPath = file.Replace("\\", "/");

                // 应用过滤规则
                if (!PassFilter(assetPath, collector.FilterRule, collector.CustomFilter))
                    continue;

                // 应用打包规则
                string bundleName = GetBundleName(assetPath, path, collector.PackRule);

                if (!builds.ContainsKey(bundleName))
                {
                    builds[bundleName] = new List<string>();
                }

                builds[bundleName].Add(assetPath);
            }
        }

        private bool PassFilter(string assetPath, FilterRule filterRule, string customFilter)
        {
            switch (filterRule)
            {
                case FilterRule.None:
                    return true;

                case FilterRule.Scene:
                    return assetPath.EndsWith(".unity");

                case FilterRule.Prefab:
                    return assetPath.EndsWith(".prefab");

                case FilterRule.Sprite:
                    return assetPath.EndsWith(".png") || assetPath.EndsWith(".jpg") ||
                           assetPath.EndsWith(".jpeg") || assetPath.EndsWith(".tga");

                case FilterRule.Material:
                    return assetPath.EndsWith(".mat");

                case FilterRule.Audio:
                    return assetPath.EndsWith(".mp3") || assetPath.EndsWith(".wav") ||
                           assetPath.EndsWith(".ogg");

                case FilterRule.Video:
                    return assetPath.EndsWith(".mp4") || assetPath.EndsWith(".mov");

                case FilterRule.TextAsset:
                    return assetPath.EndsWith(".txt") || assetPath.EndsWith(".json") ||
                           assetPath.EndsWith(".xml") || assetPath.EndsWith(".bytes");

                case FilterRule.Custom:
                    if (string.IsNullOrEmpty(customFilter)) return true;
                    string[] extensions = customFilter.Split(',');
                    foreach (string ext in extensions)
                    {
                        if (assetPath.EndsWith(ext.Trim()))
                            return true;
                    }
                    return false;

                default:
                    return true;
            }
        }

        private string GetBundleName(string assetPath, string rootPath, PackRule packRule)
        {
            string relativePath = assetPath.Replace(rootPath + "/", "");

            switch (packRule)
            {
                case PackRule.PackByFolder:
                    return Path.GetFileName(rootPath).ToLower();

                case PackRule.PackBySubFolder:
                    string subFolder = Path.GetDirectoryName(relativePath);
                    if (string.IsNullOrEmpty(subFolder))
                        return Path.GetFileName(rootPath).ToLower();
                    return (Path.GetFileName(rootPath) + "_" + subFolder.Replace("/", "_")).ToLower();

                case PackRule.PackByFile:
                    return Path.GetFileNameWithoutExtension(assetPath).ToLower();

                case PackRule.PackByAll:
                    return "all";

                case PackRule.PackByFileType:
                    string extension = Path.GetExtension(assetPath).Replace(".", "");
                    return extension.ToLower();

                default:
                    return Path.GetFileNameWithoutExtension(assetPath).ToLower();
            }
        }

        private void ClearAllAssetBundleNames()
        {
            string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            foreach (string name in assetBundleNames)
            {
                AssetDatabase.RemoveAssetBundleName(name, true);
            }
        }

        private void ClearBuild()
        {
            string outputPath = Path.Combine(_config.OutputPath, _config.BuildTarget.ToString());
            if (Directory.Exists(outputPath))
            {
                if (EditorUtility.DisplayDialog("确认", $"确定要删除构建目录吗？\n{outputPath}", "确定", "取消"))
                {
                    Directory.Delete(outputPath, true);
                    AssetDatabase.Refresh();
                    ShowNotification(new GUIContent("构建已清理"));
                }
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "构建目录不存在", "确定");
            }
        }

        private void OpenOutputFolder()
        {
            string outputPath = Path.Combine(_config.OutputPath, _config.BuildTarget.ToString());
            if (Directory.Exists(outputPath))
            {
                EditorUtility.RevealInFinder(outputPath);
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "输出目录不存在，请先构建", "确定");
            }
        }
    }
}
