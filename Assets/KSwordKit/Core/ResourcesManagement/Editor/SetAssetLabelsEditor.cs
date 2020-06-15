/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: SetResourcesLabelsEditor.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-13
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KSwordKit.Core.ResourcesManagement.Editor
{
    public class SetAssetLabelsEditor 
    {
        /// <summary>
        /// 框架名称
        /// </summary>
        public const string KSwordKitName = "KSwordKit";
        /// <summary>
        /// 文件名最大长度
        /// </summary>
        public const int FileNameMaxLength = 260;
        /// <summary>
        /// 目录名最大长度
        /// </summary>
        public const int DirectoryNameMaxLength = 248;

        [MenuItem("Assets/KSwordKit/资源管理/自动设置资源标签🏷", false, -10)]
        [MenuItem("KSwordKit/资源管理/自动设置资源标签🏷", false, -10)]
        public static void SetAssetLabels()
        {
            var objects = Selection.objects;
            // 没有选中任何资源
            if(objects.Length == 0)
            {
                UnityEngine.Debug.LogWarning(KSwordKitName + ": 未选中任何资源，无法自动设置标签🏷！");
                return;
            }

            EditorUtility.DisplayProgressBar("自动设置资源标签🏷", "程序执行中...", 0);
            bool isError = false;

            var watch = Watch.Do(() =>
            {
                try
                {
                    // 选中的所有文件
                    var selectedFileList = new List<string>();
                    foreach (var o in objects)
                    {
                        var path = AssetDatabase.GetAssetPath(o);
                        EditorUtility.DisplayProgressBar("自动设置资源标签🏷..", "正在处理：" + path, Random.Range(0f, 1f));

                        if (System.IO.File.Exists(path) && System.IO.Path.GetFileName(path) != AssetBundleRuleEditor.AssetBundleGeneratesRuleFileName)
                        {
                            var fileinfo = new System.IO.FileInfo(path);
                            selectedFileList.Add(fileinfo.FullName);
                            setABNameByFile(ConvertAssetPathToAssetBundleName(fileinfo.FullName), fileinfo.FullName);
                        }
                    }

                    objects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
                    foreach (var o in objects)
                    {
                        var path = AssetDatabase.GetAssetPath(o);
                        EditorUtility.DisplayProgressBar("自动设置资源标签🏷..", "正在处理：" + path, Random.Range(0f, 1f));
                        if(System.IO.Path.GetFileName(path) == AssetBundleRuleEditor.AssetBundleGeneratesRuleFileName)
                        {
                            AssetImporter assetImporter = AssetImporter.GetAtPath(path);  //得到Asset
                            assetImporter.assetBundleName = null;    //最终设置assetBundleName
                        }
                        else if (System.IO.File.Exists(path))
                        {
                            var fileinfo = new System.IO.FileInfo(path);
                            if (selectedFileList.Contains(fileinfo.FullName))
                                continue;

                            var dir = fileinfo.Directory;
                            var abname = ConvertAssetPathToAssetBundleName(dir.FullName);
                            if (!System.IO.File.Exists(System.IO.Path.Combine(dir.FullName, AssetBundleRuleEditor.AssetBundleGeneratesRuleFileName)))
                            {
                                var dirpath = dir.FullName;
                                var datapath = new System.IO.DirectoryInfo(Application.dataPath).FullName;
                                dirpath = dirpath.Substring(datapath.Length + 1);
                                dirpath = dirpath.Replace('\\', '/');
                                var dirs = dirpath.Split('/');
                                var dirname = string.Empty;
                                foreach (var _dir in dirs)
                                {
                                    if (string.IsNullOrEmpty(dirname))
                                        dirname = _dir;
                                    else
                                        dirname = System.IO.Path.Combine(dirname, _dir);

                                    var _path = System.IO.Path.Combine(Application.dataPath, dirname);
                                    //Debug.Log("文件: " + path + ", 上层目录：" + _path);
                                    if (System.IO.File.Exists(System.IO.Path.Combine(_path, AssetBundleRuleEditor.AssetBundleGeneratesRuleFileName)))
                                    {
                                        abname = ConvertAssetPathToAssetBundleName(new System.IO.DirectoryInfo(_path).FullName);
                                        //Debug.Log("rule 文件存在：" + _path +", 标签："+ abname);
                                        break;
                                    }
                                }
                            }

                            setABNameByFile(abname, fileinfo.FullName);
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
                UnityEngine.Debug.Log(KSwordKitName + ": 资源管理/自动设置资源标签🏷 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");

        }

        /// <summary>
        /// 将资产路径转换为 AssetBundle 名称
        /// <para>第二个参数仅供函数内部递归使用</para>
        /// </summary>
        /// <param name="assetPaths">资源路径</param>
        /// <param name="_dirs">第二个参数用于函数内部递归，是一个各级目录名列表</param>
        /// <returns></returns>
        public static string ConvertAssetPathToAssetBundleName(string assetPaths, List<string> _dirs = null)
        {
            if (string.IsNullOrEmpty(assetPaths))
                return null;

            if (_dirs == null)
            {
                var path = new System.IO.DirectoryInfo(Application.dataPath).FullName;
                if (assetPaths.StartsWith(path, System.StringComparison.Ordinal))
                    assetPaths = assetPaths.Substring(path.Length + 1);

                assetPaths = assetPaths.Replace('\\', '/');

                var dirs = new List<string>(assetPaths.Split('/'));

                var filename = dirs[dirs.Count - 1];
                filename = filename.Replace('.', '_');
                dirs.RemoveAt(dirs.Count - 1);

                var dirnewpath = string.Empty;
                foreach (var dir in dirs)
                {
                    if (string.IsNullOrEmpty(dirnewpath))
                        dirnewpath = dir;
                    else
                        dirnewpath += "__" + dir;
                }

                var abname = dirnewpath + "__" + filename;

                if (abname.Length > FileNameMaxLength)
                {
                    var half = dirs.Count / 2;
                    var dir1 = new List<string>();
                    var dir2 = new List<string>();
                    for(var i = 0; i < dirs.Count; i++)
                    {
                        if (i < half)
                            dir1.Add(dirs[i]);
                        else
                            dir2.Add(dirs[i]);
                    }
                    abname = ConvertAssetPathToAssetBundleName(null, dir1)
                        + "/"
                        + ConvertAssetPathToAssetBundleName(null, dir2)
                        + "/"
                        + filename;
                }

                return abname.ToLower();
            }
            else
            {
                var dirnewpath = string.Empty;
                foreach (var dir in _dirs)
                {
                    if (string.IsNullOrEmpty(dirnewpath))
                        dirnewpath = dir;
                    else
                        dirnewpath += "__" + dir;
                }

                if(dirnewpath.Length > DirectoryNameMaxLength)
                {
                    var half = _dirs.Count / 2;
                    var dir1 = new List<string>();
                    var dir2 = new List<string>();
                    for (var i = 0; i < _dirs.Count; i++)
                    {
                        if (i < half)
                            dir1.Add(_dirs[i]);
                        else
                            dir2.Add(_dirs[i]);
                    }
                    dirnewpath = ConvertAssetPathToAssetBundleName(null, dir1)
                        + "/"
                        + ConvertAssetPathToAssetBundleName(null, dir2);
                }

                return dirnewpath;
            }
        }
        /// <summary>
        /// 设置单个AssetBundle的Name
        /// </summary>
        /// <param name="assetFilePath">项目资源文件路径</param>
        static void setABNameByFile(string abName, string assetFilePath)
        {
            //UnityEngine.Debug.Log(KSwordKitName + ": 待处理包名：" + abName + " 文件路径：" + assetFilePath);
            var ext = System.IO.Path.GetExtension(assetFilePath).ToLower();

            if (ext == ".meta" ||
                ext == ".cs" ||
                ext == ".dll" ||
                ext == ".so" ||
                ext == ".arr" ||
                ext == ".jar" ||
                ext == ".a" ||
                ext == ".mm" ||
                ext == ".java" ||
                ext == ".c" ||
                ext == ".h" ||
                ext == ".cpp" ||
                ext == ".hpp" ||
                ext == ".lua" ||
                ext == ".plist")
                return;
            var datapath = new System.IO.DirectoryInfo(Application.dataPath).FullName;

            if (assetFilePath.StartsWith(datapath, System.StringComparison.Ordinal))
                assetFilePath = System.IO.Path.Combine( "Assets", assetFilePath.Substring(datapath.Length+1));
            if (System.IO.Path.GetExtension(assetFilePath).ToLower() == ".unity")
                abName = ConvertAssetPathToAssetBundleName(assetFilePath);

            //UnityEngine.Debug.Log(KSwordKitName + ": 待处理包名：" + abName + " 文件路径：" + assetFilePath);
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetFilePath);  //得到Asset
            assetImporter.assetBundleName = abName;    //最终设置assetBundleName

            // if(ext.StartsWith("."))
            //     ext = "._" + ext.Substring(1);
            // assetImporter.assetBundleVariant = ext;

            //abNameDic[assetFilePath] = abName;
            // UnityEngine.Debug.Log("KSwordKit: 包名：" + assetImporter.assetBundleName + " 文件路径：" + assetFilePath);
        }




        [MenuItem("Assets/KSwordKit/资源管理/清理资源标签🏷")]
        [MenuItem("KSwordKit/资源管理/清理资源标签🏷")]
        public static void ClearAssetLabels()
        {
            if (!EditorUtility.DisplayDialog("是否要清理资源标签🏷 ？", "清理后无法恢复！", "确认清理", "取消操作"))
            {
                Debug.Log(KSwordKitName + ": 资源管理/清理资源标签🏷 -> 已取消！");
                return;
            }


            EditorUtility.DisplayProgressBar("清理资源标签🏷", "等待程序执行..", 0);
            try
            {
                var watch = Watch.Do(() => {
                    var objects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
                    if (objects.Length == 0)
                    {
                        AssetDatabase.RemoveUnusedAssetBundleNames();
                        string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
                        for (int i = 0; i < allAssetBundleNames.Length; i++)
                        {
                            string text = allAssetBundleNames[i];
                            AssetDatabase.RemoveAssetBundleName(text, true);
                        }

                    }
                    foreach (var o in objects)
                    {
                        var path = AssetDatabase.GetAssetPath(o);
                        EditorUtility.DisplayProgressBar("清理资源标签🏷", "正在处理：" + path, Random.Range(0f, 1));
                        if (System.IO.Directory.Exists(path))
                            continue;

                        AssetImporter assetImporter = AssetImporter.GetAtPath(path);  //得到Asset
                        if (assetImporter != null && assetImporter.assetBundleName != null && assetImporter.assetBundleName != string.Empty)
                        {
                            assetImporter.assetBundleName = null;
                        }
                    }
                    AssetDatabase.Refresh();
                });
                UnityEngine.Debug.Log("KSwordKit: 资源管理/清理资源标签🏷 -> 完成! (" + watch.ElapsedMilliseconds + "ms)");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }
            EditorUtility.ClearProgressBar();
        }
    }
}


