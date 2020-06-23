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
using System.Runtime.CompilerServices;
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
                if(_Instance == null)
                    _Instance = new GameObject(NAME).AddComponent<ResourcesManagement>();
                return _Instance;
            }
        }

        // 将资产路径转换为 AssetBundle 名称
        public static string ConvertAssetPathToAssetBundleName(string assetPasth)
        {
            if(assetPasth.StartsWith(Application.dataPath))
                assetPasth = assetPasth.Substring(Application.dataPath.Length+1);
            else if(assetPasth.StartsWith("Assets/"))
                assetPasth = assetPasth.Substring(7);
            else if(assetPasth.StartsWith("/Assets/"))
                assetPasth = assetPasth.Substring(8);

            assetPasth = assetPasth.Replace('\\', '/');
            assetPasth = assetPasth.Replace("/", "__");
            assetPasth = assetPasth.Replace('.', '_');
            return assetPasth.ToLower();
        }
 
        public static string ResourcePath
        {
            get
            {
                var dir = System.IO.Path.Combine(Application.streamingAssetsPath, AssetBundles);
                if(Application.isEditor)
                {
                    if(!System.IO.Directory.Exists(dir))
                        return string.Empty;
                    else
                    {
                        var dirs = new System.IO.DirectoryInfo(dir).GetDirectories();
                        if(dirs.Length == 0)
                            return string.Empty;
                        return dirs[0].FullName;
                    }
                }
                else
                {
                    dir = System.IO.Path.Combine(dir, Application.platform.ToString());
                    if(System.IO.Directory.Exists(dir))
                        return dir;
                    else
                        return string.Empty;
                }
            }
        } 

        static object _asyncLock = new object();
        static Dictionary<string, string[]> _resourceDic = null;
        static Dictionary<string,string[]> resourceDic
        {
            get
            {
                if(_resourceDic == null)
                {
                    var resourceListPath = System.IO.Path.Combine(ResourcePath, ResourcesFileName);
                    if(!System.IO.File.Exists(resourceListPath))
                        return null;

                    _resourceDic=new Dictionary<string, string[]>();
                    var lines = System.IO.File.ReadAllLines(resourceListPath);
                    foreach(var line in lines)
                    {
                        var items = line.Split(',');
                        if(items.Length != 3)
                            continue;
                        _resourceDic[items[0]] = items;
                    }
                }
                return _resourceDic;
            }
        }

        static Dictionary<string, GameObject> abGODic = new Dictionary<string, GameObject>();
        static Dictionary<string, UnityEngine.Object> objectDic = new Dictionary<string, UnityEngine.Object>(); 
        
        public static void LoadAssetAsync<T>(
            string assetPath, 
            System.Action<T> resultAction, 
            System.Action<string> errorAction = null, 
            System.Action<float> progressAction = null) 
            where T: UnityEngine.Object
        {
            Instance.StartCoroutine(_LoadAssetAsync<T>(
                assetPath, resultAction, 
                errorAction, 
                progressAction)
            );
        }
        static IEnumerator _LoadAssetAsync<T>(
            string assetPath, System.Action<T> resultAction, 
            System.Action<string> errorAction = null, 
            System.Action<float> progressAction = null)
            where T : UnityEngine.Object
        {
            if(objectDic.ContainsKey(assetPath))
            {
                if(progressAction != null)
                {
                    progressAction(1);
                    yield return null;
                }
                if(resultAction != null)
                {
                    resultAction(objectDic[assetPath] as T);
                }
                yield break;
            }

            // 检查资源清单是否存在
            if(resourceDic == null)
            {
                if(progressAction != null)
                {
                    progressAction(1);
                    yield return null;
                }

                if(errorAction != null)
                {
                    errorAction(KSwordKitName + ": 资源清单不存在！");
                    yield break;
                }
            }

            // 遍历资源清单
            var _assetPath = assetPath.ToLower();
            if(resourceDic.ContainsKey(_assetPath))
            {
                var abname = resourceDic[_assetPath.ToLower()][1];
                if(abGODic.ContainsKey(abname))
                {
                    if(abGODic[abname] != null)
                    {
                        var c = abGODic[abname].GetComponent<ResourceRequestAsync>();
                        if(c != null)
                        {   
                            var abtype = typeof(T) == typeof(AssetBundle);
                            while(!c.isDone && c.AssetBundle == null)
                            {
                                if(progressAction!=null)
                                    progressAction(c.Progress * (abtype ? 1: 0.5f));
                                yield return null;
                            }
                            if(!string.IsNullOrEmpty(c.Error))
                            {
                                if(c.AssetBundle == null)
                                {
                                    if(errorAction!=null)
                                        errorAction(c.Error);
                                    yield break;
                                }
                            }
                            if(abtype)
                            {
                                if(!objectDic.ContainsKey(assetPath))
                                {
                                    objectDic[assetPath] = c.Asset;
                                }

                                if(resultAction !=null)
                                    resultAction(c.Asset as T);
                                yield break;
                            }

                            c = ResourceRequestAsync.New(c, assetPath);
                            c.ResultEvent += (r)=>
                            {
                                if(!objectDic.ContainsKey(assetPath))
                                {
                                    objectDic[assetPath] = r.Asset;
                                }
                                Destroy(c);
                                if(resultAction!=null)
                                    resultAction(r.Asset as T);
                            };
                            c.ErrorEvent += (r) =>
                            {
                                Destroy(c);
                                if(errorAction != null)
                                    errorAction(r.Error);
                            };
                            c.ProgressEvent += (r) =>
                            {
                                if(progressAction != null)
                                    progressAction(r.Progress);
                            };

                            yield return c.LoadAssetAsync<T>(c.AssetBundle, System.IO.Path.GetFileNameWithoutExtension(assetPath));

                            yield break;
                        }
                        else
                        {
                            if(errorAction != null)
                            {
                                errorAction(KSwordKitName+": 查询值为 null！\npath = " + assetPath + "\nabname = "+abname);
                            }
                            yield break;
                        }
                    }
                    else
                    {
                        if(errorAction != null)
                        {
                            errorAction(KSwordKitName+": 查询值为 null！\npath = " + assetPath + "\nabname = "+abname);
                        }
                        yield break;
                    }
                }

                var url = System.IO.Path.Combine(ResourcePath, abname);
                if(Application.platform == RuntimePlatform.OSXEditor)
                    url = "file://" + url;
                var rra = ResourceRequestAsync.New(assetPath, abname, url);
                rra.ResultEvent += (r)=>{
                    if(!objectDic.ContainsKey(r.AssetPath))
                    {
                        objectDic[r.AssetPath] = r.Asset;
                    }

                    if(resultAction!=null)
                        resultAction(r.Asset as T);
                };
                rra.ErrorEvent += (r) =>{

                    if(r.AssetBundle == null)
                    {
                        if(abGODic.ContainsKey(r.AssetBundleName))
                        {
                            abGODic.Remove(r.AssetBundleName);
                            Destroy(r.gameObject);
                        }
                    }

                    if(errorAction != null)
                        errorAction(r.Error);
                };
                rra.ProgressEvent += (r) =>{
                    if(progressAction != null)
                        progressAction(r.Progress);
                };
                abGODic[abname] = rra.gameObject;
                rra.Send<T>();
            }
            else
            {                                    
                if(errorAction != null)
                {
                    errorAction(KSwordKitName +": 资源不存在! path = " + assetPath);
                }
            }
        }
    

        public static void LoadAssetAsync(
            string assetPath, 
            System.Action<UnityEngine.Object> resultAction, 
            System.Action<string> errorAction = null, 
            System.Action<float> progressAction = null) 
        {
            Instance.StartCoroutine(_LoadAssetAsync<UnityEngine.Object>(
                assetPath, resultAction, 
                errorAction, 
                progressAction)
            );
        }
    
        public static void LoadAssetAsync<T>(
            string[] assetPaths,
            System.Action<T[]> resultAction, 
            System.Action<string> errorAction = null, 
            System.Action<float> progressAction = null)
            where T : UnityEngine.Object
        {
            var total = assetPaths.Length;
            var num = 0;
            var rlist = new List<T>();
            var error = string.Empty;

            System.Action completed = () =>{
                if(num == total)
                {
                    if(progressAction != null)
                        progressAction(1);
                    
                    Instance.StartCoroutine(_nextFrame(()=>{
                        if(resultAction !=null && rlist.Count > 0)
                            resultAction(rlist.ToArray());
                        if(errorAction!= null && !string.IsNullOrEmpty(error))
                            errorAction(error);
                    }));
                }
            };

            foreach(var path in assetPaths)
            {
                Instance.StartCoroutine(
                    _LoadAssetAsync<T>(
                        path, (r)=>{
                            rlist.Add(r);
                            num++;
                            completed();
                        }, 
                        (_error)=>{
                            error += _error+"\n";
                            num++;
                            completed();
                        }, 
                        (progress)=>{
                            if(progressAction != null)
                                progressAction(progress);
                        }
                    )
                );
            }
        }
    
        static IEnumerator _nextFrame(System.Action action)
        {
            yield return new WaitForEndOfFrame();
            action();
        }
    
        public static void LoadAssetAsync(
                        string[] assetPaths,
            System.Action<UnityEngine.Object[]> resultAction, 
            System.Action<string> errorAction = null, 
            System.Action<float> progressAction = null)
        {
            var total = assetPaths.Length;
            var num = 0;
            var rlist = new List<UnityEngine.Object>();
            var error = string.Empty;

            System.Action completed = () =>{
                if(num == total)
                {
                    if(progressAction != null)
                        progressAction(1);
                    
                    Instance.StartCoroutine(_nextFrame(()=>{
                        if(resultAction !=null && rlist.Count > 0)
                            resultAction(rlist.ToArray());
                        if(errorAction!= null && !string.IsNullOrEmpty(error))
                            errorAction(error);
                    }));
                }
            };

            foreach(var path in assetPaths)
            {
                Instance.StartCoroutine(
                    _LoadAssetAsync<UnityEngine.Object>(
                        path, (r)=>{
                            rlist.Add(r);
                            num++;
                            completed();
                        }, 
                        (_error)=>{
                            error += _error+"\n";
                            num++;
                            completed();
                        }, 
                        (progress)=>{
                            if(progressAction != null)
                                progressAction(progress);
                        }
                    )
                );
            }
        }
    

        public static void LoadAssetAsync(string assetPath, System.Action<ResourceRequestAsync> updateAction)
        {
            var rrAsync = ResourceRequestAsync.New(assetPath);
            Instance.StartCoroutine(
                _LoadAssetAsync(
                    assetPath,
                    updateAction
                )
            );
        }

        static IEnumerator _LoadAssetAsync<T>(string assetPath, System.Action<ResourceRequestAsync> updateAction) where T:UnityEngine.Object
        {
            var rra = ResourceRequestAsync.New(assetPath);
           
                        
            // if(objectDic.ContainsKey(assetPath))
            // {
            //     rra.Asset
            //     if(updateAction != null)
            //         updateAction(objectDic[assetPath] as T);
            //     yield break;
            // }

            // // 检查资源清单是否存在
            // if(resourceDic == null)
            // {
            //     if(progressAction != null)
            //     {
            //         progressAction(1);
            //         yield return null;
            //     }

            //     if(errorAction != null)
            //     {
            //         errorAction(KSwordKitName + ": 资源清单不存在！");
            //         yield break;
            //     }
            // }

            // // 遍历资源清单
            // var _assetPath = assetPath.ToLower();
            // if(resourceDic.ContainsKey(_assetPath))
            // {
            //     var abname = resourceDic[_assetPath.ToLower()][1];
            //     if(abGODic.ContainsKey(abname))
            //     {
            //         if(abGODic[abname] != null)
            //         {
            //             var c = abGODic[abname].GetComponent<ResourceRequestAsync>();
            //             if(c != null)
            //             {   
            //                 var abtype = typeof(T) == typeof(AssetBundle);
            //                 while(!c.isDone && c.AssetBundle == null)
            //                 {
            //                     if(progressAction!=null)
            //                         progressAction(c.Progress * (abtype ? 1: 0.5f));
            //                     yield return null;
            //                 }
            //                 if(!string.IsNullOrEmpty(c.Error))
            //                 {
            //                     if(c.AssetBundle == null)
            //                     {
            //                         if(errorAction!=null)
            //                             errorAction(c.Error);
            //                         yield break;
            //                     }
            //                 }
            //                 if(abtype)
            //                 {
            //                     if(!objectDic.ContainsKey(assetPath))
            //                     {
            //                         objectDic[assetPath] = c.Asset;
            //                     }

            //                     if(resultAction !=null)
            //                         resultAction(c.Asset as T);
            //                     yield break;
            //                 }

            //                 c = ResourceRequestAsync.New(c, assetPath);
            //                 c.ResultEvent += (r)=>
            //                 {
            //                     if(!objectDic.ContainsKey(assetPath))
            //                     {
            //                         objectDic[assetPath] = r.Asset;
            //                     }
            //                     Destroy(c);
            //                     if(resultAction!=null)
            //                         resultAction(r.Asset as T);
            //                 };
            //                 c.ErrorEvent += (r) =>
            //                 {
            //                     Destroy(c);
            //                     if(errorAction != null)
            //                         errorAction(r.Error);
            //                 };
            //                 c.ProgressEvent += (r) =>
            //                 {
            //                     if(progressAction != null)
            //                         progressAction(r.Progress);
            //                 };

            //                 yield return c.LoadAssetAsync<T>(c.AssetBundle, System.IO.Path.GetFileNameWithoutExtension(assetPath));

            //                 yield break;
            //             }
            //             else
            //             {
            //                 if(errorAction != null)
            //                 {
            //                     errorAction(KSwordKitName+": 查询值为 null！\npath = " + assetPath + "\nabname = "+abname);
            //                 }
            //                 yield break;
            //             }
            //         }
            //         else
            //         {
            //             if(errorAction != null)
            //             {
            //                 errorAction(KSwordKitName+": 查询值为 null！\npath = " + assetPath + "\nabname = "+abname);
            //             }
            //             yield break;
            //         }
            //     }

            //     var url = System.IO.Path.Combine(ResourcePath, abname);
            //     if(Application.platform == RuntimePlatform.OSXEditor)
            //         url = "file://" + url;
            //     var rra = ResourceRequestAsync.New(assetPath, abname, url);
            //     rra.ResultEvent += (r)=>{
            //         if(!objectDic.ContainsKey(r.AssetPath))
            //         {
            //             objectDic[r.AssetPath] = r.Asset;
            //         }

            //         if(resultAction!=null)
            //             resultAction(r.Asset as T);
            //     };
            //     rra.ErrorEvent += (r) =>{

            //         if(r.AssetBundle == null)
            //         {
            //             if(abGODic.ContainsKey(r.AssetBundleName))
            //             {
            //                 abGODic.Remove(r.AssetBundleName);
            //                 Destroy(r.gameObject);
            //             }
            //         }

            //         if(errorAction != null)
            //             errorAction(r.Error);
            //     };
            //     rra.ProgressEvent += (r) =>{
            //         if(progressAction != null)
            //             progressAction(r.Progress);
            //     };
            //     abGODic[abname] = rra.gameObject;
            //     rra.Send<T>();
            // }
            // else
            // {                                    
            //     if(errorAction != null)
            //     {
            //         errorAction(KSwordKitName +": 资源不存在! path = " + assetPath);
            //     }
            // }

            yield break;
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
                Instance.StartCoroutine(loadResourcesList(path, action));
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
            var path = AssetBundles;
#if UNITY_EDITOR
            path = System.IO.Path.Combine(path, UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString());
#endif
            if (!Application.isEditor)
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                    path = System.IO.Path.Combine(path, "iOS");
                else
                    path = System.IO.Path.Combine(path, Application.platform.ToString());
            }
            path = System.IO.Path.Combine(path, ResourcesFileName);
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
            if ((!Application.isEditor && Application.platform == RuntimePlatform.IPhonePlayer) || Application.platform == RuntimePlatform.OSXEditor)
            {
                path = "file://" + path;
            }
            return path;
        }

        ResourcesAsyncLoader _ResourcesAsyncLoader = null;
        /// <summary>
        /// 异步加载资源
        /// <para>即便 ResourcesLoadingLocation == ResourcesLoadingLocation.Resources 也需要输入资源在项目中的相对路径</para>
        /// <para>路径开头 'Assets' 可省略，如 'Assets/Resources/test.png' 可简写为 'Resources/test.png'</para>
        /// </summary>
        /// <param name="assetPath">资产路径</param>
        /// <param name="asyncAction">加载过程信息回调</param>
        /// <returns>资源管理器自身实例对象</returns>
        public ResourcesManagement LoadAssetAsync(string assetPath, System.Action<ResourcesManagement, ResourcesRequestAsyncOperation> asyncAction)
        {


            if (_ResourcesAsyncLoader == null)
                _ResourcesAsyncLoader = ResourcesAsyncLoader.New(_resourcesLoadingLocation);
           
            if(_resourcesLoadingLocation == ResourcesLoadingLocation.Resources)
            {
                var index = assetPath.IndexOf("Resources/");
            }

            _ResourcesAsyncLoader.LoadAsync(assetPath, (rrao)=> {
                if (asyncAction != null)
                    asyncAction(this, rrao);
            });
            return this;
        }
        
    }
}
