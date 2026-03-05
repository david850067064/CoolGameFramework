using UnityEditor;
using UnityEngine;

namespace CoolGameFramework.Editor
{
    /// <summary>
    /// UI绑定器编辑器
    /// </summary>
    [CustomEditor(typeof(Modules.UIBinder))]
    public class UIBinderEditor : UnityEditor.Editor
    {
        private SerializedProperty _bindListProperty;

        private void OnEnable()
        {
            _bindListProperty = serializedObject.FindProperty("BindList");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UI Component Binder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_bindListProperty, true);

            EditorGUILayout.Space();

            if (GUILayout.Button("Auto Bind Children", GUILayout.Height(30)))
            {
                AutoBindChildren();
            }

            if (GUILayout.Button("Clear All Bindings", GUILayout.Height(30)))
            {
                ClearBindings();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AutoBindChildren()
        {
            Modules.UIBinder binder = target as Modules.UIBinder;
            if (binder == null) return;

            binder.BindList.Clear();

            Transform[] children = binder.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child == binder.transform) continue;

                // 只绑定名称以特定前缀开头的对象
                string name = child.name;
                if (name.StartsWith("Btn_") || name.StartsWith("Txt_") ||
                    name.StartsWith("Img_") || name.StartsWith("Go_") ||
                    name.StartsWith("Panel_"))
                {
                    binder.BindList.Add(new Modules.UIBinder.BindData
                    {
                        Name = name,
                        GameObject = child.gameObject
                    });
                }
            }

            EditorUtility.SetDirty(binder);
            Debug.Log($"Auto bind completed! Found {binder.BindList.Count} objects.");
        }

        private void ClearBindings()
        {
            Modules.UIBinder binder = target as Modules.UIBinder;
            if (binder == null) return;

            binder.BindList.Clear();
            EditorUtility.SetDirty(binder);
            Debug.Log("All bindings cleared.");
        }
    }

    /// <summary>
    /// UI绑定器菜单
    /// </summary>
    public static class UIBinderMenu
    {
        [MenuItem("GameObject/CoolGameFramework/Add UI Binder", false, 0)]
        private static void AddUIBinder()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a GameObject first!", "OK");
                return;
            }

            if (selected.GetComponent<Modules.UIBinder>() != null)
            {
                EditorUtility.DisplayDialog("Warning", "UIBinder already exists on this GameObject!", "OK");
                return;
            }

            selected.AddComponent<Modules.UIBinder>();
            Debug.Log($"UIBinder added to {selected.name}");
        }
    }
}
