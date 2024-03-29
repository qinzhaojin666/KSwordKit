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
using KSwordKit.Contents.ResourcesManagement.GeneratedResourcePath;

namespace KSwordKit.Contents.ResourcesManagement.Editor
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
        /// 如果有被选中的文件或文件夹，则对被选中的对象进行打包（如果相应的 AssetBundleName 有值）。
        /// </summary>
        [MenuItem("Assets/KSwordKit/资源管理/生成资源包（默认位置）（当前编译平台）", false, 10)]
        [MenuItem("KSwordKit/资源管理/生成资源包（默认位置）（当前编译平台）", false, 10)]
        public static void AssetBundleBuildAll()
        {
            EditorUtility.DisplayProgressBar("生成资源包（默认位置）（当前编译平台）", "程序执行中...", 0);
            bool isError = false;
            string loginfo = null;
            var watch = Watch.Do(() =>
            {
                try
                {
                    var outputPath = assetBundleOutputDirectory();
                    string error = null;
                    try
                    {
                        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
                    }
                    catch (System.Exception e)
                    {
                        error = e.Message;
                    }

                    if (string.IsNullOrEmpty(error))
                    {
                        GenResourceList();
                        loginfo = "（全部生成）";
                    }
                    else
                    {
                        Debug.LogError(KSwordKitName + ": 执行 `生成资源包（默认位置）（当前编译平台）（选中的资源）` 时，发生错误 -> " + error);
                    }
                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    isError = true;
                    Debug.LogError(KSwordKitName + ": 执行 `生成资源包（默认位置）（当前编译平台）（选中的资源）` 时，发生错误 -> " + e.Message);
                }

            });

            EditorUtility.ClearProgressBar();
            if (!isError)
                UnityEngine.Debug.Log("KSwordKit: 资源管理/生成资源包（默认位置）（当前编译平台）" + loginfo + " -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
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
                var manifest = ParseAssetBundleManifest(System.IO.Path.Combine(outputdirpath, ResourceRootDirectoryName + ".manifest"), ResourceRootDirectoryName);
                sw.Write(JsonUtility.ToJson(manifest, true));
                MakeBuildResourcesConst(manifest, ResourcesManager.GeneratedResourcePathFilePath);
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
        public static AssetBundleManifest ParseAssetBundleManifest(string manifestPath, string assetbundleName = null)
        {

            var lines = System.IO.File.ReadAllLines(manifestPath);

            var manifest = new AssetBundleManifest();
            manifest.AssetBundleName = assetbundleName;
            var assetbundlepath = manifestPath.Replace('\\', '/');
            manifest.AssetBundlePath = assetbundlepath.Substring(0, assetbundlepath.Length - ".manifest".Length);
            bool isNewDependencyItem = false;

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
                else if (k.StartsWith("Info", System.StringComparison.Ordinal))
                    isNewDependencyItem = false;
                else if (k == "Name")
                {
                    var name = v.Trim('"');
                    name = Regex.Unescape(name);
                    var r = ParseResourceManifest(System.IO.Path.Combine(assetBundleOutputDirectory(), name + ".manifest"), name);
                    if (manifest.AssetBundleInfos == null)
                        manifest.AssetBundleInfos = new List<ResourceManifest>();
                    manifest.AssetBundleInfos.Add(r);
                }
                else if (k == "Dependencies" && v != "{}")
                {
                    isNewDependencyItem = true;
                    continue;
                }

                if (isNewDependencyItem && k.StartsWith("Dependency_", StringComparison.Ordinal))
                {
                    var name = v.Trim('"');
                    name = Regex.Unescape(name);
                    var lastman = manifest.AssetBundleInfos[manifest.AssetBundleInfos.Count - 1];
                    if (lastman.Dependencies == null)
                        lastman.Dependencies = new List<string>();
                    lastman.Dependencies.Add(name);
                }
            }

            return manifest;
        }
        /// <summary>
        /// 解析资源包 .manifest 的方法
        /// </summary>
        /// <param name="manifestPath">资源包 .manifest 文件路径 </param>
        /// <param name="assetbundleName">资源标签名</param>
        /// <returns></returns>
        static ResourceManifest ParseResourceManifest(string manifestPath, string assetbundleName)
        {
            var lines = System.IO.File.ReadAllLines(manifestPath);

            var manifest = new ResourceManifest();
            manifest.AssetBundleName = assetbundleName;
            var assetbundlepath = new System.IO.FileInfo(manifestPath).FullName; 
            manifest.AssetBundlePath = assetbundlepath.Substring(0, assetbundlepath.Length - ".manifest".Length);
            manifest.AssetBundlePath = manifest.AssetBundlePath.Replace(new System.IO.DirectoryInfo(assetBundleOutputDirectory()).FullName, "");
            manifest.AssetBundlePath = manifest.AssetBundlePath.Replace('\\', '/');
            manifest.AssetBundlePath = manifest.AssetBundlePath.Substring(1);
            bool isResourceObject = false;

            EditorUtility.DisplayProgressBar("生成资源清单", "正在分析资源包: " + manifest.AssetBundlePath, 0.2f);

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                var index = -1;
                bool isFenhao = false;
                for (var j = 0; j < line.Length; j++)
                {
                    var c = line[j];
                    if (c == '"' && isFenhao)
                        isFenhao = false;
                    else if (c == '"')
                        isFenhao = true;

                    if (!isFenhao)
                    {
                        if (c == ':')
                        {
                            index = j;
                            break;
                        }
                    }
                }

                string[] kv = null;
                bool isSetd = false;
                if (index == -1 && !isResourceObject)
                    continue;
                else if(index == -1)
                {
                    kv = new string[] {
                        line,
                        ""
                    };
                    isSetd = true;
                }
                if (!isSetd)
                {
                    kv = new string[] {
                        line.Substring(0, index),
                        line.Substring(index+1)
                    };
                }

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
                else if (k == "Assets")
                {
                    isResourceObject = true;
                    continue;
                }
                else if (k == "Dependencies" && isResourceObject)
                    isResourceObject = false;

                if (isResourceObject)
                {
                    var resourcePath = k.Substring(2).Trim('"');
                    resourcePath = Regex.Unescape(resourcePath);
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

            return manifest;
        }
        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="assetBundleManifest"></param>
        static void MakeBuildResourcesConst(AssetBundleManifest assetBundleManifest, string filePath)
        {
            var dir = filePath.Substring(0, filePath.Length - System.IO.Path.GetFileName(filePath).Length - 1);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            var dic = new Dictionary<string, List<string>>();
            var list = new List<KeyValuePair<string, List<string>>>();
            var maxCount = 0;
            foreach(var item in assetBundleManifest.AssetBundleInfos)
            {
                foreach(var ro in item.ResourceObjects)
                { 
                    if (dic.ContainsKey(ro.ResourcePath))
                        continue;
                    var items = ro.ResourcePath.Split('/');
                    if (items.Length > maxCount)
                        maxCount = items.Length;
                    dic.Add(ro.ResourcePath, new List<string>(items));
                }
            }
            foreach (var kv in dic)
                list.Add(kv);
            list.Sort((kv1, kv2) => {
                if (kv1.Value.Count > kv2.Value.Count) return 1;
                if (kv1.Value.Count < kv2.Value.Count) return -1;
                return 0;
            });

            var headStr = @"
/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: BuildConst.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-7-6
 *  File Description: 
 *      该文件由 KSwordKit 生成，每次生成资源包就会重新生成该文件，请勿修改里面的内容。
 *      该文件用于快速查找资源对象的引用路径，编写代码时方便快捷。
 *************************************************************************/
";
            var namespaceStr = @"
namespace KSwordKit.Contents.ResourcesManagement.{文件名}
{
    {字符串替换位置}
}
";
            var filecontent = headStr;
            namespaceStr = namespaceStr.Replace("{文件名}", System.IO.Path.GetFileNameWithoutExtension(filePath));

            var AssetPathObject = new GeneratedResourcePathObject();
            AssetPathObject.Name = "Assets";
            AssetPathObject.Path = AssetPathObject.Name;
            GeneratedResourcePathObject prePathObject = null;
            string path = null;
            foreach (var kv in list)
            {
                //Debug.Log("kv = " + kv.Key + ", " + kv.Value.Count);
                prePathObject = AssetPathObject;
                path = AssetPathObject.Name;
                for (var i = 1; i < kv.Value.Count; i++)
                {
                    path += "/" + kv.Value[i];
                    var name = kv.Value[i];
                    name = name.Replace('.', '_');
                    name = name.Replace("-", "中划线");
                    if (name[0] == '0' ||
                        name[0] == '1' ||
                        name[0] == '2' ||
                        name[0] == '3' ||
                        name[0] == '4' ||
                        name[0] == '5' ||
                        name[0] == '6' ||
                        name[0] == '7' ||
                        name[0] == '8' ||
                        name[0] == '9')
                        name = "_" + name;
                    
                    var o = prePathObject.Objects.Find((grpo) => grpo.Name == name);
                    if(o == null)
                    {
                        var pathObject = new GeneratedResourcePathObject();
                        pathObject.Name = name;
                        if (i == kv.Value.Count - 1)
                            pathObject.Path = kv.Key;
                        else
                            pathObject.Path = path;
                        prePathObject.Objects.Add(pathObject);
                        o = pathObject;
                    }
                    prePathObject = o;
                }
            }
            var allClassStr = AssetPathObject.ClassImplementation;
            namespaceStr = namespaceStr.Replace("{字符串替换位置}", allClassStr);
            filecontent += namespaceStr;
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            System.IO.File.WriteAllText(filePath, filecontent, System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();
        }
        
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
                }
                catch (System.Exception e)
                {
                    isError = true;
                    Debug.LogError(KSwordKitName + ": 执行 `拷贝资源包到 StreamingAssets` 时，发生错误 -> " + e.Message);
                }
            });
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            if(!isError)
                UnityEngine.Debug.Log("KSwordKit: 资源管理/拷贝资源包到 StreamingAssets -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
        }

        [MenuItem("Assets/KSwordKit/资源管理/拷贝资源包到 PersistentDataPath", false, 101)]
        [MenuItem("KSwordKit/资源管理/拷贝资源包到 PersistentDataPath", false, 101)]
        public static void CopyResourcesToPersistentDataPath()
        {
            EditorUtility.DisplayProgressBar("拷贝资源包到 PersistentDataPath", "程序执行中...", 0);
            bool isError = false;
            var watch = Watch.Do(() => {
                try
                {
                    var outputPath = System.IO.Path.Combine(Application.persistentDataPath, AssetBundles);
                    outputPath = System.IO.Path.Combine(outputPath, EditorUserBuildSettings.activeBuildTarget.ToString());
                    if (System.IO.Directory.Exists(outputPath))
                        FileUtil.DeleteFileOrDirectory(outputPath);
                    var metapath = outputPath + ".meta";
                    if (System.IO.File.Exists(metapath))
                        FileUtil.DeleteFileOrDirectory(metapath);

                    var sPath = System.IO.Path.Combine(AssetBundles, EditorUserBuildSettings.activeBuildTarget.ToString());
                    CopyFolder(sPath, outputPath);
                }
                catch (System.Exception e)
                {
                    isError = true;
                    Debug.LogError(KSwordKitName + ": 执行 `拷贝资源包到 PersistentDataPath` 时，发生错误 -> " + e.Message);
                }
            });
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            if (!isError)
            {
                UnityEngine.Debug.Log("KSwordKit: 资源管理/拷贝资源包到 PersistentDataPath -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
            }
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
        [MenuItem("Assets/KSwordKit/资源管理/清理资源包（默认位置）（当前编译平台）", false, 1002)]
        [MenuItem("KSwordKit/资源管理/清理资源包（默认位置）（当前编译平台）", false, 1002)]
        public static void AssetBundleCleanUpAssetBundles()
        {
            if (!EditorUtility.DisplayDialog("是否要清理资源包？", "清理后无法恢复！", "确认清理", "取消操作"))
            {
                Debug.Log(KSwordKitName + ": 资源管理/清理资源包（默认位置）（当前编译平台） -> 已取消！");
                return;
            }

            EditorUtility.DisplayProgressBar("清理资源包（默认位置）（当前编译平台）", "程序执行中...", 0);
            bool isError = false;

            var watch = Watch.Do(() => {
                try
                {
                    EditorUtility.DisplayProgressBar("清理资源包（默认位置）（当前编译平台）", "正在清理...", 0.5f);
                    var outputPath = System.IO.Path.Combine(AssetBundles, EditorUserBuildSettings.activeBuildTarget.ToString());

                    if (System.IO.Directory.Exists(outputPath))
                        FileUtil.DeleteFileOrDirectory(outputPath);
                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    isError = true;
                    Debug.LogError(KSwordKitName + ": 执行 `清理资源包（默认位置）（当前编译平台）` 时，发生错误 -> " + e.Message);
                }
            });

            EditorUtility.ClearProgressBar();

            if (!isError)
                UnityEngine.Debug.Log("KSwordKit: 资源管理/清理资源包（默认位置）（当前编译平台） -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
        }

        [MenuItem("Assets/KSwordKit/资源管理/清理资源包 (StreamingAssets)（当前编译平台）", false, 1003)]
        [MenuItem("KSwordKit/资源管理/清理资源包 (StreamingAssets)（当前编译平台）", false, 1003)]
        public static void AssetBundleCleanUpStreamingAssets()
        {
            if (!EditorUtility.DisplayDialog("是否要清理资源包 (StreamingAssets)（当前编译平台） ？", "清理后无法恢复！", "确认清理", "取消操作"))
            {
                Debug.Log(KSwordKitName + ": 资源管理/清理资源包 (StreamingAssets)（当前编译平台） -> 已取消！");
                return;
            }

            EditorUtility.DisplayProgressBar("清理资源包 (StreamingAssets)（当前编译平台）", "程序执行中...", 0);
            bool isError = false;

            var watch = Watch.Do(() => {

                try
                {
                    EditorUtility.DisplayProgressBar("清理资源包 (StreamingAssets)（当前编译平台）", "正在清理...", 0.5f);
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
                    Debug.LogError(KSwordKitName + ": 执行 `清理资源包 (StreamingAssets)（当前编译平台）` 时，发生错误 -> " + e.Message);
                }
            });

            EditorUtility.ClearProgressBar();

            if (!isError)
                UnityEngine.Debug.Log("KSwordKit: 资源管理/清理资源包 (StreamingAssets)（当前编译平台） -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");

        }

        [MenuItem("Assets/KSwordKit/资源管理/清理资源包 (PersistentDataPath)（当前编译平台）", false, 1003)]
        [MenuItem("KSwordKit/资源管理/清理资源包 (PersistentDataPath)（当前编译平台）", false, 1003)]
        public static void AssetBundleCleanUpPersistentDataPath()
        {
            if (!EditorUtility.DisplayDialog("是否要清理资源包 (PersistentDataPath)（当前编译平台） ？", "清理后无法恢复！", "确认清理", "取消操作"))
            {
                Debug.Log(KSwordKitName + ": 资源管理/清理资源包 (PersistentDataPath)（当前编译平台） -> 已取消！");
                return;
            }

            EditorUtility.DisplayProgressBar("清理资源包 (PersistentDataPath)（当前编译平台）", "程序执行中...", 0);
            bool isError = false;

            var watch = Watch.Do(() => {
                try
                {
                    EditorUtility.DisplayProgressBar("清理资源包 (PersistentDataPath)（当前编译平台）", "正在清理...", 0.5f);
                    var outputPath = System.IO.Path.Combine(Application.persistentDataPath, AssetBundles);
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
                    Debug.LogError(KSwordKitName + ": 执行 `清理资源包 (PersistentDataPath)（当前编译平台）` 时，发生错误 -> " + e.Message);
                }
            });

            EditorUtility.ClearProgressBar();

            if (!isError)
                UnityEngine.Debug.Log("KSwordKit: 资源管理/清理资源包 (PersistentDataPath)（当前编译平台） -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
        }
    }
}


