/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ResourcesAsyncLoader.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-11
 *  File Description: Ignore.
 *************************************************************************/
using Frankfort.Threading.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;
using UnityEngine.Networking;

namespace KSwordKit.Core.ResourcesManagement
{
	/// <summary>
	/// 资源异步加载器
	/// </summary>
    public class ResourcesAsyncLoader<T> where T : UnityEngine.Object
	{
		static ResourcesAsyncLoader()
        {
			if (_loader == null)
			{
                var o = ResourcesManagement.Instance.gameObject;
                _loader = o.GetComponent<ResourcesManagement>() as MonoBehaviour;
            }
			_Type = typeof(T);
            _TypeIsSprite = _Type == typeof(Sprite);
            _TypeIsTexture2D = _Type == typeof(Texture2D);
        }
        static bool _TypeIsSprite;
        static bool _TypeIsTexture2D;
        static System.Type _Type;
		private static MonoBehaviour _loader;

		static Dictionary<string, T> _CacheDic = new Dictionary<string, T>();
		/// <summary>
		/// 缓存字典
		/// </summary>
		public static Dictionary<string, T> CacheDic { get { return _CacheDic; } }
		static Dictionary<string, AssetBundle> _CacheAssetBunbleDic = new Dictionary<string, AssetBundle>();
		/// <summary>
		/// AssetBundle 缓冲字典
		/// </summary>
		public static Dictionary<string, AssetBundle> CacheAssetBunbleDic { get { return _CacheAssetBunbleDic; } }

		//static Dictionary<string, LoadingAssetBundle> _CacheLoadingAssetBundleDic = new Dictionary<string, LoadingAssetBundle>();
		///// <summary>
		///// 正在加载 AssetBundle 的缓冲字典
		///// </summary>
		//public static Dictionary<string, LoadingAssetBundle> CacheLoadingAssetBundleDic { get { return _CacheLoadingAssetBundleDic; } }
  //      static Dictionary<string, LoadingAssetBundle> _CacheLoadingAssetBundleRequestDic = new Dictionary<string, LoadingAssetBundle>();
  //      /// <summary>
  //      /// 正在加载 AssetBundleRequest 的缓冲字典
  //      /// </summary>
  //      public static Dictionary<string, LoadingAssetBundle> CacheLoadingAssetBundleRequestDic { get { return _CacheLoadingAssetBundleRequestDic; } }
        private static int _timeoutIfCanBeApplied;
		/// <summary>
		/// 如果能被应用的话，设置或获取超时时间
		/// <para>当 ResourcesLoadingLocation == ResourcesLoadingLocation.Resources 时，超时时间不能被应用在异步加载操作中。</para>
		/// </summary>
		public static int timeoutIfCanBeApplied 
		{
			get { return _timeoutIfCanBeApplied; } 
			set { _timeoutIfCanBeApplied = value; }
		}

		static AssetBundleManifest _ResourcePackage;
		/// <summary>
		/// 资源包信息
		/// <para>包含所有资源信息，加载资源时，该值必须存在。</para>
		/// </summary>
		public static AssetBundleManifest ResourcePackage
		{
            get { return _ResourcePackage; }
			set { _ResourcePackage = value; }
		}

