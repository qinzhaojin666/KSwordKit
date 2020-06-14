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
                        EditorUtility.DisplayProgressBar("生成资源包", "生成资源清单...", 0);
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
                        }

                        EditorUtility.DisplayProgressBar("生成资源包", "生成资源清单...", 0);
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
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            return outputPath;
        }
    }
}


