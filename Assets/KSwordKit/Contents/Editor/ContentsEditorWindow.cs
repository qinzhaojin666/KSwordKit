/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ContentsWindow.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-7-9
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KSwordKit.Contents.Editor
{
    public class ImportChildWindow : EditorWindow
    {
        List<ImportConfig> list = null;
        Vector2 scorllPos;
        static ImportChildWindow window;

        public static void Open()
        {
         
            window = GetWindow<ImportChildWindow>(false, KSwordKitConst.KSwordKitName+": "+ ContentsEditor.ImportWindowTitle);
            window.list = new List<ImportConfig>();
            window.initData();
            window.Show();
        }

        private void OnGUI()
        { 
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("所有可用部件如下：");
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();

            if (list.Count > 9)
                scorllPos = EditorGUILayout.BeginScrollView(scorllPos, false, true, GUILayout.Height(200));
            else
                scorllPos = EditorGUILayout.BeginScrollView(scorllPos, false, false, GUILayout.Height(200));

            foreach (var item in list)
            {
                addItem(item, item.Name, new System.IO.DirectoryInfo(System.IO.Path.Combine(KSwordKitConst.KSwordKitContentsSourceDiretory, item.Name)).FullName);
            }
            EditorGUILayout.EndScrollView();
        }

        void initData()
        {
            var dirInfo = new System.IO.DirectoryInfo(KSwordKit.KSwordKitConst.KSwordKitContentsSourceDiretory);
            foreach (var dir in dirInfo.GetDirectories())
            {
                var filePath = System.IO.Path.Combine(dir.FullName, ContentsEditor.ImportConfigFileName);
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        var importConfig = JsonUtility.FromJson<ImportConfig>(System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8));
                        list.Add(importConfig);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(KSwordKitConst.KSwordKitName + ": 导入部件时出错, " + e.Message);
                    }
                }
            }
        }

        void addItem(ImportConfig config, string name, string path)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            var isExists = System.IO.Directory.Exists(System.IO.Path.Combine(KSwordKitConst.KSwordKitContentsDirectory, name));
            if (isExists)
            {
                if (GUILayout.Button("重新导入", GUILayout.Width(110)))
                    import(config, name, path);
            }
            else
            {
                if (GUILayout.Button("导入", GUILayout.Width(110)))
                    import(config, name, path);
            }

            EditorGUI.BeginDisabledGroup(!isExists);
            if (GUILayout.Button("删除", GUILayout.Width(110)))
                delete(config, name, path);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        void import(ImportConfig config, string name, string path)
        {            
            Debug.Log(KSwordKitConst.KSwordKitName + ": 正准备导入部件 " + name + " ...");
            if(!System.IO.Directory.Exists(path))
            {
                Debug.Log(KSwordKitConst.KSwordKitName + ": 导入部件 " + name + " 失败！源代码不存在。");
                return;
            }
            var cPath = System.IO.Path.Combine(KSwordKitConst.KSwordKitContentsDirectory, name);
            var spath = System.IO.Path.Combine(KSwordKitConst.KSwordKitContentsSourceDiretory, name);

            var error = CopyFolder(config, true, name, path, cPath);
            foreach (var item in config.FileSettingList)
            {
                if (!string.IsNullOrEmpty(item.ExamplePath))
                    error += CopyFolder(config, false, name,
                        new System.IO.DirectoryInfo(System.IO.Path.Combine(spath, item.ExamplePath)).FullName,
                        System.IO.Path.Combine(KSwordKitConst.KSwordKitExamplesDirtory, "Example" + name));

                if (string.IsNullOrEmpty(item.Path) || string.IsNullOrEmpty(item.ImportPath))
                    continue;

                if (item.IsDir)
                {
                    error += CopyFolder(config, false, name, new System.IO.DirectoryInfo(System.IO.Path.Combine(spath, item.Path)).FullName, new System.IO.DirectoryInfo(item.ImportPath).FullName);
                }
                else
                {
                    try
                    {
                        var fName = System.IO.Path.GetFileName(item.ImportPath);
                        var dirpath = item.ImportPath.Substring(0, item.ImportPath.Length - fName.Length - 1);
                        if (!System.IO.Directory.Exists(dirpath))
                        {
                            System.IO.Directory.CreateDirectory(dirpath);
                        }
                        EditorUtility.DisplayProgressBar(KSwordKitConst.KSwordKitName + ": 拷贝部件 " + name, "正在拷贝：" + fName, UnityEngine.Random.Range(0f, 1));
                        System.IO.File.Copy(new System.IO.DirectoryInfo(System.IO.Path.Combine(spath, item.Path)).FullName, System.IO.Path.Combine(dirpath, fName), true);
                    }catch(System.Exception e)
                    {
                        error += e.Message;
                    }

                    EditorUtility.ClearProgressBar();
                }
            }
            AssetDatabase.Refresh();
            if (string.IsNullOrEmpty(error))
                Debug.Log(KSwordKitConst.KSwordKitName + ": 导入部件 " + name + " 成功！");
        }

        string CopyFolder(ImportConfig config, bool useConfig, string name, string sourceDir, string destDir)
        {
            string error = "";
            try
            {
                if (!System.IO.Directory.Exists(destDir))
                {
                    System.IO.Directory.CreateDirectory(destDir);
                }

                var cinfo = new System.IO.DirectoryInfo(System.IO.Path.Combine(System.IO.Path.Combine(KSwordKitConst.KSwordKitContentsSourceDiretory, name)));
                var icfnInfo = new System.IO.FileInfo(System.IO.Path.Combine(cinfo.FullName, ContentsEditor.ImportConfigFileName));

                string[] fileList = System.IO.Directory.GetFiles(sourceDir, "*");
                foreach (string f in fileList)
                {
                    string fName = f.Substring(sourceDir.Length + 1);
                    if (useConfig)
                    {
                        var fileinfo = new System.IO.FileInfo(f);

                        if (fileinfo.FullName == icfnInfo.FullName)
                            continue;
                        var find = config.FileSettingList.Find((fs) => {
                            if (fs.IsDir || string.IsNullOrEmpty(fs.Path) || string.IsNullOrEmpty(fs.ImportPath)) return false;
                            if (new System.IO.DirectoryInfo(System.IO.Path.Combine(cinfo.FullName, fs.Path)).FullName == fileinfo.FullName) return true;
                            return new System.IO.FileInfo(fs.Path).FullName == fileinfo.FullName;
                        });

                        if (find != null)
                            continue;
                    }

                    EditorUtility.DisplayProgressBar(KSwordKitConst.KSwordKitName + ": 拷贝部件 "+ name, "正在拷贝：" + fName, UnityEngine.Random.Range(0f, 1));
                    System.IO.File.Copy(System.IO.Path.Combine(sourceDir, fName), System.IO.Path.Combine(destDir, fName), true);
                }
                foreach (var dir in System.IO.Directory.GetDirectories(sourceDir, "*"))
                {
                    if (useConfig)
                    {
                        var dirinfo = new System.IO.DirectoryInfo(dir);
                        var f = config.FileSettingList.Find((fs) => {
                            if (new System.IO.DirectoryInfo(System.IO.Path.Combine(cinfo.FullName, fs.ExamplePath)).FullName == dirinfo.FullName) return true;
                            if (!fs.IsDir || string.IsNullOrEmpty(fs.Path) || string.IsNullOrEmpty(fs.ImportPath)) return false;
                            return new System.IO.DirectoryInfo(System.IO.Path.Combine(cinfo.FullName, fs.Path)).FullName == dirinfo.FullName;
                        });
                        if (f != null)
                            continue;
                    }
                    error += CopyFolder(config, useConfig, name, dir, System.IO.Path.Combine(destDir, dir.Substring(sourceDir.Length + 1)));
                }
            }
            catch (System.Exception e)
            {
                error += e.Message;
            }

            EditorUtility.ClearProgressBar();

            return error;
        }

        void delete(ImportConfig config, string name, string path)
        {
            Debug.Log(KSwordKitConst.KSwordKitName + ": 正准备删除部件 " + name + " ...");

            var cPath = System.IO.Path.Combine(KSwordKitConst.KSwordKitContentsDirectory, name);
            var spath = System.IO.Path.Combine(KSwordKitConst.KSwordKitContentsSourceDiretory, name);
            var epath = System.IO.Path.Combine(KSwordKitConst.KSwordKitExamplesDirtory, "Example" + name);

            if (System.IO.Directory.Exists(cPath))
                FileUtil.DeleteFileOrDirectory(cPath);

            foreach (var item in config.FileSettingList)
            {
                if (!string.IsNullOrEmpty(item.ExamplePath))
                    if (System.IO.Directory.Exists(epath))
                        FileUtil.DeleteFileOrDirectory(epath);

                if (item.IsDir)
                {
                    if (System.IO.Directory.Exists(item.ImportPath))
                        FileUtil.DeleteFileOrDirectory(item.ImportPath);
                }
                else
                {
                    if (System.IO.File.Exists(item.ImportPath))
                        FileUtil.DeleteFileOrDirectory(item.ImportPath);
                }
            }
            AssetDatabase.Refresh();
            Debug.Log(KSwordKitConst.KSwordKitName + ": 已删除部件 " + name);
        }
    }
}