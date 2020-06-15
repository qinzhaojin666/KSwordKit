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
        public const string ResourcesFileName = "resourceslist.csv";
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

            try
            {
                var watch = Watch.Do(() => {
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
                            Debug.LogWarning(KSwordKitName + ": 您选中了一些资源对象，但是都不可用！请检查它们的资源标签是否设置妥当。");
                        }

                    }
                });
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("KSwordKit: 资源管理/生成资源包 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }

            EditorUtility.ClearProgressBar();
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
        /// 生成资源清单
        /// </summary>
        public static void GenResourceList()
        {
            EditorUtility.DisplayProgressBar("生成资源包", "生成资源清单...", 0);

            try
            {
                var watch = Watch.Do(() =>
                {
                    var outputPath = assetBundleOutputDirectory();
                    var resourceListfilePath = System.IO.Path.Combine(outputPath, ResourcesFileName);
                    writeResourceListFile(resourceListfilePath);
                });
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }

            EditorUtility.ClearProgressBar();
        }

        static void writeResourceListFile(string filepath)
        {

            EditorUtility.DisplayProgressBar("正在生成资源包", "生成资源清单: 准备写入数据...", 0.1f);

            System.IO.StreamWriter sw = null;
            if (!System.IO.File.Exists(filepath))
            {
                sw = System.IO.File.CreateText(filepath);
            }
            else
            {
                sw = new System.IO.StreamWriter(filepath, false);
            }
            sw.WriteLine("ResourcesPath,AssetBundleName,AssetBundleVariant,ObjectName,ObjectExtensionName");


            EditorUtility.DisplayProgressBar("正在生成资源包", "生成资源清单: 正在写入数据...", 0.2f);

            var outputdirpath = assetBundleOutputDirectory();

            var ab = AssetBundle.LoadFromFile(System.IO.Path.Combine(outputdirpath, ResourceRootDirectoryName));
            var abmainifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            foreach(var name in abmainifest.GetAllAssetBundles())
            {
                Debug.Log("资源名称："+name);
                foreach(var dep in abmainifest.GetAllDependencies(name))
                {
                    Debug.Log("依赖: " + dep);
                }
            }

            ab.Unload(true);

            //foreach (var file in new System.IO.DirectoryInfo(assetBundleOutputDirectory()).GetFiles())
            //{
            //    if (System.IO.Path.GetExtension(file.FullName) == ".manifest")
            //        continue;
            //    if (System.IO.Path.GetFileName(file.FullName) == ResourcesFileName)
            //        continue;
            //    ab = AssetBundle.LoadFromFile(file.FullName);
            //    if (ab.isStreamedSceneAssetBundle)
            //    {
            //        var allassets = ab.GetAllScenePaths();
            //        foreach (var p in allassets)
            //        {
            //            EditorUtility.DisplayProgressBar("正在生成资源包..", "添加：" + p, Random.Range(0f, 1));

            //            var ex = System.IO.Path.GetExtension(p);
            //            if (ex.StartsWith("."))
            //                ex = ex.Substring(1);
            //            sw.WriteLine(p + "," + file.Name + "," + ex);
            //        }

            //    }
            //    else
            //    {
            //        var allassets = ab.GetAllAssetNames();
            //        foreach (var o in allassets)
            //        {
            //            EditorUtility.DisplayProgressBar("正在生成资源包..", "添加：" + o, Random.Range(0f, 1));

            //            var ex = System.IO.Path.GetExtension(o);
            //            if (ex.StartsWith("."))
            //                ex = ex.Substring(1);
            //            sw.WriteLine(o + "," + file.Name + "," + ex);
            //        }
            //    }

            //    ab.Unload(true);
            //}
            
            sw.Close();
            EditorUtility.DisplayProgressBar("正在生成资源包", "资源清单已生成！", 1f);
        }
    }
}


