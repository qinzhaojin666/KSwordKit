/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ResourcesAsyncLoader.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-11
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace KSwordKit.Core.ResourcesManagement
{
	/// <summary>
	/// 资源异步加载器
	/// </summary>
    public class ResourcesAsyncLoader : MonoBehaviour, IResourcesAsyncLoader
    {
		public const string KSwordKitName = "KSwordKit";
		public const string ClassName = "ResourcesAsyncLoader";
		public const string AsyncLoader = "asyncloader";

		/// <summary>
		/// 构造函数
		/// <para>参数 resourcesLoadingLocation 的不同将指示实例对象使用不同的加载方式加载资源; 默认值为 Resources </para>
		/// </summary>
		/// <param name="resourcesLoadingLocation">指示函数加载的位置</param>
		public ResourcesAsyncLoader(ResourcesLoadingLocation resourcesLoadingLocation = ResourcesLoadingLocation.Resources)
        {
			_resourcesLoadingLocation = resourcesLoadingLocation;
			if (_asyncLoaderParent == null)
			{
				_asyncLoaderParent = new GameObject(ClassName).transform;
				GameObject.DontDestroyOnLoad(_asyncLoaderParent);
			}
			_asyncLoaderGameObject = new GameObject(AsyncLoader);
			_asyncLoaderGameObject.transform.parent = _asyncLoaderParent;
		}

		private static Transform _asyncLoaderParent;
		private GameObject _asyncLoaderGameObject;

		private int _timeoutIfCanBeApplied;
		/// <summary>
		/// 如果能被应用的话，设置或获取超时时间
		/// <para>当 ResourcesLoadingLocation == ResourcesLoadingLocation.Resources 时，超时时间不能被应用在异步加载操作中。</para>
		/// </summary>
		public int timeoutIfCanBeApplied {
			get { return _timeoutIfCanBeApplied; } 
			set
            {
				_timeoutIfCanBeApplied = value;
				if (_unityWebRequest != null)
					_unityWebRequest.timeout = _timeoutIfCanBeApplied;
            }
		}

		private ResourcesLoadingLocation _resourcesLoadingLocation;
		private UnityEngine.Networking.UnityWebRequest _unityWebRequest;
		/// <summary>
		/// 资源加载的位置
		/// <para>该值在构造函数中传入</para>
		/// </summary>
		public ResourcesLoadingLocation ResourcesLoadingLocation { get { return _resourcesLoadingLocation; } }

		string _assetPath;
		ResourcesRequestAsyncOperation _resourcesRequestAsyncOperation;

		/// <summary>
		/// 根据 assetPath 异步加载资源
		/// <para>参数 asyncAction 是加载资源的异步回调；查看<see cref="ResourcesRequestAsyncOperation"/></para>
		/// </summary>
		/// <param name="assetPath">资源路径</param>
		/// <param name="asyncAction">异步回调；参数是异步结果，内部包含进度、错误信息、加载结果等内容； </param>
		public void LoadAsync(string assetPath, System.Action<ResourcesRequestAsyncOperation> asyncAction)
        {
			_assetPath = assetPath;
			_resourcesRequestAsyncOperation = new ResourcesRequestAsyncOperation();
			_resourcesRequestAsyncOperation._resourcePath = _assetPath;

			_asyncLoaderGameObject.name = "assetPath:" + _assetPath;

			switch(ResourcesLoadingLocation)
            {
				case ResourcesLoadingLocation.Resources:
					
					StartCoroutine(_resourcesRequestAsyncOperation.AsyncByResources(Resources.LoadAsync(_assetPath), asyncAction));
                    
					break;
				case ResourcesLoadingLocation.StreamingAssetsPath:

                    _unityWebRequest = new UnityEngine.Networking.UnityWebRequest(_assetPath);
                    _unityWebRequest.timeout = timeoutIfCanBeApplied;
					StartCoroutine(_resourcesRequestAsyncOperation.AsyncByAssetBundle(_unityWebRequest, asyncAction));

					break;
				case ResourcesLoadingLocation.PersistentDataPath:
					
					_unityWebRequest = new UnityEngine.Networking.UnityWebRequest(_assetPath);
					_unityWebRequest.timeout = timeoutIfCanBeApplied;
					StartCoroutine(_resourcesRequestAsyncOperation.AsyncByAssetBundle(_unityWebRequest, asyncAction));
					
					break;
				case ResourcesLoadingLocation.RemotePath:
					
					_unityWebRequest = new UnityEngine.Networking.UnityWebRequest(_assetPath);
					_unityWebRequest.timeout = timeoutIfCanBeApplied;
					StartCoroutine(_resourcesRequestAsyncOperation.AsyncByAssetBundle(_unityWebRequest, asyncAction));
					
					break;
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
		/// <summary>
		/// 根据指定路径加载指定类型T的资源
		/// <para>参数 asyncAction 是加载资源的异步回调；查看<see cref="ResourcesRequestAsyncOperation"/></para>
		/// </summary>
		/// <typeparam name="T">标识要加载资源的类型</typeparam>
		/// <param name="assetPath">资源的路径</param>
		/// <param name="asyncAction">异步回调；参数是异步结果，内部包含进度、错误信息、加载结果等内容；</param>
		public void LoadAsync<T>(string assetPath, System.Action<ResourcesRequestAsyncOperation> asyncAction) where T : UnityEngine.Object
        {

        }
		/// <summary>
		/// 给定的路径（文件夹或文件）中加载所有资源
		/// <para>默认不会递归遍历子目录内资源</para>
		/// <para>如果想递归目录内所有资源可以使用<seealso cref="LoadAllAsync{T}(string, bool, System.Action{ResourcesRequestAsyncOperation})"/></para>
		/// <para>参数 asyncAction 是异步回调；查看<see cref="ResourcesRequestAsyncOperation{T}"/></para>
		/// </summary>
		/// <param name="assetPath">给定的资源路径（文件夹或文件）</param>
		/// <param name="asyncAction">异步回调；参数是异步结果，内部包含进度、错误信息、加载结果等内容；</param>
		public void LoadAllAsync<T>(string asstPath, System.Action<ResourcesRequestAsyncOperation> asyncAction) where T : UnityEngine.Object
        {

        }
		/// <summary>
		/// 给定的路径（文件夹或文件）中加载所有资源
		/// <para>当参数 deepResources 值为 false时，函数行为和<seealso cref="LoadAllAsync{T}(string, System.Action{ResourcesRequestAsyncOperation})"/>相同</para>
		/// <para>参数 asyncAction 是异步回调；查看<see cref="ResourcesRequestAsyncOperation{T}"/></para>
		/// </summary>
		/// <param name="asstPath">给定的资源路径（文件夹或文件）</param>
		/// <param name="deepResources">指示函数是否遍历子目录所有资源</param>
		/// <param name="asyncAction">异步回调；参数是异步结果，内部包含进度、错误信息、加载结果等内容；</param>
		public void LoadAllAsync<T>(string asstPath, bool deepResources, System.Action<ResourcesRequestAsyncOperation> asyncAction) where T : UnityEngine.Object
        {

        }
	}
}


