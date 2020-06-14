/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ResourcesEditor.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-7
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace KSwordKit.Core.ResourcesManagement.Editor
{
    public class ResourcesEditor
    {
        /// <summary>
        /// 用于AssetBundle输出位置
        /// </summary>
        public const string AssetBundles = "AssetBundles";
        public const string ResourcesFileName = "resourceslist.csv";

        /// <summary>
        /// 按当前目标平台生成AssetBundle
        /// 如果有被选中的文件或文件夹，则对被选中的对象进行打包（如果已被AssetBundleName 有值）。
        /// </summary>
        //[MenuItem("Assets/KSwordKit/资源管理/生成资源包", false, 10)]
        //[MenuItem("KSwordKit/资源管理/生成资源包", false, 10)]
        //public static void AssetBundleBuildAll()
        //{
        //    EditorUtility.DisplayProgressBar("正在生成资源包..", "程序执行中...", 0);

        //    try{

        //        var watch = watchIt(()=>{
        //            var outputPath = assetBundleOutputDirectory();
        //            if(Selection.objects.Length == 0){
                        
        //                BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        //                EditorUtility.DisplayProgressBar("正在生成资源包..", "准备写入资源清单...", 0);

        //            }else{

        //                var objects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        //                Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
        //                foreach(var o in objects)
        //                {
        //                    var path = AssetDatabase.GetAssetPath(o);
        //                    if (System.IO.File.Exists(path))
        //                    {
        //                        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        //                        if (string.IsNullOrEmpty(assetImporter.assetBundleName))
        //                            continue;
        //                        if (dic.ContainsKey(assetImporter.assetBundleName))
        //                        {
        //                            dic[assetImporter.assetBundleName].Add(path);
        //                        }
        //                        else
        //                        {
        //                            var list = new List<string>();
        //                            list.Add(path);
        //                            dic[assetImporter.assetBundleName] = list;
        //                        }
        //                    }
        //                }

        //                if(dic.Count != 0)
        //                {
        //                    var map = new List<AssetBundleBuild>();

        //                    foreach(var kv in dic)
        //                    {
        //                        var build = new AssetBundleBuild();
        //                        build.assetBundleName = kv.Key;
        //                        build.assetNames = kv.Value.ToArray();
        //                        map.Add(build);
        //                    }

        //                    BuildPipeline.BuildAssetBundles(outputPath, map.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        //                }
                        
        //                EditorUtility.DisplayProgressBar("正在生成资源包..", "准备写入资源清单...", 0);
        //            }
        //        });
        //        AssetDatabase.Refresh();
        //        UnityEngine.Debug.Log("KSwordKit: 资源管理/生成资源包 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
        //    }
        //    catch(System.Exception e)
        //    {
        //        UnityEngine.Debug.LogError(e.Message);
        //    }

        //    EditorUtility.ClearProgressBar();
        //}

        /// <summary>
        /// 生成资源清单
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/生成资源清单", false, 10)]
        [MenuItem("KSwordKit/资源管理/生成资源清单", false, 10)]
        public static void GenResourceList()
        {
            EditorUtility.DisplayProgressBar("正在生成资源清单..", "程序执行中...", 0);

            try{

                var watch = watchIt(()=>{
                    var outputPath = assetBundleOutputDirectory();
                    var resourceListfilePath = System.IO.Path.Combine(outputPath, ResourcesFileName);
                    writeResourceListFile(resourceListfilePath);
                });
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("KSwordKit: 资源管理/生成资源清单 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
            }
            catch(System.Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }

            EditorUtility.ClearProgressBar();
        }
        /// <summary>
        /// 按当前目标平台生成资源包和资源清单
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/生成资源包和资源清单", false, 10)]
        [MenuItem("KSwordKit/资源管理/生成资源包和资源清单", false, 10)]
        public static void BuildAllAndGenResourceList()
        {
            EditorUtility.DisplayProgressBar("正在生成资源包和资源清单..", "程序执行中...", 0);

            try{
                var watch = watchIt(()=>{
                    //AssetBundleBuildAll();
                    GenResourceList();
                });
                UnityEngine.Debug.Log("KSwordKit: 资源管理/生成资源包和资源清单 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
            }
            catch(System.Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }

            EditorUtility.ClearProgressBar();
        }


        /// <summary>
        /// 拷贝资源包到StreamingAssets中
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/拷贝到 StreamingAssets", false, 70)]
        [MenuItem("KSwordKit/资源管理/拷贝到 StreamingAssets", false, 70)]
        static void CopyResourcesToStreamingAssets()
        {
            EditorUtility.DisplayProgressBar("正在拷贝到 StreamingAssets..", "程序执行中...", 0);
            try{

                var watch = watchIt(()=>{

                    CopyFolder(assetBundleOutputDirectory(), asssetBundleStreamingAssetsDirctory());
                    AssetDatabase.Refresh();
                });
                UnityEngine.Debug.Log("KSwordKit: 资源管理/拷贝到 StreamingAssets -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
            }
            catch(System.Exception e){
                UnityEngine.Debug.LogError(e.Message);
            }
            EditorUtility.ClearProgressBar();

        }

        /// <summary>
        /// 清理清理资源包
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/清理资源包")]
        [MenuItem("KSwordKit/资源管理/清理资源包")]
        public static void AssetBundleCleanUpAssetBundles()
        {
            EditorUtility.DisplayProgressBar("正在清理资源包..", "程序执行中...", 0);
            try{
                var watch = watchIt(()=>{

                    EditorUtility.DisplayProgressBar("正在清理资源包..", "正在清理...", 0.5f);
                    var outputPath = assetBundleOutputDirectory();
                    if (System.IO.Directory.Exists(outputPath))
                        FileUtil.DeleteFileOrDirectory(outputPath);
                });
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("KSwordKit: 资源管理/清理资源包 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");

            }
            catch(System.Exception e){
                UnityEngine.Debug.LogError(e.Message);
            }
            EditorUtility.ClearProgressBar();
        }
        
        /// <summary>
        /// 清理StreamingAssets文件夹
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/清理StreamingAssets中正使用的资源包")]
        [MenuItem("KSwordKit/资源管理/清理StreamingAssets中正使用的资源包")]
        public static void AssetBundleCleanUpStreamingAssets()
        {
            EditorUtility.DisplayProgressBar("正在清理StreamingAssets中正使用的资源包..", "程序执行中...", 0);
            try{
                var watch = watchIt(()=>{
                    var outputPath = assetBundleOutputDirectory();
                    EditorUtility.DisplayProgressBar("正在清理StreamingAssets中正使用的资源包..", "正在清理...", 0.5f);
                    outputPath = asssetBundleStreamingAssetsDirctory();
                    if (System.IO.Directory.Exists(outputPath))
                        FileUtil.DeleteFileOrDirectory(outputPath);
                    var metapath = outputPath + ".meta";
                    if(System.IO.File.Exists(metapath))
                        FileUtil.DeleteFileOrDirectory(metapath);
                });
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("KSwordKit: 资源管理/清理StreamingAssets中正使用的资源包 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");

            }
            catch(System.Exception e){
                UnityEngine.Debug.LogError(e.Message);
            }
            EditorUtility.ClearProgressBar();
        }


        /// <summary>
        /// 清理资源包和treamingAssets文件夹
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/清理资源包和StreamingAssets文件夹")]
        [MenuItem("KSwordKit/资源管理/清理资源包和StreamingAssets文件夹")]
        public static void AssetBundleCleanUpAssetBundlesAndStreamingAssets()
        {
            EditorUtility.DisplayProgressBar("正在清理资源包和treamingAssets文件夹..", "程序执行中...", 0);
            try{
                var watch = watchIt(()=>{
                    AssetBundleCleanUpAssetBundles();
                    AssetBundleCleanUpStreamingAssets();
                });

                UnityEngine.Debug.Log("KSwordKit: 资源管理/清理资源包和treamingAssets文件夹 -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
            }
            catch(System.Exception e){
                UnityEngine.Debug.LogError(e.Message);
            }
            EditorUtility.ClearProgressBar();
        }


        /// <summary>
        /// 记录参数action执行时间的秒表
        /// </summary>
        /// <param name="action">要执行的任务</param>
        /// <returns>参数action执行时间的秒表对象</returns>
        static Stopwatch watchIt(System.Action action)
        {
            var watch = new Stopwatch();
            watch.Start();
            if(action != null)
                action();
            watch.Stop();
            return watch;
        }
        /// <summary>
        /// 设置单个AssetBundle的Name
        /// </summary>
        /// <param name="assetFilePath">项目资源文件路径</param>
        static void setABNameByFile(string abName, string assetFilePath)
        {
           // UnityEngine.Debug.Log("KSwordKit: 待处理包名：" + abName + " 文件路径：" + assetFilePath);
            EditorUtility.DisplayProgressBar("正在设置资源包名...", "正在设置文件：" + assetFilePath, Random.Range(0f,1));

            var ext = System.IO.Path.GetExtension(assetFilePath).ToLower();
            
            if (System.IO.Path.GetFileNameWithoutExtension(assetFilePath) == "" ||
                ext == ".meta" ||
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
                ext == ".lua"||
                ext == ".plist")
                return;

            if(System.IO.Path.GetExtension(assetFilePath) == ".unity")
            {
                abName = ResourcesManagement.ConvertAssetPathToAssetBundleName(assetFilePath);
            }
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetFilePath);  //得到Asset
            assetImporter.assetBundleName = abName;    //最终设置assetBundleName
            
            // if(ext.StartsWith("."))
            //     ext = "._" + ext.Substring(1);
            // assetImporter.assetBundleVariant = ext;

            abNameDic[assetFilePath] = abName;
            // UnityEngine.Debug.Log("KSwordKit: 包名：" + assetImporter.assetBundleName + " 文件路径：" + assetFilePath);
        }
        static Dictionary<string, string> abNameDic = new Dictionary<string, string>();
        /// <summary>
        /// 创建AssetBundle输出目录
        /// </summary>
        /// <returns></returns>
        static string assetBundleOutputDirectory()
        {
            var outputPath = System.IO.Path.Combine(AssetBundles, EditorUserBuildSettings.activeBuildTarget.ToString());
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            return outputPath;
        }
        static string asssetBundleStreamingAssetsDirctory(){
            var dir = System.IO.Path.Combine(Application.streamingAssetsPath, AssetBundles);
            return System.IO.Path.Combine(dir,EditorUserBuildSettings.activeBuildTarget.ToString());
        }
        static void writeResourceListFile(string filepath)
        {        

            EditorUtility.DisplayProgressBar("正在生成资源包..", "准备写入资源清单...", 0.1f);

            System.IO.StreamWriter sw = null;
            if(!System.IO.File.Exists(filepath))
            {
                sw = System.IO.File.CreateText(filepath);
            }
            else
            {
                sw = new System.IO.StreamWriter(filepath, false);
            }
            sw.WriteLine("ResourcesFullName,AssetBundleName,AssetBundleVariant,ObjectName");

            
            EditorUtility.DisplayProgressBar("正在生成资源包..", "准备写入资源清单...", 0.2f);

            foreach(var file in new System.IO.DirectoryInfo(assetBundleOutputDirectory()).GetFiles())
            {
                if(System.IO.Path.GetExtension(file.FullName) == ".manifest")
                    continue;
                if(System.IO.Path.GetFileName(file.FullName) == ResourcesFileName)
                    continue;
                var ab = AssetBundle.LoadFromFile(file.FullName);
                if(ab.isStreamedSceneAssetBundle)
                {
                    var allassets = ab.GetAllScenePaths();
                    foreach(var p in allassets)
                    {
                        EditorUtility.DisplayProgressBar("正在生成资源包..", "添加：" + p, Random.Range(0f,1));

                        var ex = System.IO.Path.GetExtension(p);
                        if(ex.StartsWith("."))
                            ex = ex.Substring(1);
                        sw.WriteLine(p +"," + file.Name + "," + ex);
                    }

                }
                else
                {
                    var allassets = ab.GetAllAssetNames();
                    foreach(var o in allassets)
                    {
                        EditorUtility.DisplayProgressBar("正在生成资源包..", "添加：" + o, Random.Range(0f,1));

                        var ex = System.IO.Path.GetExtension(o);
                        if(ex.StartsWith("."))
                            ex = ex.Substring(1);
                        sw.WriteLine(o +","+file.Name+","+ex);
                    }
                }

                ab.Unload(true);
            }

            sw.Close();

            EditorUtility.DisplayProgressBar("正在生成资源包..", "资源清单准备完成！", 1f);
        }
        
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
                    EditorUtility.DisplayProgressBar("正在拷贝到 StreamingAssets..", "正在拷贝："+ fName, Random.Range(0f,1));
                    System.IO.File.Copy(System.IO.Path.Combine(sourceDir, fName), System.IO.Path.Combine(destDir, fName), true);
                }
            }
            catch (System.IO.DirectoryNotFoundException dirNotFound)
            {
                throw new System.IO.DirectoryNotFoundException(dirNotFound.Message);
            }
        }
    }
}