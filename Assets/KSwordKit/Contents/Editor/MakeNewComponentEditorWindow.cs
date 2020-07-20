using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KSwordKit.Contents.Editor
{
    public class MakeNewComponentEditorWindow : EditorWindow
    {
        static MakeNewComponentEditorWindow window;

        public static void Open()
        {

            window = GetWindow<MakeNewComponentEditorWindow>(false, KSwordKitConst.KSwordKitName + ": " + ContentsEditor.MakeNewWindowTitle);

            window.Show();
        }

        string newComponentPath = string.Empty;
        string name = "";
        string dependencies = "";
        string exampleFolderPaths = "";
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            name = EditorGUILayout.TextField("部件名称：", name);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("依赖：");
            dependencies = EditorGUILayout.TextArea(dependencies, GUILayout.Height(100));
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("示例代码位置：");
            exampleFolderPaths = EditorGUILayout.TextArea(exampleFolderPaths, GUILayout.Height(100));
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("特殊文件设置");
            if (GUILayout.Button("添加"))
            {

            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("已存在新部件配置:");
            if (GUILayout.Button("选择配置文件"))
            {
                newComponentPath = EditorUtility.OpenFilePanel("选择新部件的导入配置文件", "Assets", "json");
            }
            EditorGUILayout.EndHorizontal();
        }
    }

}