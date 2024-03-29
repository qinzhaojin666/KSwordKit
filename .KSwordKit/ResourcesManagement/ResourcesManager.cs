/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ResourcesManager.cs
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

namespace KSwordKit.Contents.ResourcesManagement
{
    public class ResourcesManager : MonoBehaviour
    {
        public const string NAME = "ResourcesManager";
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
        /// 生成的资源名称路径等相关Const字段文件存放路径
        /// </summary>
        public static string GeneratedResourcePathFilePath = "Assets/GeneratedResourcePath/GeneratedResourcePath.cs";


        static bool instanceExists;
        private void Awake()
        {
            if (instanceExists)
                Destroy(gameObject);
        }

        static ResourcesManager _Instance;
        /// <summary>
        /// ResourcesManager类单例
        /// </summary>
        public static ResourcesManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new GameObject(NAME).AddComponent<ResourcesManager>();
                    instanceExists = true;
                    DontDestroyOnLoad(_Instance.gameObject);
                }
                return _Instance;
            }
        }

        static Dictionary<string, ResourceObject> _ResourceObjectPath_ResourceObjectDic = new Dictionary<string, ResourceObject>();
        static Dictionary<string, ResourceManifest> _AssetbundleName_AssetBundlePathDic = new Dictionary<string, ResourceManifest>();
        /// <summary>
        /// 资源对象路径与资源对象之间的映射关系
        /// </summary>
        public Dictionary<string, ResourceObject> ResourceObjectPath_ResourceObjectDic { get { return _ResourceObjectPath_ResourceObjectDic; } }
        /// <summary>
        /// 资源包名与资源包之间的映射关系
        /// </summary>
        public Dictionary<string, ResourceManifest> AssetbundleName_AssetBundlePathDic { get { return _AssetbundleName_AssetBundlePathDic; } }

        /// <summary>
        /// 初始化资源管理器
        /// <para>默认初始化方式，会从参数指示的位置，按照方法<see cref="GetResourceListFilePath"/>返回的路径进行加载。</para>
        /// </summary>
        /// <param name="resourcesLoadingLocation">加载资源的位置</param>
        /// <returns>自身实例对象</returns>
        public ResourcesManager Init(ResourcesLoadingLocation resourcesLoadingLocation = ResourcesLoadingLocation.Resources, System.Action<bool, float, string> initAction = null)
        {
            if (initAction != null)
                Instance.OnInitEvent += initAction;

            if (Instance._isInitializing || _Instance._isInitDone)
                return Instance;

            Instance._isInitializing = true;
            Instance._isInitDone = false;
            Instance._initProgress = 0;
            Instance._initError = null;
            _resourcesLoadingLocation = resourcesLoadingLocation;

            NextFrame(() => {
                Instance.StartCoroutine(loadResourcesList(onInitFunc));
            });

            return Instance;
        }
        /// <summary>
        /// 资源管理器的初始化
        /// <para>针对可以直接提供资源清单文本内容的情况</para>
        /// </summary>
        /// <param name="resourceslist_jsonString">资源清单的文本内容</param>
        /// <returns>自身实例对象</returns>
        public ResourcesManager Init(string resourceslist_jsonString, System.Action<bool, float, string> initAction = null)
        {
            return Init(ResourcesLoadingLocation.Resources, resourceslist_jsonString, initAction);
        }
        /// <summary>
        /// 资源管理器的初始化
        /// <para>针对可以直接提供资源清单文本内容的情况</para>
        /// </summary>
        /// <param name="resourcesLoadingLocation">资源加载位置</param>
        /// <param name="resourceslist_jsonString">资源清单的文本内容</param>
        /// <returns>自身实例对象</returns>
        public ResourcesManager Init(ResourcesLoadingLocation resourcesLoadingLocation, string resourceslist_jsonString, System.Action<bool, float, string> initAction = null)
        {
            if (initAction != null)
                Instance.OnInitEvent += initAction;

            if (Instance._isInitializing || _Instance._isInitDone)
                return Instance;

            Instance._isInitializing = true;
            Instance._isInitDone = false;
            Instance._initProgress = 0;
            Instance._initError = null;

            NextFrame(() => {
                Instance.StartCoroutine(startThreadHandleResourcePackage(resourceslist_jsonString, onInitFunc));
            });
            return Instance;
        }
        /// <summary>
        /// 资源管理器的初始化
        /// <para>针对提供自定义资源清单文件json数据的加载请求的情况</para>
        /// <para>大多用于资源清单位于服务器的时候</para>
        /// </summary>
        /// <param name="resourceslistJsonFileWebRequest">配置好了的加载资源清单json数据的请求对象</param>
        /// <returns>自身实例对象</returns>
        public ResourcesManager Init(UnityEngine.Networking.UnityWebRequest resourceslistJsonFileWebRequest, System.Action<bool, float, string> initAction = null)
        {
            return Init(ResourcesLoadingLocation.Resources, resourceslistJsonFileWebRequest, initAction);
        }
        /// <summary>
        /// 资源管理器的初始化
        /// <para>针对提供自定义资源清单文件json数据的加载请求的情况</para>
        /// <para>大多用于资源清单位于服务器的时候</para>
        /// </summary>
        /// <param name="resourcesLoadingLocation">资源加载位置</param>
        /// <param name="resourceslistJsonFileWebRequest">配置好了的加载资源清单json数据的请求对象</param>
        /// <returns>自身实例对象</returns>
        public ResourcesManager Init(ResourcesLoadingLocation resourcesLoadingLocation, UnityEngine.Networking.UnityWebRequest resourceslistJsonFileWebRequest, System.Action<bool, float, string> initAction = null)
        {
            if (initAction != null)
                Instance.OnInitEvent += initAction;

            if (Instance._isInitializing || _Instance._isInitDone)
                return Instance;

            Instance._isInitializing = true;
            Instance._isInitDone = false;
            Instance._initProgress = 0;
            Instance._initError = null;
            _resourcesLoadingLocation = resourcesLoadingLocation;

            NextFrame(() => {
                Instance.StartCoroutine(loadResourcesList(resourceslistJsonFileWebRequest, onInitFunc));
            });

            return Instance;
        }
        /// <summary>
        /// 正在初始化
        /// <para>参数为初始化情况回调</para>
        /// <para>回调参数分别为：是否完成、初始化进度、错误信息</para>
        /// </summary>
        event System.Action<bool, float, string> _OnInit;
        /// <summary>
        /// 初始化进度事件
        /// <para>回调参数为：初始化进度</para>
        /// </summary>
        event System.Action<float> _OnInitializing;
        /// <summary>
        /// 初始化成功了
        /// </summary>
        event System.Action _OnInitializedSuccessfully;
        /// <summary>
        /// 初始化失败了
        /// </summary>
        event System.Action<string> _OnInitializationFailed;
        public event System.Action<bool ,float,string> OnInitEvent
        {
            add
            {
                _OnInit += value;
                if(_isInitDone)
                {
                    value(false, 1, null);
                    NextFrame(() => value(true, 1, _initError));
                }
            }
            remove
            {
                _OnInit -= value;
            }
        }
        /// <summary>
        /// 正在初始化
        /// <para>参数为初始化情况回调</para>
        /// <para>回调参数为：初始化进度</para>
        /// </summary>
        public event System.Action<float> OnInitializingEvent
        {
            add
            {
                _OnInitializing += value;
                if (_isInitDone)
                {
                    value(1);
                }
            }
            remove
            {
                _OnInitializing -= value;
            }
        }
        /// <summary>
        /// 初始化成功了
        /// <para>当初始化成功时会触发该事件</para>
        /// </summary>
        public event System.Action OnInitializedSuccessfullyEvent
        {
            add
            {
                _OnInitializedSuccessfully += value;
                if(_isInitDone && string.IsNullOrEmpty(_initError))
                {
                    value();
                }
            }
            remove
            {
                _OnInitializedSuccessfully -= value;
            }
        }
        /// <summary>
        /// 初始化失败了
        /// <para>参数为失败信息</para>
        /// <para>当初始化失败时会触发该事件</para>
        /// </summary>
        public event System.Action<string> OnInitializationFailedEvent
        {
            add
            {
                _OnInitializationFailed += value;
                if (_isInitDone && !string.IsNullOrEmpty(_initError))
                {
                    value(_initError);
                }
            }
            remove
            {
                _OnInitializationFailed -= value;
            }
        }
        /// <summary>
        /// 初始化函数
        /// <para>该函数充当一个内部回调函数，统一处理初始化相关事件。</para>
        /// </summary>
        /// <param name="isdone">是否完成</param>
        /// <param name="progress">初始化进度</param>
        /// <param name="error">错误信息</param>
        void onInitFunc(bool isdone, float progress, string error)
        {
            if (isdone)
            {
                Instance._isInitDone = true;
                Instance._isInitializing = false;
                Instance._initProgress = 1;
                Instance._initError = error;

                if (Instance._OnInit != null)
                    Instance._OnInit(true, 1, error);

                if (string.IsNullOrEmpty(Instance._initError))
                {
                    if (Instance._OnInitializedSuccessfully != null)
                        Instance._OnInitializedSuccessfully();
                }
                else
                {
                    if (Instance._OnInitializationFailed != null)
                        Instance._OnInitializationFailed(Instance._initError);
                }

                return;
            }

            if (Instance._OnInit != null)
                Instance._OnInit(false, progress, null);
            if (Instance._OnInitializing != null)
                Instance._OnInitializing(progress);
        }
        bool _isInitializing;
        bool _isInitDone;
        float _initProgress;
        string _initError = null;
        /// <summary>
        /// 正在初始化
        /// <para>参数为初始化情况回调</para>
        /// <para>回调参数分别为：初始化进度</para>
        /// </summary>
        /// <param name="initializingAction">初始化情况回调</param>
        /// <returns>自身实例对象</returns>
        public ResourcesManager OnInitializing(System.Action<float> initializingAction)
        {
            OnInitializingEvent += initializingAction;
            return Instance;
        }
        /// <summary>
        /// 注册一个初始化成功时的回调
        /// <para>当初始化成功时，会回调<paramref name="initializedSuccessfully"/></para>
        /// </summary>
        public ResourcesManager OnInitializedSuccessfully(System.Action initializedSuccessfully)
        {
            OnInitializedSuccessfullyEvent += initializedSuccessfully;
            return Instance;
        }
        /// <summary>
        /// 初始化失败了
        /// <para>当初始化失败时，会回调<paramref name="initializationFailed"/></para>
        /// <para>回调<paramref name="initializationFailed"/> 中的参数为失败信息</para>
        /// </summary>
        public ResourcesManager OnInitializationFailed(System.Action<string> initializationFailed)
        {
            OnInitializationFailedEvent += initializationFailed;
            return Instance;
        }


        ResourcesLoadingLocation _resourcesLoadingLocation = ResourcesLoadingLocation.Resources;
        /// <summary>
        /// 获得资源加载位置
        /// </summary>
        public ResourcesLoadingLocation ResourcesLoadingLocation {
            get { return _resourcesLoadingLocation; }
            set { _resourcesLoadingLocation = value; }
        }
        static AssetBundleManifest _AssetBundleManifest;
        /// <summary>
        /// 资源清单
        /// </summary>
        public AssetBundleManifest ResourcePackage { get { return _AssetBundleManifest; } }

        IEnumerator loadResourcesList(System.Action<bool, float, string> action)
        {
            var path = GetResourceListFilePath();
            // 不是 StreamingAssetsPath 都可以使用System.IO.File读取资源清单
            if (ResourcesLoadingLocation != ResourcesLoadingLocation.StreamingAssetsPath)
            {
                string text = null;
                string error = null;
                float progress = 0f;
                try
                {
                    text = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
                    progress = 0.25f;
                    action(false, progress, null);
                }
                catch (System.Exception e)
                {
                    error = KSwordKitName + ": " + e.Message;
                }

                if (string.IsNullOrEmpty(error))
                {
                    yield return startThreadHandleResourcePackage(text, (isdone, _progress, _error) => {
                        if (isdone)
                        {
                            action(true, 1, _error);
                            return;
                        }
                        action(false, 0.25f + 0.75f * _progress, null);
                    });
                }
                else
                {
                    action(false, 1, null);
                    yield return null;

                    action(true, 1, error);
                }
            }
            else
            {
                if (IsNeedAddLocalFilePathPrefix())
                    path = "file://" + path;

                yield return loadResourcesList(path, action);
            }
        }
        IEnumerator loadResourcesList(string path, System.Action<bool, float, string> action)
        {
            var www = UnityEngine.Networking.UnityWebRequest.Get(path);
            var op = www.SendWebRequest();
            while(!op.isDone)
            {
                action(false, 0.5f * op.progress, null);
                yield return null;
            }
            
            if(string.IsNullOrEmpty(www.error))
            {
                var text = www.downloadHandler.text;
                yield return startThreadHandleResourcePackage(text, (isdone, progress, error) => {
                    if (isdone)
                    {
                        action(true, 1, error);
                        return;
                    }
                    action(false, 0.5f + 0.5f * progress, null);
                });
            }
            else
                action(true, 1, KSwordKitName + ": " + www.error);
        }
        IEnumerator loadResourcesList(UnityEngine.Networking.UnityWebRequest www, System.Action<bool, float, string> action)
        {
            var op = www.SendWebRequest();
            while (!op.isDone)
            {
                action(false, 0.5f * op.progress, null);
                yield return null;
            }

            if (string.IsNullOrEmpty(www.error))
            {
                var text = www.downloadHandler.text;
                yield return startThreadHandleResourcePackage(text, (isdone, progress, error) => {
                    if (isdone)
                    {
                        action(true, 1, error);
                        return;
                    }
                    action(false, 0.5f + 0.5f * progress, null);
                });
            }
            else
            {
                action(false, 1, null);
                NextFrame(() => action(true, 1, KSwordKitName + ": " + www.error));
            }
        }
        IEnumerator startThreadHandleResourcePackage(string jsonContent, System.Action<bool, float, string> action)
        {
            var text = jsonContent;
            bool isdone = false;
            var progress = 0f;
            string error = null;

            var _Thread = new System.Threading.Thread(() =>
            {
                try
                {
                    var r = JsonUtility.FromJson<AssetBundleManifest>(text);
                    if (r != null)
                    {
                        _AssetBundleManifest = r;
                        progress = 0.5f;
                    }
                    else
                    {
                        progress = 1;
                        error = KSwordKitName + ": 资源清单解析失败！请检查资源清单 json 格式是否正确。或者可以重新生成资源包。（点击：KSwordKit/资源管理/生成资源包）";
                        isdone = true;
                    }
                }
                catch (System.Exception e)
                {
                    error = KSwordKitName + ": " + e.Message;
                    progress = 1;
                    isdone = true;
                }

                if (Instance.ResourcePackage != null)
                {
                    var dc = Instance.ResourcePackage.AssetBundleInfos.Count;
                    float cc = 0;
                    Instance.ResourcePackage.AssetBundleInfos.ForEach((rm) =>
                    {
                        Instance.AssetbundleName_AssetBundlePathDic[rm.AssetBundleName] = rm;
                        rm.ResourceObjects.ForEach((ro) =>
                        {
                            Instance.ResourceObjectPath_ResourceObjectDic[ro.ResourcePath] = ro;
                        });
                        cc++;
                        progress = 0.5f + 0.5f * cc / dc;
                    });
                    isdone = true;
                }
            });
            _Thread.Start();

            var p = 0f;
            while (!isdone)
            {
                p = progress;
                action(false, progress, null);
                yield return null;
            }
            if (p != 1)
            {
                action(false, 1, null);
                yield return null;
            }
            action(true, 1, error);
        }
        /// <summary>
        /// 获取资源清单路径
        /// <para>在资源位置是<see cref="ResourcesLoadingLocation.RemotePath"/>或者<see cref="ResourcesLoadingLocation.PersistentDataPath"/> 都返回由 <see cref="Application.persistentDataPath"/>拼接而成路径</para>
        /// <para>在资源位置是<see cref="ResourcesLoadingLocation.StreamingAssetsPath"/>时，则返回由 <see cref="Application.streamingAssetsPath"/> 拼接而成的路径</para>
        /// <para>在资源位置是<see cref="ResourcesLoadingLocation.Resources"/>时，则默认返回由 资源包生成的位置 拼接而成的路径</para>
        /// </summary>
        /// <returns>资源清单路径</returns>
        public string GetResourceListFilePath()
        {
            return System.IO.Path.Combine(GetResourcesFileRootDirectory(), ResourcesFileName);
        }
        public bool IsNeedAddLocalFilePathPrefix(){
            return (!Application.isEditor && Application.platform == RuntimePlatform.IPhonePlayer) || 
                Application.platform == RuntimePlatform.OSXEditor;
        }
        /// <summary>
        /// 获得资源路径根目录
        /// </summary>
        /// <returns>资源根目录</returns>
        public string GetResourcesFileRootDirectory()
        {
            var path = AssetBundles;
#if UNITY_EDITOR
            path = System.IO.Path.Combine(path, 
            UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString());
#endif
#if !UNITY_EDITOR
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                path = System.IO.Path.Combine(path, "iOS");
            else
                path = System.IO.Path.Combine(path, Application.platform.ToString());
#endif
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
        public ResourcesManager LoadAssetAsync(string assetPath, System.Action<bool, float, string, UnityEngine.Object> asyncAction)
        {
            return LoadAssetAsync<UnityEngine.Object>(assetPath, asyncAction);
        }
        /// <summary>
        /// 异步加载资源
        /// <para>即便 ResourcesLoadingLocation == ResourcesLoadingLocation.Resources, 参数 <paramref name="assetPath"/> 也需要输入资源在项目中的相对路径</para>
        /// <para>路径开头 'Assets' 可省略，如 'Assets/Resources/test.png' 可简写为 'Resources/test.png'</para>
        /// <para>路径分隔符统一为 '/';当资源存在依赖时，将会自动加载依赖。</para>
        /// </summary>
        /// <param name="assetPath">资产路径</param>
        /// <param name="asyncAction">加载过程中的回调信息</param>
        /// <returns>资源管理器自身实例对象</returns>
        public ResourcesManager LoadAssetAsync<T>(string assetPath, System.Action<bool, float, string, T> asyncAction) where T: UnityEngine.Object
        {
            ResourcesAsyncLoader<T>.ResourcePackage = ResourcePackage;
            ResourcesAsyncLoader<T>.LoadAsync(assetPath, _resourcesLoadingLocation, (isdone, progress, error, obj) => {
                if (asyncAction != null)
                    asyncAction(isdone, progress, error, obj);
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
        public ResourcesManager LoadAssetAsync(string[] assetPaths, System.Action<bool, float, string, UnityEngine.Object[]> asyncAction)
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
        public ResourcesManager LoadAssetAsync<T>(string[] assetPaths, System.Action<bool, float, string, T[]> asyncAction) where T : UnityEngine.Object
        {
            ResourcesAsyncLoader<T>.ResourcePackage = ResourcePackage;
            ResourcesAsyncLoader<T>.LoadAsync(assetPaths, _resourcesLoadingLocation, (isdone, progress, error, objs) => {
                if (asyncAction != null)
                    asyncAction(isdone, progress, error, objs);
            });
            return this;
        }
        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="scenePath">场景资源路径</param>
        /// <param name="asyncAction">异步数据回调</param>
        /// <returns></returns>
        public ResourcesManager LoadSceneAsync(string scenePath, Func<string, AsyncOperation> scnenAsyncRequestFunc, System.Action<bool, float, string, SceneInfo> asyncAction)
        {
            ResourcesAsyncLoader<SceneInfo>.ResourcePackage = ResourcePackage;
            ResourcesAsyncLoader<SceneInfo>.LoadSceneAsync(scenePath, _resourcesLoadingLocation, scnenAsyncRequestFunc, (isdone, progress, error, obj) => {
                if (asyncAction != null)
                    asyncAction(isdone, progress, error, obj);
            });
            return this;
        }

        public static void NextFrame(System.Action action)
        {
            Instance.StartCoroutine(_ThreadWaitForNextFrame(action));
        }
        static IEnumerator _ThreadWaitForNextFrame(System.Action action)
        {
            yield return null;
            action();
        }
        public void WaitWhile(Func<bool> conditionFunc, System.Action action)
        {
            if (conditionFunc())
                NextFrame(() =>
                {
                    WaitWhile(conditionFunc, action);
                });
            else
                action();
        }
    }
}