		/// <summary>
		/// 根据资源路径 <paramref name="assetPath"/> 异步加载资源
		/// <para>异步加载总比同步的方法慢一帧</para>
		/// <para>参数 asyncAction 是加载资源的异步回调，回调参数含义为 isDone, progress, error, asset</para>
		/// </summary>
		/// <param name="assetPath">资源路径</param>
		/// <param name="resourcesLoadingLocation">资源加载位置</param>
		/// <param name="asyncAction">异步回调；参数是异步结果，内部包含进度、错误信息、加载结果等内容。</param>
		public static void LoadAsync(
			string assetPath, 
			ResourcesLoadingLocation resourcesLoadingLocation, 
			System.Action<bool, float, string, T> asyncAction)
        {
            // 总共需要3步
            // 1. 初始化资源包，只需执行一次
            // 2. 加载资源包
            // 3. 从资源包中加载资源


            // 资源包对象须存在
            if (_ResourcePackage == null)
            {
                asyncAction(true, 1, ResourcesManagement.KSwordKitName + ": 请先调用 SetResourcePackage 方法设置资源包", null);
                return;
            }


            // 检查缓存内容, 缓存中存在时，加载缓存中的资源
            if (CacheDic.ContainsKey(assetPath))
            {
				if (asyncAction != null)
                {
					asyncAction(false, 1, null, null);
					NextFrame(() => asyncAction(true, 1, null, CacheDic[assetPath]));
				}

				return;
            }

			// 根据资源加载位置的不同采取不同的加载策略
			switch(resourcesLoadingLocation)
			{                   
				// 使用 Resources.LoadAsync
				case ResourcesLoadingLocation.Resources:
                    _loadResourcesByResources(assetPath, asyncAction);
                    break;
				// 其他路径下都使用同样的办法加载资源
				// 加载器只处理本地资源
				// 当使用远程资源时，资源管理器会在需要更新的时候，先更新远程资源到本地，进而保证加载器加载的永远是最新的资源包。
				case ResourcesLoadingLocation.StreamingAssetsPath:
				case ResourcesLoadingLocation.PersistentDataPath:
				case ResourcesLoadingLocation.RemotePath:

                    if (ResourcesManagement.Instance.ResourceObjectPath_ResourceObjectDic.ContainsKey(assetPath))
                    {
                        var ro = ResourcesManagement.Instance.ResourceObjectPath_ResourceObjectDic[assetPath];
                        var rm = ResourcesManagement.Instance.AssetbundleName_AssetBundlePathDic[ro.AssetBundleName];
                        // 加载资源
                        System.Action abloadedAction = () => {
                            float _progress = 0;
                            _loader.StartCoroutine(ro.AsyncLoad(assetPath, rm.AssetBundle, (isdone, progress, error, obj) => {
                                
                                if (isdone)
                                {
                                    System.Action action = () => {
                                        string asyncLoadAbr_error = null;
                                        try
                                        {
                                            T t = null;
                                            if (_TypeIsSprite)
                                            {
                                                var t2d = obj as Texture2D;
                                                t = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero) as T;
                                            }
                                            else
                                                t = obj as T;

                                            if (t != null)
                                            {
                                                // 资源加载成功后，存入资源缓存中
                                                CacheDic[assetPath] = t;
                                                asyncAction(true, 1, null, t);
                                            }
                                            else
                                            {
                                                asyncLoadAbr_error = ResourcesManagement.KSwordKitName + ": 资源加载成功，但该资源无法转换为 " + _Type.FullName + " 类型。\nassetPath="+assetPath;
                                                asyncAction(true, 1, asyncLoadAbr_error, null);
                                            }
                                        }
                                        catch (System.Exception e)
                                        {
                                            asyncLoadAbr_error = ResourcesManagement.KSwordKitName + ": 资源加载成功，但该资源无法转换为 " + _Type.FullName + " 类型, " + e.Message + "\nassetPath=" + assetPath;;
                                            asyncAction(true, 1, asyncLoadAbr_error, null);
                                        }
                                    };
                                    if (_progress != 1)
                                    {
                                        if (asyncAction != null)
                                            asyncAction(false, 1, error, null);
                                        NextFrame(action);
                                    }
                                    else
                                        action();

                                    return;
                                }

                                _progress = 2f / 3f + 1f / 3f * progress;
                                if (asyncAction != null && _progress > 2f/3f)
                                    asyncAction(false, _progress, error, null);
                            }));
                        };

                        if (rm.AssetBundle == null)
                        {
                            // 加载资源包
                            rm.AsyncLoadTimeout = timeoutIfCanBeApplied;
                            _loader.StartCoroutine(rm.AsyncLoad((isdone, progress, error, obj) =>
                            {
                                if (isdone)
                                {
                                    if (string.IsNullOrEmpty(error))
                                    {
                                        if (asyncAction != null)
                                            asyncAction(false, 2f / 3f, null, null);
                                        abloadedAction();
                                    }
                                    else if (asyncAction != null)
                                        asyncAction(false, 1, error, null);

                                    return;
                                }
                                if (asyncAction != null)
                                    asyncAction(false, 1f / 3f + 1f / 3f * progress, error, null);
                            }));
                        }
                        else
                            abloadedAction();
                    }
                    else
                    {
                        if (asyncAction != null)
                        {
                            asyncAction(false, 1, null, null);
                            NextFrame(() => asyncAction(true, 1, ResourcesManagement.KSwordKitName + ": 资源不存在！请检查参数 assetPath 是否正确，assetPath=" + assetPath, null));
                        }
                    }

					break;
			}
		}

        static void _loadResourcesByResources(string assetPath, System.Action<bool, float, string, T> asyncAction)
        {
            // 检查路径
            var str = "Resources/";
            var index = assetPath.IndexOf(str);
            str = assetPath.Substring(index + str.Length);
            str = str.Replace(System.IO.Path.GetExtension(str), "");
            _loader.StartCoroutine(AsyncByResources(assetPath, Resources.LoadAsync(str), asyncAction));
        }
        static IEnumerator AsyncByResources(string path, ResourceRequest resourceRequest, System.Action<bool, float, string, T> asyncAction)
        {
			while (!resourceRequest.isDone)
			{
				asyncAction(false, resourceRequest.progress, null, null);
				yield return null;
			}

			if (resourceRequest.progress != 1)
				asyncAction(false, 1, null, null);
			yield return null;

			if (resourceRequest.asset == null)
				asyncAction(true, 1, ResourcesManagement.KSwordKitName + ": 资源加载失败或者文件不存在! 请检查参数 assetPath 是否正确, assetPath=" + path, null);
            else
            {
                try
                {
                    T t = null;
                    if (_TypeIsSprite)
                    {
                        var t2d = resourceRequest.asset as Texture2D;
                        var s = Sprite.Create(t2d, new Rect(Vector2.zero, new Vector2(t2d.width, t2d.height)), Vector2.zero);
                        s.name = t2d.name;
                        t = s as T;
                    }
                    else
                       t = resourceRequest.asset as T;

                    if (t != null)
                    {
						// 资源加载成功后，存入资源缓存中
						CacheDic[path] = t;
						asyncAction(true, 1, null, t);
					}
					else
						asyncAction(true, 1, ResourcesManagement.KSwordKitName + ": 资源加载成功，但该资源无法转换为 " + _Type.FullName + " 类型", null);
				}
				catch (System.Exception e)
                {
					asyncAction(true, 1, ResourcesManagement.KSwordKitName + ": 资源加载成功，但该资源无法转换为 " + _Type.FullName + " 类型, " + e.Message, null);
				}
			}
		}

  

