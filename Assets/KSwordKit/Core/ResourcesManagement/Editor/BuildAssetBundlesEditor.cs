/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: BuildAssetBundlesEditor.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-14
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace KSwordKit.Core.ResourcesManagement.Editor
{
    public class BuildAssetBundlesEditor
    {
        /// <summary>
        /// 用于AssetBundle输出位置
        /// </summary>
        public const string AssetBundles = "AssetBundles";
        /// <summary>
        /// 资源根文件夹名
        /// </summary>
        public const string ResourceRootDirectoryName = "resources";
        /// <summary>
        /// 资源清单文件名
        /// </summary>
        public const string ResourcesFileName = "resourceslist.json";
        /// <summary>
        /// 框架名称
        /// </summary>
        public const string KSwordKitName = "KSwordKit";

        /// <summary>
        /// 按当前目标平台生成AssetBundle
        /// 如果有被选中的文件或文件夹，则对被选中的对象进行打包（如果已被AssetBundleName 有值）。
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/生成资源包", false, 10)]
        [MenuItem("KSwordKit/资源管理/生成资源包", false, 10)]
        public static void AssetBundleBuildAll()
        {
            EditorUtility.DisplayProgressBar("生成资源包", "程序执行中...", 0);
            bool isError = false;
            var watch = Watch.Do(() =>
            {
                try
                {
                    var outputPath = assetBundleOutputDirectory();
                    if (Selection.objects.Length == 0)
                    {
                        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
                        GenResourceList();
                    }
                    else
                    {

                        var objects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
                        Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
                        foreach (var o in objects)
                        {
                            var path = AssetDatabase.GetAssetPath(o);
                            if (System.IO.File.Exists(path))
                            {
                                AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                                    continue;

                                if (dic.ContainsKey(assetImporter.assetBundleName))
                                {
                                    dic[assetImporter.assetBundleName].Add(path);
                                }
                                else
                                {
                                    var list = new List<string>();
                                    list.Add(path);
                                    dic[assetImporter.assetBundleName] = list;
                                }
                            }
                        }

                        if (dic.Count != 0)
                        {
                            var map = new List<AssetBundleBuild>();
                            foreach (var kv in dic)
                            {
                                var build = new AssetBundleBuild();
                                build.assetBundleName = kv.Key;
                                build.assetNames = kv.Value.ToArray();

                                map.Add(build);
                            }

                            BuildPipeline.BuildAssetBundles(outputPath, map.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

                            GenResourceList();
                        }
                        else
                        {
                            Debug.LogWarning(KSwordKitName + ": 您选中了一些不可用的资源对象！请检查它们的资源标签是否设置妥当。");
                        }
                    }

                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    isError = true;
                    UnityEngine.Debug.LogError(e.Message);
                }

            });

            EditorUtility.ClearProgressBar();
            if (!isError)
                UnityEngine.Debug.Log("KSwordKit: 资源管理/生成资源包 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
        }
        /// <summary>
        /// 创建AssetBundle输出目录
        /// </summary>
        /// <returns>返回资源包生成的目录路径</returns>
        public static string assetBundleOutputDirectory()
        {
            var outputPath = System.IO.Path.Combine(AssetBundles, EditorUserBuildSettings.activeBuildTarget.ToString());
            outputPath = System.IO.Path.Combine(outputPath, ResourceRootDirectoryName);
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            return outputPath;
        }
        /// <summary>
        /// 创建AssetBundle的StreamingAssets目录
        /// </summary>
        /// <returns>返回资源包的StreamingAssets目录路径</returns>
        public static string asssetBundleStreamingAssetsDirctory()
        {
            var dir = System.IO.Path.Combine(Application.streamingAssetsPath, AssetBundles);
            dir = System.IO.Path.Combine(dir, EditorUserBuildSettings.activeBuildTarget.ToString());
            return System.IO.Path.Combine(dir, ResourceRootDirectoryName);
        }
        /// <summary>
        /// 生成资源清单
        /// </summary>
        public static void GenResourceList()
        {
            EditorUtility.DisplayProgressBar("生成资源清单", "生成资源清单...", 0);
            bool isError = false;

            var watch = Watch.Do(() =>
            {
                try
                {
                    var outputPath = System.IO.Path.Combine(AssetBundles, EditorUserBuildSettings.activeBuildTarget.ToString());
                    if (!System.IO.Directory.Exists(outputPath))
                        System.IO.Directory.CreateDirectory(outputPath);
                    var resourceListfilePath = System.IO.Path.Combine(outputPath, ResourcesFileName);
                    writeResourceListFile(resourceListfilePath);
                }
                catch (System.Exception e)
                {
                    isError = true;
                    UnityEngine.Debug.LogError(e.Message);
                }
            });
 
            EditorUtility.ClearProgressBar();
            if (!isError)
                UnityEngine.Debug.Log("KSwordKit: 资源清单生成完成! (" + watch.Elapsed.TotalSeconds + "s)");
        }
        static void writeResourceListFile(string filepath)
        {

            EditorUtility.DisplayProgressBar("生成资源清单", "准备写入数据...", 0.1f);
            bool isError = false;
            System.IO.StreamWriter sw = null;
            if (!System.IO.File.Exists(filepath))
                sw = System.IO.File.CreateText(filepath);
            else
                sw = new System.IO.StreamWriter(filepath, false);

            try
            {
                //sw.WriteLine("ResourcesPath,AssetBundleName,AssetBundleVariant,ObjectName,ObjectExtensionName");
                EditorUtility.DisplayProgressBar("生成资源清单", "正在分析资源包...", 0.2f);
                var outputdirpath = assetBundleOutputDirectory();
                var manifest = Parse(System.IO.Path.Combine(outputdirpath, ResourceRootDirectoryName + ".manifest"), ResourceRootDirectoryName, true);
                sw.Write(JsonUtility.ToJson(manifest, true));
            }
            catch(System.Exception e)
            {
                isError = true;
                UnityEngine.Debug.LogError(e.Message);
            }
            sw.Close();
            if (!isError)
                EditorUtility.DisplayProgressBar("生成资源清单", "资源清单已生成！", 1f);
        }
        /// <summary>
        /// 解析 .manifest 的方法
        /// </summary>
        /// <param name="manifestPath">文件路径</param>
        /// <param name="assetbundleName">资源标签名</param>
        /// <param name="isMain">是否是主包</param>
        public static ResourceBundleManifest Parse(string manifestPath, string assetbundleName = null, bool isMain = false)
        {

            var lines = System.IO.File.ReadAllLines(manifestPath);

            var manifest = new ResourceBundleManifest();
            manifest.AssetBundleName = assetbundleName;
            manifest.IsMain = isMain;
            var assetbundlepath = manifestPath.Replace('\\', '/');
            manifest.AssetBundlePath = assetbundlepath.Substring(0, assetbundlepath.Length - ".manifest".Length);
            bool isNewDependencyItem = false;
            bool isResourceObject = false;

            EditorUtility.DisplayProgressBar("生成资源清单", "正在分析资源包: " + manifest.AssetBundlePath, 0.2f);

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                var index = -1;
                bool isFenhao = false;
                for(var j = 0; j < line.Length; j++)
                {
                    var c = line[j];
                    if (c == '"' && isFenhao)
                        isFenhao = false;
                    else if (c == '"')
                        isFenhao = true;

                    if(!isFenhao)
                    {
                        if(c == ':')
                        {
                            index = j;
                            break;
                        }
                    }
                }    

                if (index == -1)
                    continue;
                var kv = new string[] { 
                    line.Substring(0, index),
                    line.Substring(index+1)
                };
                var k = kv[0].TrimStart(' ');
                var v = kv[1].TrimStart(' ');
                //Debug.Log("k =" + k + "\nv =" + v + "\nassetbundleName=" + assetbundleName);
                if (k == "ManifestFileVersion")
                    manifest.ManifestFileVersion = v;
                else if (k == "CRC")
                    manifest.CRC = v;
                else if (k == "serializedVersion" && string.IsNullOrEmpty(manifest.AssetFileHashSerializedVersion))
                    manifest.AssetFileHashSerializedVersion = v;
                else if (k == "serializedVersion")
                    manifest.TypeTreeHashSerializedVersion = v;
                else if (k == "Hash" && string.IsNullOrEmpty(manifest.AssetFileHash))
                    manifest.AssetFileHash = v;
                else if (k == "Hash")
                    manifest.TypeTreeHash = v;
                else if (k == "HashAppended")
                    manifest.HashAppended = v;
                else if (k == "AssetBundleManifest")
                    manifest.IsMain = true;

                if(manifest.IsMain)
                {
                    if(k.StartsWith("Info", System.StringComparison.Ordinal))
                        isNewDependencyItem = false;
                    else if (k == "Name")
                    {
                        var name = v.Trim('"');
                        name = Regex.Unescape(name);
                        var r = Parse(System.IO.Path.Combine(assetBundleOutputDirectory(), name + ".manifest"), name);
                        if (manifest.AssetBundleInfos == null)
                            manifest.AssetBundleInfos = new List<ResourceBundleManifest>();
                        manifest.AssetBundleInfos.Add(r);
                    }
                    else if (k == "Dependencies" && v != "{}")
                    {
                        isNewDependencyItem = true;
                        continue;
                    }
                    
                    if(isNewDependencyItem && k.StartsWith("Dependency_", StringComparison.Ordinal))
                    {
                        if (manifest.Dependencies == null)
                            manifest.Dependencies = new List<ResourceBundleManifest>();
                        var name = v.Trim('"');
                        name = Regex.Unescape(name);
                        var r = Parse(System.IO.Path.Combine(assetBundleOutputDirectory(), name + ".manifest"), name);
                        var lastman = manifest.AssetBundleInfos[manifest.AssetBundleInfos.Count - 1];
                        if (lastman.Dependencies == null)
                            lastman.Dependencies = new List<ResourceBundleManifest>();
                        lastman.Dependencies.Add(r);
                    }
                }
                else if(i > 3 && !manifest.IsMain)
                {
                    if (k == "Assets")
                    {
                        isResourceObject = true;
                        continue;
                    }
                    else if (k == "Dependencies" && isResourceObject)
                        isResourceObject = false;

                    if (isResourceObject)
                    {
                        var items = k.Split(' ');
                        var resourcePath = items[1].Trim('"');
                        var ritem = new ResourceObject();
                        ritem.ResourcePath = resourcePath;
                        ritem.FileExtensionName = System.IO.Path.GetExtension(resourcePath).ToLower();
                        ritem.IsScene = ritem.FileExtensionName == ".unity";
                        ritem.ObjectName = System.IO.Path.GetFileNameWithoutExtension(resourcePath);
                        ritem.AssetBundleName = assetbundleName;
                        if (manifest.ResourceObjects == null)
                            manifest.ResourceObjects = new List<ResourceObject>();
                        manifest.ResourceObjects.Add(ritem);
                    }
                }
            }

            return manifest;
        }
        /// <summary>
        /// 拷贝资源包到StreamingAssets中
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/拷贝资源包到 StreamingAssets", false, 100)]
        [MenuItem("KSwordKit/资源管理/拷贝资源包到 StreamingAssets", false, 100)]
        public static void CopyResourcesToStreamingAssets()
        {
            EditorUtility.DisplayProgressBar("拷贝资源包到 StreamingAssets", "程序执行中...", 0);
            bool isError = false;
            var watch = Watch.Do(() => {
                try
                {
                    var outputPath = System.IO.Path.Combine(Application.streamingAssetsPath, AssetBundles);
                    outputPath = System.IO.Path.Combine(outputPath, EditorUserBuildSettings.activeBuildTarget.ToString());
                    if (System.IO.Directory.Exists(outputPath))
                        FileUtil.DeleteFileOrDirectory(outputPath);
                    var metapath = outputPath + ".meta";
                    if (System.IO.File.Exists(metapath))
                        FileUtil.DeleteFileOrDirectory(metapath);

                    var sPath = System.IO.Path.Combine(AssetBundles, EditorUserBuildSettings.activeBuildTarget.ToString());
                    CopyFolder(sPath, outputPath);
                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    isError = true;
                    UnityEngine.Debug.LogError(e.Message);
                }
            });

            EditorUtility.ClearProgressBar();
            if(!isError)
                UnityEngine.Debug.Log("KSwordKit: 资源管理/拷贝资源包到 StreamingAssets -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
        }
        /// <summary>
        /// 将源目录中的所有内容拷贝到目标目录中
        /// <para>如果目标目录不存在，就创建它。</para>
        /// </summary>
        /// <param name="sourceDir">源目录</param>
        /// <param name="destDir">目标目录</param>
        private static void CopyFolder(string sourceDir, string destDir)
        {
            if (!System.IO.Directory.Exists(destDir))
            {
                System.IO.Directory.CreateDirectory(destDir);
            }

            try
            {
                string[] fileList = System.IO.Directory.GetFiles(sourceDir, "*");
                foreach (string f in fileList)
                {
                    // Remove path from the file name.
                    string fName = f.Substring(sourceDir.Length + 1);
                    EditorUtility.DisplayProgressBar("拷贝资源包到 StreamingAssets", "正在拷贝：" + fName, UnityEngine.Random.Range(0f, 1));
                    System.IO.File.Copy(System.IO.Path.Combine(sourceDir, fName), System.IO.Path.Combine(destDir, fName), true);
                }
                foreach(var dir in System.IO.Directory.GetDirectories(sourceDir,"*"))
                {
                    CopyFolder(dir, System.IO.Path.Combine(destDir, dir.Substring(sourceDir.Length + 1)));
                    //Debug.Log(dir + ", sourceDir="+sourceDir);
                }
            }
            catch (System.IO.DirectoryNotFoundException dirNotFound)
            {
                throw new System.IO.DirectoryNotFoundException(dirNotFound.Message);
            }
        }

        /// <summary>
        /// 清理输出目录中的资源包（当前编译平台）
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/清理资源包", false, 1002)]
        [MenuItem("KSwordKit/资源管理/清理资源包", false, 1002)]
        public static void AssetBundleCleanUpAssetBundles()
        {
            if (!EditorUtility.DisplayDialog("是否要清理资源包？", "清理后无法恢复！", "确认清理", "取消操作"))
            {
                Debug.Log(KSwordKitName + ": 资源管理/清理资源包 -> 已取消！");
                return;
            }

            EditorUtility.DisplayProgressBar("清理资源包", "程序执行中...", 0);
            bool isError = false;

            var watch = Watch.Do(() => {
                try
                {
                    EditorUtility.DisplayProgressBar("清理资源包", "正在清理...", 0.5f);
                    var outputPath = System.IO.Path.Combine(AssetBundles, EditorUserBuildSettings.activeBuildTarget.ToString());
                    if (System.IO.Directory.Exists(outputPath))
                        FileUtil.DeleteFileOrDirectory(outputPath);
                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    isError = true;

                    UnityEngine.Debug.LogError(e.Message);
                }
            });

            EditorUtility.ClearProgressBar();

            if (!isError)
                UnityEngine.Debug.Log("KSwordKit: 资源管理/清理资源包 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
        }
        /// <summary>
        /// 清理StreamingAssets文件夹内的资源包（当前编译平台）
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/清理 StreamingAssets", false, 1003)]
        [MenuItem("KSwordKit/资源管理/清理 StreamingAssets", false, 1003)]
        public static void AssetBundleCleanUpStreamingAssets()
        {
            if (!EditorUtility.DisplayDialog("是否要清理 StreamingAssets ？", "清理后无法恢复！", "确认清理", "取消操作"))
            {
                Debug.Log(KSwordKitName + ": 资源管理/清理 StreamingAssets -> 已取消！");
                return;
            }

            EditorUtility.DisplayProgressBar("清理 StreamingAssets", "程序执行中...", 0);
            bool isError = false;

            var watch = Watch.Do(() => {

                try
                {
                    EditorUtility.DisplayProgressBar("清理 StreamingAssets", "正在清理...", 0.5f);
                    var outputPath = System.IO.Path.Combine(Application.streamingAssetsPath, AssetBundles);
                    outputPath = System.IO.Path.Combine(outputPath, EditorUserBuildSettings.activeBuildTarget.ToString());
                    if (System.IO.Directory.Exists(outputPath))
                        FileUtil.DeleteFileOrDirectory(outputPath);
                    var metapath = outputPath + ".meta";
                    if (System.IO.File.Exists(metapath))
                        FileUtil.DeleteFileOrDirectory(metapath);
                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    isError = true;
                    UnityEngine.Debug.LogError(e.Message);
                }
            });

            EditorUtility.ClearProgressBar();

            if (!isError)
                UnityEngine.Debug.Log("KSwordKit: 资源管理/清理 StreamingAssets -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");

        }
    }
}


