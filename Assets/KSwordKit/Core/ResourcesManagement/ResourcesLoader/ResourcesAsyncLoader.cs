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
using UnityEngine;
using UnityEngine.Networking;

namespace KSwordKit.Core.ResourcesManagement
{
	/// <summary>
	/// 资源异步加载器
	/// </summary>
    public class ResourcesAsyncLoader<T> where T : UnityEngine.Object
	{
		public const string KSwordKitName = "KSwordKit";
		public const string ClassName = "ResourcesAsyncLoader";
		public const string AsyncLoader = "asyncloader";

		static ResourcesAsyncLoader()
        {
			if (_asyncLoaderParent == null)
			{
				_asyncLoaderParent = new GameObject(ClassName).transform;
				GameObject.DontDestroyOnLoad(_asyncLoaderParent);
				var o = new GameObject(AsyncLoader);
				_loader = o.AddComponent<LoaderMonoBehaviour>();
			}
			TypeFullName = typeof(T).FullName;
		}

		static string TypeFullName;
		private static Transform _asyncLoaderParent;
		private static LoaderMonoBehaviour _loader;

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
		static Dictionary<string, System.Action<bool, float, string>> _CacheLoadingAssetBundleDic = new Dictionary<string, System.Action<bool, float, string>>();
		/// <summary>
		/// 正在加载 AssetBundle 的缓冲字典
		/// </summary>
		public static Dictionary<string, System.Action<bool, float, string>> CacheLoadingAssetBundleDic { get { return _CacheLoadingAssetBundleDic; } }
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
			// 检查缓存内容, 缓存中存在时，加载缓存中的资源
			if(CacheDic.ContainsKey(assetPath))
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
					// 检查路径
					var str = "Resources/";
					var index = assetPath.IndexOf(str);
					str = assetPath.Substring(index + str.Length);
					str = str.Replace(System.IO.Path.GetExtension(str), "");
					_loader.StartCoroutine(Async(assetPath, Resources.LoadAsync(str), asyncAction));
					break;
				// 其他路径下都使用同样的办法加载资源
				// 加载器只处理本地资源
				// 当使用远程资源时，资源管理器会在需要更新的时候，先更新远程资源到本地，进而保证加载器加载的永远是最新的资源包。
				case ResourcesLoadingLocation.StreamingAssetsPath:
				case ResourcesLoadingLocation.PersistentDataPath:
				case ResourcesLoadingLocation.RemotePath:
					// 资源包对象须存在
					if(_ResourcePackage != null)
                    {
						// 从资源包中查找目标资源并开始加载
						var rm = _ResourcePackage.AssetBundleInfos.Find((rmanifest) => rmanifest.ResourceObjects.Find((ro) => 
						{
							// 找到目标资源
							if (ro.ResourcePath == assetPath)
							{
								// 资源包所在根目录
								var rootDir = System.IO.Path.Combine(ResourcesManagement.GetResourcesFileRootDirectory(), ResourcesManagement.ResourceRootDirectoryName);
								// 先加载依赖的资源包
								float c = rmanifest.Dependencies.Count;
								float cc = 0;
								string error = null;
								bool haveError = false;
								for(var i = 0; i < c; i++)
                                {
									var d = rmanifest.Dependencies[i];
									// 资源包缓存中存在时，加载下一个依赖项
									if(CacheAssetBunbleDic.ContainsKey(d))
                                    {
										cc++;
										// 正在加载依赖项...
										asyncAction(false, (1f / 3f) * (cc / c), null, null);
										continue;
                                    }
									// 加载依赖的资源包
									var www = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle("file://" + 
										System.IO.Path.Combine(rootDir, _ResourcePackage.AssetBundleInfos.Find((r) => r.AssetBundleName == d).AssetBundlePath)
										);
									www.timeout = timeoutIfCanBeApplied;
									_loader.StartCoroutine(AsyncLoadDependencies(d, www, (isdone, progress, _error) =>
									{
										// 加载依赖项时已经发生了错误时，不在继续处理
										if (haveError)
                                        {
											c++;
											return;
                                        }
										// 依赖加载完成
										if(isdone)
                                        {
                                            if (!string.IsNullOrEmpty(_error))
                                            {
												haveError = true;
												error = _error;
												return;
											}
											cc++;
											return;
										}
										// 正在加载依赖项...
										asyncAction(false, (1f / 3f) * (cc / c) * progress, null, null);
									}));
								}
								// 等待所有依赖加载完成之后
								// 加载目标资源所在的资源包
								WaitAsyncLoadDependencies(() => cc != c, ()=> {
									// 依赖加载过程中发生了错误时，返回错误信息。
									if(haveError)
                                    {
										asyncAction(true, 1, error, null);
										return;
                                    }
									// 缓存的资源包中存在的话，就从缓存中加载
									if(CacheAssetBunbleDic.ContainsKey(rmanifest.AssetBundleName))
                                    {
										_loader.StartCoroutine(Async(assetPath, CacheAssetBunbleDic[rmanifest.AssetBundleName].LoadAssetAsync(ro.ObjectName), asyncAction));
										return;
                                    }
									// 加载资源包中的资源
									var www = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle("file://" + System.IO.Path.Combine(rootDir, rmanifest.AssetBundlePath));
									www.timeout = timeoutIfCanBeApplied;
									_loader.StartCoroutine(Async(assetPath, ro, www, asyncAction));
								});

								return true;
							}
							else
								return false;

						}) != null);

						if (rm == null)
							asyncAction(true, 1, KSwordKitName + ": 资源不存在！请检查参数 assetPath 是否正确，assetPath=" + assetPath, null);
					}
                    else
						asyncAction(true, 1, KSwordKitName + ": 请先调用 SetResourcePackage 方法设置资源包", null);