        static void WaitWhile(Func<bool> waitFunc, System.Action completedAction)
        {
            if (waitFunc())
                completedAction();
            else
                NextFrame(() =>
                {
                    WaitWhile(waitFunc, completedAction);
                });
        }
        /// <summary>
        /// 根据资源路径数组 <paramref name="assetPaths"/> 异步加载一组资源
        /// <para>异步加载总比同步的方法慢一帧</para>
        /// <para>参数 <paramref name="asyncAction"/> 是加载资源的异步回调, 回调参数含义为 isDone, progress, error, assets </para>
        /// </summary>
        /// <param name="assetPaths">资源路径</param>
        /// <param name="resourcesLoadingLocation">资源加载位置</param>
        /// <param name="asyncAction">异步回调；参数是异步结果，内部包含进度、错误信息、加载结果等内容。 </param>
        public static void LoadAsync(string[] assetPaths, ResourcesLoadingLocation resourcesLoadingLocation, System.Action<bool, float, string, T[]> asyncAction)
        {
			int c = assetPaths.Length;
			int cc = 0;
			string error = null;
			var objs = new T[c];
            var _progress = 0f;
			for (var i = 0; i < c; i++)
            {
                var index = i;
				LoadAsync(assetPaths[i], resourcesLoadingLocation, (isdone, progress, _error, obj) => {

					if (isdone)
                    {
						cc++;
                        objs[index] = obj;
                        if (!string.IsNullOrEmpty(_error))
                        {
							if (string.IsNullOrEmpty(error))
								error = _error;
							else
								error += "\n" + _error;
                        }

                        if(c == cc)
                        {
                            if (_progress == 1f)
                                asyncAction(true, 1, error, objs);
                            else
                            {
                                asyncAction(false, 1, null, null);
                                NextFrame(() => {
                                    asyncAction(true, 1, error, objs);
                                });
                            }

                        }
                        return;
                    }
                    _progress = (float)cc / c + 1f / (float)c * progress;
                    asyncAction(false, _progress, null, null);
				});
			}
        }

		/// <summary>
		/// 给定的路径（文件夹或文件）中加载所有资源
		/// <para>如果想递归目录内所有资源可以使用<seealso cref="LoadAllAsync(string, bool, System.Action{ResourcesRequestAsyncOperation})"/></para>
		/// <para>参数 asyncAction 是加载资源的异步回调；查看<see cref="ResourcesRequestAsyncOperation"/></para>
		/// </summary>
		/// <param name="assetPath">给定的资源路径（文件夹或文件）</param>
		/// <param name="asyncAction">异步回调；参数是异步结果，内部包含进度、错误信息、加载结果等内容；</param>
		public void LoadAllAsync(string assetPath, System.Action<ResourcesRequestAsyncOperation> asyncAction)
        {

        }
		/// <summary>
		/// 给定的路径（文件夹或文件）中加载所有资源
		/// <para>参数 deepResources 指示函数是否递归遍历子目录内的资源，当值为false时，函数行为和<seealso cref="LoadAllAsync(string, System.Action{ResourcesRequestAsyncOperation})"/>相同</para>
		/// <para>参数 asyncAction 是加载资源的异步回调；查看<see cref="ResourcesRequestAsyncOperation"/></para>
		/// </summary>
		/// <param name="assetPath">给定的资源路径（文件夹或文件）</param>
		/// <param name="deepResources">指示函数是否遍历子目录所有资源</param>
		/// <param name="asyncAction">异步回调；参数是异步结果，内部包含进度、错误信息、加载结果等内容；</param>
		public void LoadAllAsync(string assetPath, bool deepResources, System.Action<ResourcesRequestAsyncOperation> asyncAction)
        {

        }

		static void NextFrame(System.Action action)
        {
			_loader.StartCoroutine(_ThreadWaitForNextFrame(action));
        }
		static IEnumerator _ThreadWaitForNextFrame(System.Action action)
        {
			yield return null;
			action();
        }
	}
}


