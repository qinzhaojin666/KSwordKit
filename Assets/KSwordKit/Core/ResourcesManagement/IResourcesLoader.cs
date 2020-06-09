/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: IResourcesLoader.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-10
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace KSwordKit.Core.ResourcesManagement
{
    /// <summary>
    /// 定义资源加载的几种方法
    /// </summary>
    public interface IResourcesLoader
    {
        /// <summary>
        /// 根据 assetPath 加载资源，resultAction中将加载结果异步回调给用户，加载结果包括进度等信息
        /// 默认资源为 UnityEngine.Object 类型的对象
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="resultAction">资源加载结果</param>
        void LoadAsync(string assetPath, System.Action<ResourcesRequestAsyncOperation> resultAction);
        void LoadAsync(string[] assetPaths, System.Action<ResourcesRequestAsyncOperation> resultAction);
        void LoadAsync<T>(string assetPath, System.Action<ResourcesRequestAsyncOperation<T>> resultAction) where T: UnityEngine.Object;
        void LoadAsync<T>(string[] assetPaths, System.Action<ResourcesRequestAsyncOperation<T>> resultAction) where T : UnityEngine.Object;
        ResourcesRequestAsyncOperation LoadAsync(params string[] assetPaths);
        ResourcesRequestAsyncOperation<T> LoadAsync<T>(params string[] assetPaths) where T : UnityEngine.Object;



        /// <summary>
        /// 根据 assetPath 加载资源，resultAction中将加载结果异步回调给用户
        /// </summary>
        /// <param name="assetPaths">资源路径</param>
        /// <returns>资源加载结果； 加载成功，返回资源对象； 如果加载出错，返回 null</returns>
        UnityEngine.Object Load(params string[] assetPaths);
        T Load<T>(params string[] assetPaths);

        UnityEngine.Object Load(string[] assetPaths, ref string error);
        T Load<T>(string[] assetPaths, ref string error);

        UnityEngine.Object Load(string assetPaths, ref string error);
        T Load<T>(string assetPaths, ref string error);
    }
}

