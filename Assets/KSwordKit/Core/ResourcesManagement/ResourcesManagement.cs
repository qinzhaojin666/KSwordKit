/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ResourcesManagement.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-7
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace KSwordKit.Core
{
    public class ResourcesManagement : MonoBehaviour
    {
        const string KitName = "KSwordKit";
        public const string NAME = "ResourcesManagement";
        /// <summary>
        /// 用于AssetBundle输出位置
        /// </summary>
        public const string AssetBundles = "AssetBundles";
        public const string ResourcesFileName = "resourcesfile.csv";

        static ResourcesManagement _instance;
        static ResourcesManagement instance
        {
            get
            {
                if(_instance == null)
                    _instance = new GameObject(NAME).AddComponent<ResourcesManagement>();
                return _instance;
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
        static Dictionary<string, UnityEngine.Object> objectDic = new Dictionary<string, Object>(); 
        
        public static void LoadAssetAsync<T>(
            string assetPath, 
            System.Action<T> resultAction, 
            System.Action<string> errorAction = null, 
            System.Action<float> progressAction = null) 
            where T: UnityEngine.Object
        {
            instance.StartCoroutine(_LoadAssetAsync<T>(
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
                    errorAction(KitName + ": 资源清单不存在！");
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
                                errorAction(KitName+": 查询值为 null！\npath = " + assetPath + "\nabname = "+abname);
                            }
                            yield break;
                        }
                    }
                    else
                    {
                        if(errorAction != null)
                        {
                            errorAction(KitName+": 查询值为 null！\npath = " + assetPath + "\nabname = "+abname);
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
                    errorAction(KitName +": 资源不存在! path = " + assetPath);
                }
            }
        }
    

        public static void LoadAssetAsync(
            string assetPath, 
            System.Action<UnityEngine.Object> resultAction, 
            System.Action<string> errorAction = null, 
            System.Action<float> progressAction = null) 
        {
            instance.StartCoroutine(_LoadAssetAsync<UnityEngine.Object>(
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
                    
                    instance.StartCoroutine(_nextFrame(()=>{
                        if(resultAction !=null && rlist.Count > 0)
                            resultAction(rlist.ToArray());
                        if(errorAction!= null && !string.IsNullOrEmpty(error))
                            errorAction(error);
                    }));
                }
            };

            foreach(var path in assetPaths)
            {
                instance.StartCoroutine(
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
                    
                    instance.StartCoroutine(_nextFrame(()=>{
                        if(resultAction !=null && rlist.Count > 0)
                            resultAction(rlist.ToArray());
                        if(errorAction!= null && !string.IsNullOrEmpty(error))
                            errorAction(error);
                    }));
                }
            };

            foreach(var path in assetPaths)
            {
                instance.StartCoroutine(
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
            instance.StartCoroutine(
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
            //         errorAction(KitName + ": 资源清单不存在！");
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
            //                     errorAction(KitName+": 查询值为 null！\npath = " + assetPath + "\nabname = "+abname);
            //                 }
            //                 yield break;
            //             }
            //         }
            //         else
            //         {
            //             if(errorAction != null)
            //             {
            //                 errorAction(KitName+": 查询值为 null！\npath = " + assetPath + "\nabname = "+abname);
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
            //         errorAction(KitName +": 资源不存在! path = " + assetPath);
            //     }
            // }

            yield break;
        }
    }
}
