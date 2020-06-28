/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ResourcesManagement.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-7
 *  File Description: Ignore.
 *************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace KSwordKit.Core.ResourcesManagement
{
    public class ResourcesManagement : MonoBehaviour
    {
        public const string NAME = "ResourcesManagement";
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

        static ResourcesManagement _Instance;
        public static ResourcesManagement Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new GameObject(NAME).AddComponent<ResourcesManagement>();
                    GameObject.DontDestroyOnLoad(_Instance.gameObject);
                }
                return _Instance;
            }
        }



        /// <summary>
        /// 初始化资源管理器
        /// <para>默认初始化方式，会从参数指示的位置，按照方法<see cref="GetResourceListFilePath"/>返回的路径进行加载。</para>
        /// </summary>
        /// <param name="resourcesLoadingLocation">加载资源的位置</param>
        /// <returns>自身实例对象</returns>
        public static ResourcesManagement Init(ResourcesLoadingLocation resourcesLoadingLocation)
        {
            Instance._isInitd = false;
            Instance._initProgress = 0;
            Instance._initError = null;
            _resourcesLoadingLocation = resourcesLoadingLocation;

            LoadResourcesListAsync((progress, error, rbm) => {
                if (Instance._OnInitializing != null)
                    Instance._OnInitializing(Instance, progress);

                if(progress == 1)
                {
                    Instance._isInitd = true;
                    Instance._initProgress = 1;
                    Instance._initError = error;

                    if (Instance._OnInitCompleted != null)
                        Instance._OnInitCompleted(Instance, Instance._initError);
                }
            });

            return Instance;
        }
        /// <summary>
        /// 资源管理器的初始化
        /// <para>针对可以直接提供资源清单文本内容的情况</para>
        /// </summary>
        /// <param name="resourcesLoadingLocation">资源位置</param>
        /// <param name="resourceslist_jsonString">资源清单的文本内容</param>
        /// <returns>自身实例对象</returns>
        public static ResourcesManagement Init(ResourcesLoadingLocation resourcesLoadingLocation, string resourceslist_jsonString)
        {
            Instance._isInitd = false;
            Instance._initProgress = 0;
            Instance._initError = null;
            _resourcesLoadingLocation = resourcesLoadingLocation;
            try
            {
                var r = JsonUtility.FromJson<AssetBundleManifest>(resourceslist_jsonString);
                if (r != null)
                    _AssetBundleManifest = r;
            }
            catch (System.Exception e)
            {
                Instance._initError = KSwordKitName + ": " + e.Message;
            }
            if (Instance._OnInitializing != null)
                Instance._OnInitializing(Instance, 1);
            if (Instance._OnInitCompleted != null)
                Instance._OnInitCompleted(Instance, Instance._initError);

            return Instance;
        }
        /// <summary>
        /// 资源管理器的初始化
        /// <para>针对提供自定义资源清单文件json数据的加载请求的情况</para>
        /// <para>大多用于资源清单位于服务器的时候</para>
        /// </summary>
        /// <param name="resourcesLoadingLocation">资源位置</param>
        /// <param name="resourceslistJsonFileWebRequest">配置好了的加载资源清单json数据的请求对象</param>
        /// <returns>自身实例对象</returns>
        public static ResourcesManagement Init(ResourcesLoadingLocation resourcesLoadingLocation, UnityEngine.Networking.UnityWebRequest resourceslistJsonFileWebRequest)
        {
            Instance._isInitd = false;
            Instance._initProgress = 0;
            Instance._initError = null;
            _resourcesLoadingLocation = resourcesLoadingLocation;

            Instance.StartCoroutine(loadResourcesList(resourceslistJsonFileWebRequest, (progress, error, rbm) =>
            {
                if (Instance._OnInitializing != null)
                    Instance._OnInitializing(Instance, progress);

                if (progress == 1)
                {
                    Instance._isInitd = true;
                    Instance._initProgress = 1;
                    Instance._initError = error;

                    if (Instance._OnInitCompleted != null)
                        Instance._OnInitCompleted(Instance, Instance._initError);
                }
            }));

            return Instance;
        }
        /// <summary>
        /// 初始化完成
        /// <para>如果初始化过程中发生错误时，完成时带有错误信息回调。</para>
        /// <para>如果初始化过程中没有任何错误，完成时带有null回调。</para>
        /// </summary>
        /// <returns>自身实例对象</returns>
        public ResourcesManagement OnInitCompleted(System.Action<ResourcesManagement, string> action)
        {
            _OnInitCompleted += action;
            if (_isInitd)
                action(this, _initError);
            return Instance;
        }
        /// <summary>
        /// 初始化完成事件
        /// </summary>
        event System.Action<ResourcesManagement, string> _OnInitCompleted;
        bool _isInitd;
        float _initProgress;
        string _initError = null;
        /// <summary>
        /// 正在初始化
        /// <para>参数为初始化进度回调</para>
        /// </summary>
        /// <param name="progressAction">初始化进度回调</param>
        /// <returns>自身实例对象</returns>
        public ResourcesManagement OnInitializing(System.Action<ResourcesManagement, float> progressAction)
        {
            _OnInitializing += progressAction;
            if (_isInitd)
                progressAction(this, 1);

            return Instance;
        }
        /// <summary>
        /// 初始化进度事件
        /// </summary>
        event System.Action<ResourcesManagement, float> _OnInitializing;
        static UnityEngine.Networking.UnityWebRequest _UnityWebRequest;
        static ResourcesLoadingLocation _resourcesLoadingLocation = ResourcesLoadingLocation.Resources;
        /// <summary>
        /// 获得资源加载位置
        /// </summary>
        public static ResourcesLoadingLocation ResourcesLoadingLocation { get { return _resourcesLoadingLocation; } }
        static AssetBundleManifest _AssetBundleManifest;
        /// <summary>
        /// 资源清单
        /// </summary>
        public AssetBundleManifest ResourcePackage { get { return _AssetBundleManifest; } }
        static void LoadResourcesListAsync(System.Action<float, string, AssetBundleManifest> action)
        {
            if(_AssetBundleManifest != null)
            {
                action(1, null, _AssetBundleManifest);
                return;
            }

            var path = GetResourceListFilePath();
            // 不是 StreamingAssetsPath 都可以使用System.IO.File读取资源清单
            if (ResourcesLoadingLocation != ResourcesLoadingLocation.StreamingAssetsPath)
            {
                var text = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
                try
                {
                    var r = JsonUtility.FromJson<AssetBundleManifest>(text);
                    if (r != null)
                    {
                        _AssetBundleManifest = r;
                        action(1, null, _AssetBundleManifest);
                    }
                    else throw new IOException("解析资源清单失败，filepath = " + path);
                }
                catch (System.Exception e)
                {
                    action(1, KSwordKitName + ": " + e.Message, null);
                }
            }
            else
            {
                if (IsNeedAddLocalFilePathPrefix())
                    path = "file://" + path;
                Instance.StartCoroutine(loadResourcesList(path, action));
            }
        }
        static IEnumerator loadResourcesList(string path, System.Action<float, string, AssetBundleManifest> action)
        {
            var www = UnityEngine.Networking.UnityWebRequest.Get(path);
            var op = www.SendWebRequest();
            while(!op.isDone)
            {
                action(op.progress, null, null);
                yield return null;
            }

            if(string.IsNullOrEmpty(www.error))
            {
                try
                {
                    var text = www.downloadHandler.text;
                    var r = JsonUtility.FromJson<AssetBundleManifest>(text);
                    if (r != null)
                        _AssetBundleManifest = r;

                    action(1, null, r);
                }
                catch (System.Exception e)
                {
                    action(1, KSwordKitName + ": " + e.Message, null);
                }
            }
            else
            {
                action(1, KSwordKitName + ": " + www.error, null);
            }
        }
        static IEnumerator loadResourcesList(UnityEngine.Networking.UnityWebRequest www, System.Action<float, string, AssetBundleManifest> action)
        {
            var op = www.SendWebRequest();
            while (!op.isDone)
            {
                action(op.progress, null, null);
                yield return null;
            }

            if (string.IsNullOrEmpty(www.error))
            {
                try
                {
                    var text = www.downloadHandler.text;
                    var r = JsonUtility.FromJson<AssetBundleManifest>(text);
                    if (r != null)
                    {
                        _AssetBundleManifest = r;
                        action(1, null, _AssetBundleManifest);
                    }
                    else throw new IOException("解析资源清单失败，url=" + www.url);
                }
                catch (System.Exception e)
                {
                    action(1, KSwordKitName + ": " + e.Message, null);
                }
            }
            else
            {
                action(1, KSwordKitName + ": " + www.error, null);
            }
        }
        /// <summary>
        /// 获取资源清单路径
        /// <para>在资源位置是<see cref="ResourcesLoadingLocation.RemotePath"/>或者<see cref="ResourcesLoadingLocation.PersistentDataPath"/> 都返回由 <see cref="Application.persistentDataPath"/>拼接而成路径</para>
        /// <para>在资源位置是<see cref="ResourcesLoadingLocation.StreamingAssetsPath"/>时，则返回由 <see cref="Application.streamingAssetsPath"/> 拼接而成的路径</para>
        /// <para>在资源位置是<see cref="ResourcesLoadingLocation.Resources"/>时，则默认返回由 资源包生成的位置 拼接而成的路径</para>
        /// </summary>
        /// <returns>资源清单路径</returns>
        public static string GetResourceListFilePath()
        {
            return System.IO.Path.Combine(GetResourcesFileRootDirectory(), ResourcesFileName);
        }
        public static bool IsNeedAddLocalFilePathPrefix(){
            return (!Application.isEditor && Application.platform == RuntimePlatform.IPhonePlayer) || 
                Application.platform == RuntimePlatform.OSXEditor;
        }
        /// <summary>
        /// 获得资源路径根目录
        /// </summary>
        /// <returns>资源根目录</returns>
        public static string GetResourcesFileRootDirectory()
        {
            var path = AssetBundles;
#if UNITY_EDITOR
            path = System.IO.Path.Combine(path, 
            UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString());
#endif
            if (!Application.isEditor)
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                    path = System.IO.Path.Combine(path, "iOS");
                else
                    path = System.IO.Path.Combine(path, Application.platform.ToString());
            }
            switch (ResourcesLoadingLocation)
            {
                case ResourcesLoadingLocation.StreamingAssetsPath:
                    path = System.IO.Path.Combine(Application.streamingAssetsPath, path);
                    break;

                case ResourcesLoadingLocation.RemotePath:
                case ResourcesLoadingLocation.PersistentDataPath:
                    path = System.IO.Path.Combine(Application.persistentDataPath, path);
                    break;
            }
            return path;
        }

        /// <summary>
        /// 异步加载资源
        /// <para>即便 ResourcesLoadingLocation == ResourcesLoadingLocation.Resources, 参数 <paramref name="assetPath"/> 也需要输入资源在项目中的相对路径</para>
        /// <para>路径开头 'Assets' 可省略，如 'Assets/Resources/test.png' 可简写为 'Resources/test.png'</para>
        /// <para>路径分隔符统一为 '/'</para>
        /// <para>当资源存在依赖时，将会自动加载依赖。</para>
        /// </summary>
        /// <param name="assetPath">资产路径</param>
        /// <param name="asyncAction">加载过程中的回调信息</param>
        /// <returns>资源管理器自身实例对象</returns>
        public ResourcesManagement LoadAssetAsync(string assetPath, System.Action<ResourcesManagement, bool, float, string, UnityEngine.Object> asyncAction)
        {
            return LoadAssetAsync<UnityEngine.Object>(assetPath, asyncAction);
        }
        /// <summary>
        /// 异步加载资源
        /// <para>即便 ResourcesLoadingLocation == ResourcesLoadingLocation.Resources, 参数 <paramref name="assetPath"/> 也需要输入资源在项目中的相对路径</para>
        /// <para>路径开头 'Assets' 可省略，如 'Assets/Resources/test.png' 可简写为 'Resources/test.png'</para>
        /// <para>路径分隔符统一为 '/'</para>
        /// <para>当资源存在依赖时，将会自动加载依赖。</para>
        /// </summary>
        /// <param name="assetPath">资产路径</param>
        /// <param name="asyncAction">加载过程中的回调信息</param>
        /// <returns>资源管理器自身实例对象</returns>
        public ResourcesManagement LoadAssetAsync<T>(string assetPath, System.Action<ResourcesManagement, bool, float, string, T> asyncAction) where T: UnityEngine.Object
        {
            ResourcesAsyncLoader<T>.ResourcePackage = ResourcePackage;
            ResourcesAsyncLoader<T>.LoadAsync(assetPath, _resourcesLoadingLocation, (isdone, progress, error, obj) => {
                if (asyncAction != null)
                    asyncAction(this, isdone, progress, error, obj);
            });
            return this;
        }
        /// <summary>
        /// 异步加载一组资源
        /// <para>即便 ResourcesLoadingLocation == ResourcesLoadingLocation.Resources, 参数 <paramref name="assetPaths"/> 中的每一项路径也都需要输入资源在项目中的相对路径</para>
        /// <para>路径开头 'Assets' 可省略，如 'Assets/Resources/test.png' 可简写为 'Resources/test.png'</para>
        /// <para>路径分隔符统一为 '/'</para>
        /// <para>当资源存在依赖时，将会自动加载依赖。</para>
        /// </summary>
        /// <param name="assetPaths">要加载的资源数组</param>
        /// <param name="asyncAction">加载过程中的回调信息</param>
        /// <returns>资源管理器自身实例对象</returns>
        public ResourcesManagement LoadAssetAsync(string[] assetPaths, System.Action<ResourcesManagement, bool, float, string, UnityEngine.Object[]> asyncAction)
        {
            return LoadAssetAsync<UnityEngine.Object>(assetPaths, asyncAction);
        }
        /// <summary>
        /// 异步加载一组资源
        /// <para>即便 ResourcesLoadingLocation == ResourcesLoadingLocation.Resources, 参数 <paramref name="assetPaths"/> 中的每一项路径也都需要输入资源在项目中的相对路径</para>
        /// <para>路径开头 'Assets' 可省略，如 'Assets/Resources/test.png' 可简写为 'Resources/test.png'</para>
        /// <para>路径分隔符统一为 '/'</para>
        /// <para>当资源存在依赖时，将会自动加载依赖。</para>
        /// </summary>
        /// <param name="assetPaths">资产路径</param>
        /// <param name="asyncAction">加载过程中的回调信息</param>
        /// <returns>资源管理器自身实例对象</returns>
        public ResourcesManagement LoadAssetAsync<T>(
            string[] assetPaths, 
            System.Action<ResourcesManagement, bool, float, string, T[]> asyncAction) where T : UnityEngine.Object
        {
            ResourcesAsyncLoader<T>.ResourcePackage = ResourcePackage;
            ResourcesAsyncLoader<T>.LoadAsync(assetPaths, _resourcesLoadingLocation, (isdone, progress, error, objs) => {
                if (asyncAction != null)
                    asyncAction(this, isdone, progress, error, objs);
            });
            return this;
        }
    }
}