					break;
			}
		}

		static IEnumerator Async(string path, ResourceRequest resourceRequest, System.Action<bool, float, string, T> asyncAction)
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
				asyncAction(true, 1, KSwordKitName + ": 资源加载失败或者文件不存在! 请检查参数 assetPath 是否正确, assetPath=" + path, null);
            else
            {
                try
                {
					var t = resourceRequest.asset as T;
					if (t != null)
                    {
						// 资源加载成功后，存入资源缓存中
						CacheDic[path] = t;
						asyncAction(true, 1, null, t);
					}
					else
						asyncAction(true, 1, KSwordKitName + ": 资源加载成功，但该资源无法转换为 " + TypeFullName + " 类型", null);
				}
				catch (System.Exception e)
                {
					asyncAction(true, 1, KSwordKitName + ": 资源加载成功，但该资源无法转换为 " + TypeFullName + " 类型, " + e.Message, null);
				}
			}
		}
		static IEnumerator Async(string path, ResourceObject resourceObject, UnityEngine.Networking.UnityWebRequest unityWebRequest, System.Action<bool, float, string, T> asyncAction)
        {
			var op = unityWebRequest.SendWebRequest();
			while (!op.isDone)
			{
				asyncAction(false, (1f/3f) + (1f/3f) * op.progress, null, null);
				yield return null;
			}
			if (op.progress != 1)
				asyncAction(false, 2f / 3f, null, null);
			yield return null;

			if (string.IsNullOrEmpty(unityWebRequest.error))
            {

				AssetBundleRequest assetBundleRequest = null;
                try
                {
					AssetBundle ab = null;
					if(CacheAssetBunbleDic.ContainsKey(resourceObject.AssetBundleName))
						ab = CacheAssetBunbleDic[resourceObject.AssetBundleName];
					else
                    {
						ab = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(unityWebRequest);
						if (ab != null)
							// 资源包加载成功后，存入资源包缓存中
							CacheAssetBunbleDic[resourceObject.AssetBundleName] = ab;
					}
					assetBundleRequest = ab.LoadAssetAsync(resourceObject.ObjectName);
				}
				catch(System.Exception e)
                {
					asyncAction(true, 1, KSwordKitName + ": 资源加载失败！\n请检查参数 assetPath 是否正确, assetPath=" + path + "\nmessage: "+ e.Message, null);
					yield break;
				}

				float p = 0;
				while (!assetBundleRequest.isDone)
				{
					p = 2f / 3f + (1f / 3f) * assetBundleRequest.progress;
					asyncAction(false, p, null, null);
					yield return null;
				}
				if (assetBundleRequest.progress != 1 || p != 1)
					asyncAction(false, 1, null, null);
				yield return null;

                try
                {
					var t = assetBundleRequest.asset as T;
					if (t != null)
					{
						// 资源加载成功后，存入资源缓存中
						CacheDic[path] = t;
						asyncAction(true, 1, null, t);
					}
					else
						asyncAction(true, 1, KSwordKitName + ": 资源加载成功，但该资源无法转换为 " + TypeFullName + " 类型", null);
				}
				catch(System.Exception e)
                {
					asyncAction(true, 1, KSwordKitName + ": 资源加载成功，但该资源无法转换为 " + TypeFullName + " 类型, " + e.Message, null);
				}
			}
			else
            {
				asyncAction(false, 1, null, null);
				yield return null;
				asyncAction(true, 1, KSwordKitName + ": 资源加载失败或者文件不存在！\n请检查参数 assetPath 是否正确, assetPath=" + path + "\nerror:" + unityWebRequest.error + "\nurl: " + unityWebRequest.url, null);
			}
		}
		static IEnumerator Async(string path, AssetBundleRequest assetBundleRequest, System.Action<bool, float, string, T> asyncAction)
        {
			float p = 0;
			while (!assetBundleRequest.isDone)
			{
				p = 0.6666f + (1f / 3f) * assetBundleRequest.progress;
				asyncAction(false, p, null, null);
				yield return null;
			}
			if (assetBundleRequest.progress != 1 || p != 1)
				asyncAction(false, 1, null, null);
			yield return null;

			if (assetBundleRequest.asset == null)
				asyncAction(true, 1, KSwordKitName + ": 资源加载失败或者文件不存在! 请检查参数 assetPath 是否正确, assetPath=" + path, null);
			else
			{
				try
				{
					var t = assetBundleRequest.asset as T;
					if (t != null)
					{
						// 资源加载成功后，存入资源缓存中
						CacheDic[path] = t;
						asyncAction(true, 1, null, t);
					}
					else
						asyncAction(true, 1, KSwordKitName + ": 资源加载成功，但该资源无法转换为 " + TypeFullName + " 类型", null);
				}
				catch (System.Exception e)
				{
					asyncAction(true, 1, KSwordKitName + ": 资源加载成功，但该资源无法转换为 " + TypeFullName + " 类型, " + e.Message, null);
				}
			}
		}
		static IEnumerator AsyncLoadDependencies(string assetBundleName, UnityEngine.Networking.UnityWebRequest unityWebRequest, System.Action<bool, float, string> action)
        {
			var op = unityWebRequest.SendWebRequest();
			while (!op.isDone)
            {
				action(false, op.progress, null);
				yield return null;
			}
			if (op.progress != 1)
				action(false, 1, null);

			yield return null;
			if (string.IsNullOrEmpty(unityWebRequest.error))
			{
				try
				{
					AssetBundle ab = null;
					if (CacheAssetBunbleDic.ContainsKey(assetBundleName))
						ab = CacheAssetBunbleDic[assetBundleName];
                    else
                    {
						ab = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(unityWebRequest);
						if (ab != null)
							// 依赖加载成功后，存入资源包的缓冲中
							CacheAssetBunbleDic[assetBundleName] = ab;
					}

					if (ab != null)
						action(true, 1, null);
					else
						action(true, 1, "获取依赖资源失败， assetBunbleName=" + assetBundleName);
				}
				catch (System.Exception e)
				{
					action(true, 1, e.Message);
				}
			}
			else
				action(true, 1, unityWebRequest.error);
		}
		static void WaitAsyncLoadDependencies(Func<bool> waitFunc, System.Action completedAction)
        {
			if (waitFunc())
				NextFrame(() =>
				{
					WaitAsyncLoadDependencies(waitFunc, completedAction);
				});
			else
				completedAction();
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
			// 剔除重复元素
			var paths = new List<string>();
			for(var i= 0; i< assetPaths.Length; i++)
            {
				if (!paths.Contains(assetPaths[i]))
					paths.Add(assetPaths[i]);
            }

			float c = paths.Count;
			float cc = 0;
			string error = null;
			var objs = new List<T>();
			for (var i = 0; i < c; i++)
            {
				LoadAsync(paths[i], resourcesLoadingLocation, (isdone, progress, _error, obj) => {

					if (isdone)
                    {
						cc++;
						if (string.IsNullOrEmpty(_error))
							objs.Add(obj);
						else
                        {
							if (string.IsNullOrEmpty(error))
								error = _error;
							else
								error += "\n" + _error;
                        }
						return;
                    }

					asyncAction(false, cc / c + 1f / c * progress, null, null);
				});
			}

			WaitAsyncLoadDependencies(() => c != cc, ()=> {
				asyncAction(true, 1, error, objs.ToArray());
			});
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


