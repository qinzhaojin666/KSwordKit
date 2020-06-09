using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;

namespace KSwordKit.Core.AssetManagement.Editor
{
    public class AssetsEditor
    {
        /// <summary>
        /// AssetBundles 的名称，用于AssetBundles输出位置
        /// </summary>
        public const string AssetBundles = "AssetBundles";

        /// <summary>
        /// 标记所有被选中的文件
        /// </summary>
        [MenuItem("Assets/KSwordKit/AssetBundle/命名/文件")]
        [MenuItem("KSwordKit/AssetBundle/命名/文件")]
        public static void AssetBundleMarkFile()
        {
            var watch = watchIt(() => {
                float i = 0;
                var max = Selection.objects.Length;
                EditorUtility.DisplayProgressBar("正在命名文件...", "等待程序执行..", i);
                try
                {
                    foreach (var path in Selection.objects)
                    {
                        var p = AssetDatabase.GetAssetPath(path);
                        EditorUtility.DisplayProgressBar("正在命名文件...", "正在处理：" + p, (++i) / max);

                        if (System.IO.File.Exists(p))
                            setABNameByFile(p, p);
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError(e.Message);
                }
                EditorUtility.ClearProgressBar();
            });
            UnityEngine.Debug.Log("KSwordKit: AssetBundle/命名/文件 -> 完成! (" + watch.ElapsedMilliseconds + "ms)");
        }
        /// <summary>
        /// 标记所有被选中的文件夹
        /// </summary>
        [MenuItem("Assets/KSwordKit/AssetBundle/命名/文件夹")]
        [MenuItem("KSwordKit/AssetBundle/命名/文件夹")]
        public static void AssetBundleMarkDirctory()
        {
            var watch = watchIt(() => {

                float i = 0;
                var max = Selection.objects.Length;
                EditorUtility.DisplayProgressBar("正在命名文件夹...", "等待程序执行..", i);
                try
                {
                    foreach (var path in Selection.objects)
                    {
                        var p = AssetDatabase.GetAssetPath(path);
                        EditorUtility.DisplayProgressBar("正在命名文件夹...", "正在处理：" + p, (++i) / max);
                        if (System.IO.Directory.Exists(p))
                            setAssetBundlesNameByDirctory(p, p);
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError(e.Message);
                }
                EditorUtility.ClearProgressBar();
            });
            UnityEngine.Debug.Log("KSwordKit: AssetBundle/命名/文件夹 -> 完成! (" + watch.ElapsedMilliseconds + "ms)");
        }
        /// <summary>
        /// 清理所有标记的内容
        /// </summary>
        [MenuItem("Assets/KSwordKit/AssetBundle/清除命名")]
        [MenuItem("KSwordKit/AssetBundle/清除命名")]
        public static void AssetBundlesCleanUpAssetBundleName()
        {
            var watch = watchIt(() => {
                bool isall = true;
                getEachSelectionObject((obj) => {
                    var dir = AssetDatabase.GetAssetPath(obj);
                    if (System.IO.Directory.Exists(dir))
                    {
                        if (isall)
                            isall = false;
                        assetBundlesCleanUpAssetBundleName(dir);
                    }
                    else if (System.IO.File.Exists(dir))
                    {
                        if (isall)
                            isall = false;
                        AssetImporter assetImporter = AssetImporter.GetAtPath(dir);  //得到Asset
                        if (assetImporter != null)
                            assetImporter.assetBundleName = null;
                    }
                });

                if (isall)
                    assetBundlesCleanUpAssetBundleName("Assets");
                AssetDatabase.Refresh();
            });
            UnityEngine.Debug.Log("KSwordKit: AssetBundle -> 清除命名完成! (" + watch.ElapsedMilliseconds + "ms)");
        }
        /// <summary>
        /// 按当前目标平台对所有被标记的资源生成AssetBundle
        /// </summary>
        [MenuItem("Assets/KSwordKit/AssetBundle/生成 AssetBundle")]
        [MenuItem("KSwordKit/AssetBundle/生成 AssetBundles")]
        public static void AssetBundleBuildAll()
        {
            var watch = watchIt(assetBundleBuildAll);
            UnityEngine.Debug.Log("KSwordKit: AssetBundle/生成 AssetBundles -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
        }
        /// <summary>
        /// 清理AssetBundle文件
        /// </summary>
        [MenuItem("Assets/KSwordKit/AssetBundle/清理 AssetBundles")]
        [MenuItem("KSwordKit/AssetBundle/清理 AssetBundles")]
        public static void AssetBundleCleanUpAssetBundles()
        {
            var watch = watchIt(assetBundleCleanUpAssetBundles);
            UnityEngine.Debug.Log("KSwordKit: AssetBundle/清理 AssetBundles -> 完成! (" + watch.Elapsed.TotalSeconds + "s)");
        }

        // -------------------------------- 私有函数 ------------------------------------- \\
        /// <summary>
        /// 记录参数action执行时间的秒表
        /// </summary>
        /// <param name="action">要执行的任务</param>
        /// <returns>参数action执行时间的秒表对象</returns>
        static Stopwatch watchIt(System.Action action)
        {
            var watch = new Stopwatch();
            watch.Start();
            action();
            watch.Stop();
            return watch;
        }
        /// <summary>
        /// 获得每一个选中的对象
        /// </summary>
        /// <param name="itemAction">每个被选中对象的回调动作</param>
        static void getEachSelectionObject(System.Action<Object> itemAction)
        {
            foreach (var item in Selection.objects)
                itemAction(item);
        }
        /// <summary>
        /// 根据文件路径获得目录路径
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件路径所在的目录路径</returns>
        static string getFileDirectory(string filePath)
        {
            var path = filePath;
            if (path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                path = path.Substring(0, filePath.Length - 1);
            var index = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
            if (index == -1)
                return path;
            else
                return path.Substring(0, index);
        }
        /// <summary>
        /// 创建AssetBundle输出目录
        /// </summary>
        /// <returns></returns>
        static string assetBundleOutputDirectory()
        {
            var outputPath = System.IO.Path.Combine(Application.streamingAssetsPath + "/" + AssetBundles, EditorUserBuildSettings.activeBuildTarget.ToString());
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            return outputPath;
        }
        /// <summary>
        /// 设置所有在指定路径下的AssetBundleName
        /// </summary>
        static void setAssetBundlesNameByDirctory(string abName, string dirpath)
        {
            var dirinfo = new System.IO.DirectoryInfo(dirpath);
            foreach (var fp in dirinfo.GetFiles())
                setABNameByFile(abName, System.IO.Path.Combine(dirpath, fp.Name));
            foreach (var dir in dirinfo.GetDirectories())
                setAssetBundlesNameByDirctory(abName, System.IO.Path.Combine(dirpath, dir.Name));
        }
        /// <summary>
        /// 设置单个AssetBundle的Name
        /// </summary>
        /// <param name="assetFilePath">项目资源文件路径</param>
        static void setABNameByFile(string abName, string assetFilePath)
        {
            if (System.IO.Path.GetFileName(assetFilePath) == "markok" ||
                System.IO.Path.GetExtension(assetFilePath) == ".meta" ||
                System.IO.Path.GetExtension(assetFilePath) == ".cs")
                return;
            var extension = System.IO.Path.GetExtension(abName);
            var filename = System.IO.Path.GetFileNameWithoutExtension(abName) + (string.IsNullOrEmpty(extension) ? "" : "_" + extension.Substring(1));
            var parentpath = abName.Substring(0, abName.Length - filename.Length);

            // 处理特殊文件
            if (System.IO.Path.GetExtension(assetFilePath) == ".unity")
            {
                UnityEngine.Debug.Log("KSwordKit: 处理场景文件：" + assetFilePath);
                extension = System.IO.Path.GetExtension(assetFilePath);
                filename = System.IO.Path.GetFileNameWithoutExtension(assetFilePath) + (string.IsNullOrEmpty(extension) ? "" : "_" + extension.Substring(1));
                parentpath = assetFilePath.Substring(0, assetFilePath.Length - filename.Length);
                abName = System.IO.Path.Combine(parentpath, filename);
            }
            else
                abName = System.IO.Path.Combine(parentpath, filename);
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetFilePath);  //得到Asset
            abName = abName.Replace('\\', '/');
            abName = abName.Replace("/", "__");
            assetImporter.assetBundleName = abName;    //最终设置assetBundleName
            UnityEngine.Debug.Log("KSwordKit: 包名：" + assetImporter.assetBundleName + " 文件路径：" + assetFilePath);
        }


        static List<string> markOKDirPaths = new List<string>();
        static List<string> markFilePaths = new List<string>();
        static List<string> markDirPaths = new List<string>();
        /// <summary>
        /// 清理所有标记的内容
        /// </summary>
        static void assetBundlesCleanUpAssetBundleName(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "Assets")
            {
                string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
                for (int i = 0; i < allAssetBundleNames.Length; i++)
                {
                    string text = allAssetBundleNames[i];
                    AssetDatabase.RemoveAssetBundleName(text, true);
                }
            }
            else
            {
                var dirinfo = new System.IO.DirectoryInfo(path);
                foreach (var file in dirinfo.GetFiles())
                {
                    if (file.Name == "markok" || file.Extension == ".meta")
                        continue;

                    AssetImporter assetImporter = AssetImporter.GetAtPath(System.IO.Path.Combine(path, file.Name));  //得到Asset
                    AssetDatabase.RemoveAssetBundleName(assetImporter.assetBundleName, true);
                }
                foreach (var info in dirinfo.GetDirectories())
                    assetBundlesCleanUpAssetBundleName(System.IO.Path.Combine(path, info.Name));
            }
        }
        /// <summary>
        /// 按当前目标平台对所有被标记的资源生成AssetBundle
        /// </summary>
        public static void assetBundleBuildAll()
        {
            EditorUtility.DisplayProgressBar("正在 Build AssetBundle..", "程序执行中...", 0);
            try
            {
                var outputPath = assetBundleOutputDirectory();
                BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 清理AssetBundle文件
        /// </summary>
        static void assetBundleCleanUpAssetBundles()
        {
            var outputPath = System.IO.Path.Combine(Application.streamingAssetsPath + "/" + AssetBundles, EditorUserBuildSettings.activeBuildTarget.ToString());
            if (System.IO.Directory.Exists(outputPath))
                FileUtil.DeleteFileOrDirectory(outputPath);
        }

    }
}