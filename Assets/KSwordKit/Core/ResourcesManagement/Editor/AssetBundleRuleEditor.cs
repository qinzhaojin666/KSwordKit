/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: AssetBundleRuleEditor.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-14
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KSwordKit.Core.ResourcesManagement.Editor
{
    public class AssetBundleRuleEditor
    {
        /// <summary>
        /// 框架名称
        /// </summary>
        public const string KSwordKitName = "KSwordKit";
        /// <summary>
        /// AssetBundle生成规则文件名
        /// </summary>
        public const string AssetBundleGeneratesRuleFileName = "rule";
        /// <summary>
        /// AssetBundle生成规则文件内容
        /// </summary>
        public const string AssetBundleGeneratesRuleFileContent = "使该文件所在文件夹内（包括子目录）所有资源位于同一个 AssetBundle 中。";




        [MenuItem("Assets/KSwordKit/资源管理/创建生成AssetBundle规则文件", false, -10)]
        [MenuItem("KSwordKit/资源管理/创建生成AssetBundle规则文件", false, -10)]
        public static void SetMakeAllResourcesMerge()
        {
            var objects = Selection.objects;
            // 没有选中任何资源
            if (objects.Length == 0)
            {
                UnityEngine.Debug.LogWarning(KSwordKitName + ": 未选中任何资源，无法创建生成AssetBundle规则文件！");
                return;
            }

            EditorUtility.DisplayProgressBar("创建生成AssetBundle规则文件", "程序执行中...", 0);
            bool isError = false;
            var watch = Watch.Do(() => {
                try
                {
                    var rulefilepath = AssetBundleGeneratesRuleFileName;
                    float i = 0;
                    foreach (var _object in objects)
                    {
                        i++;
                        var path = AssetDatabase.GetAssetPath(_object);

                        EditorUtility.DisplayProgressBar("创建生成AssetBundle规则文件", "正在处理：" + path, i / objects.Length);

                        if (System.IO.File.Exists(path))
                        {
                            var fileinfo = new System.IO.FileInfo(path);
                            rulefilepath = System.IO.Path.Combine(fileinfo.Directory.FullName, AssetBundleGeneratesRuleFileName);
                        }
                        else if (System.IO.Directory.Exists(path))
                        {
                            rulefilepath = System.IO.Path.Combine(path, AssetBundleGeneratesRuleFileName);
                        }

                        if (!System.IO.File.Exists(rulefilepath))
                        {
                            var file = System.IO.File.CreateText(rulefilepath);
                            file.Write(AssetBundleGeneratesRuleFileContent);
                            file.Close();
                        }
                    }

                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    isError = true;
                    Debug.LogError(KSwordKitName + ": 发生错误，" + e.Message);
                }
            });

            EditorUtility.ClearProgressBar();
            if (!isError)
                UnityEngine.Debug.Log(KSwordKitName + ": 资源管理/创建生成AssetBundle规则文件 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
        }


        [MenuItem("Assets/KSwordKit/资源管理/清理生成AssetBundle规则文件")]
        [MenuItem("KSwordKit/资源管理/清理生成AssetBundle规则文件")]
        public static void ClearMakeAllResourcesMerge()
        {
            if (!EditorUtility.DisplayDialog("是否要清理 rule 文件？", "清理后无法恢复！", "确认清理", "取消操作"))
            {
                Debug.Log(KSwordKitName + ": 资源管理/清理生成AssetBundle规则文件 -> 已取消！");
                return;
            }

            EditorUtility.DisplayProgressBar("清理生成AssetBundle规则文件", "程序执行中...", 0);
            bool isError = false;

            var watch = Watch.Do(() => {
                try
                {
                    var objects = Selection.objects;
                    // 没有选中任何资源
                    if (objects.Length == 0)
                    {
                        eachFile(Application.dataPath, (dir) => {
                            EditorUtility.DisplayProgressBar("清理生成AssetBundle规则文件", "正在处理：" + dir.Name, Random.Range(0f,1f));

                            var rule = System.IO.Path.Combine(dir.FullName, AssetBundleGeneratesRuleFileName);
                            if (System.IO.File.Exists(rule))
                                FileUtil.DeleteFileOrDirectory(rule);
                            var rulemeta = rule + ".meta";
                            if (System.IO.File.Exists(rulemeta))
                                FileUtil.DeleteFileOrDirectory(rulemeta);
                        });

                    }
                    else
                    {
                        float i = 0;
                        foreach (var _object in objects)
                        {
                            i++;
                            var path = AssetDatabase.GetAssetPath(_object);

                            EditorUtility.DisplayProgressBar("清理生成AssetBundle规则文件", "正在处理：" + path, i / objects.Length);

                            if (System.IO.File.Exists(path))
                            {
                                if (System.IO.Path.GetFileName(path) == AssetBundleGeneratesRuleFileName)
                                    FileUtil.DeleteFileOrDirectory(path);
                                var rulemeta = path + ".meta";
                                if (System.IO.File.Exists(rulemeta))
                                    FileUtil.DeleteFileOrDirectory(rulemeta);
                            }
                            else if (System.IO.Directory.Exists(path))
                            {
                                eachFile(path, (dir) => {
                                    EditorUtility.DisplayProgressBar("清理生成AssetBundle规则文件", "正在处理：" + dir.Name, Random.Range(0f, 1f));
                                    var rule = System.IO.Path.Combine(dir.FullName, AssetBundleGeneratesRuleFileName);
                                    if (System.IO.File.Exists(rule))
                                        FileUtil.DeleteFileOrDirectory(rule);
                                    var rulemeta = rule + ".meta";
                                    if (System.IO.File.Exists(rulemeta))
                                        FileUtil.DeleteFileOrDirectory(rulemeta);
                                });
                            }
                        }
                    }

                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    isError = true;
                    Debug.LogError(KSwordKitName + ": 发生错误，" + e.Message);
                }
            });

            EditorUtility.ClearProgressBar();
            if (!isError)
                UnityEngine.Debug.Log(KSwordKitName + ": 资源管理/清理生成AssetBundle规则文件 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");

        }
        private static void eachFile(string dirPath = null, System.Action<System.IO.DirectoryInfo> action = null)
        {
            if (dirPath == null)
                dirPath = Application.dataPath;

            var dir = new System.IO.DirectoryInfo(dirPath);
            foreach (var _dir in dir.GetDirectories())
            {
                if (action != null)
                    action(_dir);
                eachFile(_dir.FullName, action);
            }
        }
    }
}
