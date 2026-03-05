using System.Collections.Generic;
using UnityEngine;

namespace CoolGameFramework.Modules
{
    /// <summary>
    /// 本地化管理器
    /// </summary>
    public class LocalizationManager : Core.ModuleBase
    {
        public override int Priority => 12;

        private Dictionary<string, string> _localizedStrings = new Dictionary<string, string>();
        private SystemLanguage _currentLanguage = SystemLanguage.English;

        public SystemLanguage CurrentLanguage => _currentLanguage;

        public override void OnInit()
        {
            _currentLanguage = Application.systemLanguage;
            LoadLanguage(_currentLanguage);
        }

        /// <summary>
        /// 加载语言
        /// </summary>
        public void LoadLanguage(SystemLanguage language)
        {
            _currentLanguage = language;
            _localizedStrings.Clear();

            string languageCode = GetLanguageCode(language);
            TextAsset textAsset = Core.GameEntry.Resource.Load<TextAsset>($"Localization/{languageCode}");

            if (textAsset != null)
            {
                ParseLocalizationData(textAsset.text);
                Debug.Log($"Loaded language: {languageCode}");
            }
            else
            {
                Debug.LogWarning($"Language file not found: {languageCode}");
            }
        }

        /// <summary>
        /// 解析本地化数据
        /// </summary>
        private void ParseLocalizationData(string data)
        {
            string[] lines = data.Split('\n');
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    _localizedStrings[key] = value;
                }
            }
        }

        /// <summary>
        /// 获取本地化文本
        /// </summary>
        public string GetText(string key, string defaultValue = "")
        {
            if (_localizedStrings.TryGetValue(key, out string value))
            {
                return value;
            }
            return string.IsNullOrEmpty(defaultValue) ? key : defaultValue;
        }

        /// <summary>
        /// 获取格式化文本
        /// </summary>
        public string GetTextFormat(string key, params object[] args)
        {
            string text = GetText(key);
            return string.Format(text, args);
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        public void ChangeLanguage(SystemLanguage language)
        {
            LoadLanguage(language);
        }

        /// <summary>
        /// 获取语言代码
        /// </summary>
        private string GetLanguageCode(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return "zh-CN";
                case SystemLanguage.ChineseTraditional:
                    return "zh-TW";
                case SystemLanguage.English:
                    return "en-US";
                case SystemLanguage.Japanese:
                    return "ja-JP";
                case SystemLanguage.Korean:
                    return "ko-KR";
                case SystemLanguage.French:
                    return "fr-FR";
                case SystemLanguage.German:
                    return "de-DE";
                case SystemLanguage.Spanish:
                    return "es-ES";
                default:
                    return "en-US";
            }
        }

        public override void OnDestroy()
        {
            _localizedStrings.Clear();
        }
    }
}
